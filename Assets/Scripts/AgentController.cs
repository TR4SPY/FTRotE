using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using PLAYERTWO.ARPGProject;
using AI_DDA.Assets.Scripts;
using Unity.MLAgents.Policies;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class AgentController : Agent
{
    private Entity entity; // Referencja do Entity
    private EntityAI entityAI; // Referencja do EntityAI
    private float stuckTimer = 0f;
    private Vector3 lastPosition;
    private bool isResetting = false;
    private bool agentWantsToMove = false;
    private float interactionTimeout = 10f; // Maksymalny czas na osiągnięcie celu (w sekundach)
    private float interactionTimer = 0f;
    private float lastAttackTime = -5f; // Ostatni czas ataku
    private float attackCooldown = 2f; // Cooldown ataku (w sekundach)
    private Vector3 wanderTarget; // Docelowy punkt wędrowania
    private float lastWanderChangeTime;
    private GameDatabase gameDatabase;
    private AgentBehaviorLogger agentLogger;
    private EntityAreaScanner scanner;
    private EntitySkillManager skillManager;
    private Vector3? currentExplorationTarget = null; // Pierwotny cel eksploracji
    private HashSet<string> interactedNPCs = new HashSet<string>(); // Lista interakcji
    private HashSet<string> discoveredZones = new HashSet<string>();  
    private HashSet<int> visitedWaypoints = new HashSet<int>();


    public Transform target; // Cel agenta
    public bool isAI = true; // Flaga do identyfikacji jako AI Agent

    public override void Initialize()
    {
        Debug.Log($"Academy environment step count: {Unity.MLAgents.Academy.Instance.StepCount}");
        Debug.Log($"Is Academy Training: {Unity.MLAgents.Academy.Instance.IsCommunicatorOn}");

        var behaviorParameters = GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
        Debug.Log($"Vector Observation Size: {behaviorParameters.BrainParameters.VectorObservationSize}");

        entity = GetComponent<Entity>();
        entityAI = GetComponent<EntityAI>();

        if (entity == null || entityAI == null)
        {
            Debug.LogError("Entity or EntityAI component is missing on the AI Agent!");
        }

        // Ustaw flagę AI Agenta
        entity.isAgent = true;

        // Ustaw domyślne parametry
        if (entity.isAgent)
        {
            entity.moveSpeed = 5f; // Ustaw prędkość tylko dla agenta
            entity.rotationSpeed = 720f;
        }

        gameDatabase = Object.FindFirstObjectByType<GameDatabase>();
        if (gameDatabase == null)
        {
            Debug.LogError("GameDatabase not found in the scene!");
            return;
        }

        var skillManager = GetComponent<EntitySkillManager>();
        if (skillManager != null && skillManager.hands == null)
        {
            var handsPlaceholder = new GameObject("AgentHands").transform;
            handsPlaceholder.parent = transform;
            handsPlaceholder.localPosition = Vector3.zero;
            skillManager.hands = handsPlaceholder;
            Debug.Log("Assigned placeholder hands to Agent AI.");
        }

        agentLogger = GetComponent<AgentBehaviorLogger>();
        if (agentLogger == null)
        {
            Debug.LogError("AgentBehaviorLogger component is missing on the Agent AI!");
            return;
        }

        InitializeAgentStats();
    //  EquipDefaultItems();
        LearnDefaultSkills();

        Debug.Log($"Agent {gameObject.name} initialized with Behavior Name: {GetComponent<BehaviorParameters>().BehaviorName}");
    }

    public override void OnEpisodeBegin()
    {
        // Reset pozycji AI Agenta
        // Vector3 startPosition = new Vector3(Random.Range(-50f, 50f), 0.5f, Random.Range(-50f, 50f));
        // entity.Teleport(startPosition, Quaternion.identity);

        Debug.Log("Episode has started");
        // Debug.Log($"Agent position: {transform.position}");
        // Ustaw nowy cel
        SetRandomTarget();
        Debug.Log($"Target: {target?.name ?? "No target set"}");

        Debug.Log($"Agent {name} initialized. Entity moveSpeed: {entity.moveSpeed}");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Pozycja agenta (3 obserwacje)
        Vector3 agentPosition = transform.position;
        sensor.AddObservation(agentPosition.x);
        sensor.AddObservation(agentPosition.y);
        sensor.AddObservation(agentPosition.z);

        // Pozycja celu i odległość do celu (4 obserwacje)
        if (target != null)
        {
            Vector3 targetPosition = target.position;
            sensor.AddObservation(targetPosition.x);
            sensor.AddObservation(targetPosition.y);
            sensor.AddObservation(targetPosition.z);
            sensor.AddObservation(Vector3.Distance(agentPosition, targetPosition));
        }
        else
        {
            sensor.AddObservation(0f); // Cel x
            sensor.AddObservation(0f); // Cel y
            sensor.AddObservation(0f); // Cel z
            sensor.AddObservation(0f); // Odległość do celu
        }

        // Dodanie najbliższego przeciwnika (4 obserwacje)
        var closestEnemy = GetComponent<EntityAreaScanner>().GetClosestTarget();
        if (closestEnemy != null)
        {
            Vector3 enemyPosition = closestEnemy.position;
            sensor.AddObservation(enemyPosition.x);
            sensor.AddObservation(enemyPosition.y);
            sensor.AddObservation(enemyPosition.z);
            sensor.AddObservation(Vector3.Distance(agentPosition, enemyPosition));
        }
        else
        {
            sensor.AddObservation(0f); // Przeciwnik x
            sensor.AddObservation(0f); // Przeciwnik y
            sensor.AddObservation(0f); // Przeciwnik z
            sensor.AddObservation(0f); // Odległość do przeciwnika
        }

        // Debugowanie
        // Debug.Log("CollectObservations: Agent now observes enemies and target zones.");
    }

    // -------------------------------------------------
    // Główne sterowanie akcji (priorytety)
    // -------------------------------------------------
   public override void OnActionReceived(ActionBuffers actions)
    {
        agentWantsToMove = false;

        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        Vector3 moveDir = new Vector3(moveX, 0f, moveZ).normalized;

        // 1) Priorytet: Jeśli wróg jest w pobliżu → walka!
        if (FindAndAttackEnemy()) return;

        // 2) Jeśli widzi NPC po drodze, wchodzi w interakcję i potem kontynuuje swój cel
        bool isInteracting = false;
        
        if (InteractWithNPC()) return;
        
        if (isInteracting) return; // **Nie wykonuj kolejnych działań, jeśli trwa interakcja**

        // 3) Jeśli idzie do waypointu i zobaczy nową strefę, zmienia cel
        var zone = GetClosestZone();
        if (zone != null && !HasDiscoveredZone(zone.name))
        {
            Debug.Log($"[Agent {name}] Moving to explore zone: {zone.name}");
            target = zone; // Ustawiamy target, żeby uniknąć przypadkowych ponownych odkryć
            DiscoverZone(zone.name);
        }

        // 4) Jeśli idzie do strefy, ale widzi waypoint, najpierw odwiedza waypoint
        if (DiscoverWaypoint()) return;

        // 5) Jeśli nie ma żadnych innych priorytetów, kontynuuje swoją eksplorację
        ContinueExploration(moveDir);
    }

    private IEnumerator RunCoroutineAndSetFlag(IEnumerator coroutine, System.Action onComplete)
    {
        yield return StartCoroutine(coroutine);
        onComplete?.Invoke();
    }

    // -------------------------------------------------
    // Metody walki, interakcji, odkrywania, roamowania
    // -------------------------------------------------

    private void ContinueExploration(Vector3 moveDir)
    {
        if (currentExplorationTarget != null)
        {
            Debug.Log($"[Agent {name}] Continuing exploration towards: {currentExplorationTarget}");
            entity.MoveTo(currentExplorationTarget.Value);
        }
        else
        {
            WanderAround(moveDir); // Jeśli nie ma innego celu, losowa eksploracja
        }
    }

    /// <summary>
    /// Jeśli w pobliżu jest NPC i możemy z nim wejść w interakcję, robimy to i zwracamy true.
    /// </summary>
    private bool InteractWithNPC()
    {
        var scanner = GetComponent<EntityAreaScanner>();
        var closestNPC = scanner?.GetClosestInteractiveObject();

        if (closestNPC != null && closestNPC.CanAgentInteract)
        {
            float distance = Vector3.Distance(transform.position, closestNPC.transform.position);
            if (interactedNPCs.Contains(closestNPC.name))
            {
                Debug.Log($"[Agent {name}] Already interacted with NPC {closestNPC.name}. Skipping.");
                return false;
            }

            if (distance > 2f) 
            {
                entity.MoveTo(closestNPC.transform.position);
                Debug.Log($"[Agent {name}] Moving closer to NPC: {closestNPC.name}");
                return true;
            }
            else
            {
                interactedNPCs.Add(closestNPC.name);
                closestNPC.Interact(entity);
                agentLogger?.LogNpcInteraction();
                SetReward(0.75f); // Nagroda za interakcję z NPC

                if (ShouldEndEpisode())
                {
                    EndAgentEpisode();
                }

                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Jeśli agent stoi blisko Waypointu, odkryj go i zwróć true.
    /// </summary>
    private bool DiscoverWaypoint()
    {
        var scanner = GetComponent<EntityAreaScanner>();
        var closestWaypoint = scanner?.GetClosestWaypoint();

        if (closestWaypoint != null)
        {
            int waypointID = closestWaypoint.GetInstanceID();

            if (visitedWaypoints.Contains(waypointID))
            {
                Debug.Log($"[Agent {name}] Already visited waypoint {closestWaypoint.name}. Skipping.");
                return false;
            }

            float distance = Vector3.Distance(transform.position, closestWaypoint.transform.position);

            if (distance > 3f) 
            {
                entity.MoveTo(closestWaypoint.transform.position);
                Debug.Log($"[Agent {name}] Moving closer to waypoint: {closestWaypoint.name}");
                return true;
            }
            else
            {
                visitedWaypoints.Add(waypointID);
                agentLogger?.LogWaypointDiscovery(waypointID);
                SetReward(0.5f); // Nagroda za odkrycie waypointu

                if (ShouldEndEpisode())
                {
                    EndAgentEpisode();
                }

                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Jeżeli nic innego nie robimy, agent wędruje. 
    /// W tym wypadku używamy wektora z ML-Agents lub losowego, jeśli za mały.
    /// </summary>
    private void WanderAround(Vector3 moveDir)
    {
        float minDistance = 5f; // Minimalna odległość nowego celu od obecnej pozycji
        float wanderRadius = 20f; // Maksymalny promień wędrowania

        // Jeśli agent nie ma celu lub już osiągnął poprzedni, losujemy nowy cel
        if (wanderTarget == Vector3.zero || Vector3.Distance(transform.position, wanderTarget) < 2f)
        {
            Debug.Log($"[Agent {name}] Choosing a new wander target...");

            Vector3 newTarget;
            int attempts = 0;

            do
            {
                newTarget = transform.position + new Vector3(
                    Random.Range(-wanderRadius, wanderRadius),
                    0,
                    Random.Range(-wanderRadius, wanderRadius)
                );
                attempts++;
            }
            while (Vector3.Distance(transform.position, newTarget) < minDistance && attempts < 10);

            wanderTarget = newTarget;
            lastWanderChangeTime = Time.time;

            Debug.Log($"[Agent {name}] New wander target set: {wanderTarget}");
        }

        // Jeśli Agent AI nie osiągnął jeszcze celu, kontynuuje ruch
        if (Vector3.Distance(transform.position, wanderTarget) > 2f)
        {
            Debug.Log($"[Agent {name}] Moving to {wanderTarget}");
            entity.MoveTo(wanderTarget);
        }
        else
        {
            Debug.Log($"[Agent {name}] Reached wander target, selecting a new one...");
            wanderTarget = Vector3.zero; // Natychmiastowe losowanie nowego celu
        }
    }

    private Transform GetClosestZone()
    {
        // np. "ZoneTrigger" obiekty
        var zones = Object.FindObjectsByType<ZoneTrigger>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (zones.Length == 0) return null;

        Transform best = null;
        float bestDist = Mathf.Infinity;
        foreach (var z in zones)
        {
            float d = Vector3.Distance(transform.position, z.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = z.transform;
            }
        }
        return best;
    }

    private bool FindAndAttackEnemy()
    {
        var scanner = GetComponent<EntityAreaScanner>();
        var closestEnemy = scanner?.GetClosestTarget();

        if (closestEnemy != null && closestEnemy.CompareTag("Entity/Enemy"))
        {
            float distanceToEnemy = Vector3.Distance(transform.position, closestEnemy.position);
            if (distanceToEnemy <= 1.5f) // Zasięg ataku
            {
                PerformAttack(closestEnemy);
                return true; // Atak wykonany
            }
            else
            {
                agentWantsToMove = true;
                entity.MoveTo(closestEnemy.position); // Zbliż się do przeciwnika
                ResumeExplorationAfterCombat(); // Po walce agent wraca do eksploracji
                return true;
            }
        }

        // return false; // Brak przeciwników
        SetReward(-1.0f); // Kara za brak walki
        Debug.Log($"[Agent {name}] Penalty -1.0 for not engaging in combat.");
        return false;
    }

    private void ResumeExplorationAfterCombat()
    {
        if (currentExplorationTarget != null)
        {
            Debug.Log($"[Agent {name}] Resuming exploration towards: {currentExplorationTarget}");
            entity.MoveTo(currentExplorationTarget.Value);
        }
    }

    private void PerformAttack(Transform enemy)
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        if (enemy == null || enemy.GetComponent<Entity>().isDead)
        {
            Debug.Log($"[Agent {name}] Attack failed: target is null or already dead.");
            return;
        }

        var targetEntity = enemy.GetComponent<Entity>();

        if (skillManager != null && skillManager.CanUseSkill())
        {
            Debug.Log($"[Agent {name}] Using skill on {enemy.name}");
            skillManager.PerformSkill(); // Usunięcie argumentu, bo funkcja go nie wymaga

        }
        else
        {
            Debug.Log($"[Agent {name}] Attacking {enemy.name} with basic attack!");
            targetEntity.Damage(entity, 10, false);
        }

        lastAttackTime = Time.time;

        // **Nowe sprawdzenie, czy wróg rzeczywiście umarł**
        StartCoroutine(WaitForEnemyDeath(targetEntity));
    }

    private IEnumerator WaitForEnemyDeath(Entity targetEntity)
    {
        float timeout = 2f; // Maksymalnie 2 sekundy czekania
        float elapsed = 0f;

        while (!targetEntity.isDead && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null; // Poprawne oczekiwanie na zakończenie warunku
        }

        if (targetEntity.isDead)
        {
            Debug.Log($"[Agent {name}] Killed enemy: {targetEntity.name}");
            agentLogger?.LogEnemyKilled(targetEntity.name);

            // **Dodajemy nagrodę za pokonanie wroga**
            SetReward(1.5f);
            Debug.Log($"[Agent {name}] Reward +1.5 for killing an enemy.");
        }
    }

    private void SetRandomTarget()
    {
        var targets = Object.FindObjectsByType<ZoneTrigger>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Debug.Log($"Found {targets.Length} ZoneTriggers.");

        if (targets.Length > 0)
        {
            target = targets[Random.Range(0, targets.Length)].transform;
            Debug.Log($"Target set to: {target.name}");
        }
        else
        {
            Debug.LogWarning("No ZoneTriggers found. Cannot set target.");
            EndAgentEpisode(); // Zamiast EndEpisode()
            return;
        }

        if (target != null)
        {
            entity.SetTarget(target);
        }
    }

    public string GetTargetName()
    {
        return target != null ? target.name : "";
    }

    public void DiscoverZone(string zoneName, bool confirmedByTrigger = false)
    {
        if (HasDiscoveredZone(zoneName))
        {
            Debug.Log($"[Agent {name}] Zone {zoneName} already discovered. Skipping.");
            return;
        }

        Debug.Log($"[Agent {name}] Discovering new zone: {zoneName}");

        discoveredZones.Add(zoneName);
        agentLogger?.LogAgentZoneDiscovery(zoneName);

        Debug.Log($"[Agent {name}] Successfully logged zone discovery: {zoneName}");

        SetReward(1.0f); // Nagroda za odkrycie

        // Jeśli strefa została odkryta przez AI (OnActionReceived), nie zmieniamy celu
        if (!confirmedByTrigger)
        {
            SetRandomTarget();
        }
    }

    public bool HasDiscoveredZone(string zoneName)
    {
        return discoveredZones.Contains(zoneName);
    }

    private void InitializeAgentStats()
    {
        var statsManager = GetComponent<EntityStatsManager>();
        if (statsManager != null)
        {
            statsManager.level = 10;
            statsManager.strength = 50;
            statsManager.dexterity = 58;
            statsManager.vitality = 52;
            statsManager.energy = 55;
            statsManager.health = 500;
            //statsManager.mana = 500;
            statsManager.Revitalize(); // Upewnia się, że mana jest na max
            statsManager.Initialize();

            Debug.Log($"[InitializeAgentStats] Stats initialized for Agent AI: Health={statsManager.health}, Mana={statsManager.mana}");
        }
        else
        {
            Debug.LogError("[InitializeAgentStats] EntityStatsManager is missing on Agent AI!");
        }
    }

    private void LearnDefaultSkills()
    {
        var skillManager = GetComponent<EntitySkillManager>();
        if (skillManager != null)
        {
            // Przykład: Dodanie umiejętności do agenta
            foreach (var skill in skillManager.skills)
            {
                Debug.Log($"Skill '{skill.name}' assigned to Agent AI.");
            }

            Debug.Log("Agent AI successfully initialized with skills.");
        }
        else
        {
            Debug.LogError("EntitySkillManager is missing on Agent AI.");
        }
    }

    private void EndAgentEpisode()
    {
        if (!ShouldEndEpisode()) // Jeśli agent nie spełnił jeszcze wszystkich celów, nie kończymy epizodu
        {
            Debug.Log($"[Agent {name}] Episode should not end yet. Continuing.");
            return;
        }

        Debug.Log($"[Agent {name}] All exploration goals met. Ending episode...");

        if (agentLogger != null)
        {
            Debug.Log("[AgentController] Saving agent logs...");
            agentLogger.SaveAgentLogsToFile();
        }
        else
        {
            Debug.LogError("[AgentController] AgentBehaviorLogger instance is NULL! Cannot save logs.");
        }

        EndEpisode();
    }

/*
    private bool ShouldEndEpisode()
    {
        return (discoveredZones.Count >= totalZones &&
                visitedWaypoints.Count >= totalWaypoints &&
                interactedNPCs.Count >= totalNPCs);
    }
*/
    private bool ShouldEndEpisode()
    {
        bool allZonesDiscovered = discoveredZones.Count >= Object.FindObjectsByType<ZoneTrigger>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
        bool allWaypointsDiscovered = visitedWaypoints.Count >= Object.FindObjectsByType<Waypoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
        bool allNPCsInteracted = interactedNPCs.Count >= Object.FindObjectsByType<NpcInteractionLogger>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;

        Debug.Log($"[Agent {name}] Checking end conditions: Zones {discoveredZones.Count}/{Object.FindObjectsByType<ZoneTrigger>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length}, Waypoints {visitedWaypoints.Count}/{Object.FindObjectsByType<Waypoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length}, NPCs {interactedNPCs.Count}/{Object.FindObjectsByType<NpcInteractionLogger>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length}");

        return allZonesDiscovered && allWaypointsDiscovered && allNPCsInteracted;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Debug.Log("Agent enabled.");
    }

    protected override void OnDisable()
    {
        Debug.Log("Agent disabled.");
        base.OnDisable();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle")) // Przeszkody muszą mieć tag "Obstacle"
        {
            Debug.Log($"Obstacle detected: {collision.gameObject.name}. Avoiding obstacle.");
            Vector3 avoidDirection = (transform.position - collision.contacts[0].point).normalized;
            agentWantsToMove = true;
            entity.MoveTo(transform.position + avoidDirection * 3f); // Przesunięcie o 3 jednostki od przeszkody
        }
    }

    private void CheckIfStuck()
    {
        float movementThreshold = 0.5f; // Większy próg ruchu
        float stuckCheckTime = 5f; // Czas, po którym uznajemy, że agent utknął

        if (!agentWantsToMove)
        {
            stuckTimer = 0f;
            lastPosition = transform.position;
            return;
        }

        float distMoved = Vector3.Distance(transform.position, lastPosition);
        if (distMoved < movementThreshold && agentWantsToMove)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > stuckCheckTime)
            {
                Debug.Log($"[Agent {name}] Detected as stuck! Choosing new direction.");

                stuckTimer = 0f;
                wanderTarget = Vector3.zero; // Resetowanie celu
                WanderAround(new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized);
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = transform.position;
    }

    private void CheckInteractionTimeout()
    {
        if (isResetting) return;

        if (target != null)
        {
            interactionTimer += Time.deltaTime;

            if (interactionTimer > interactionTimeout)
            {
                Debug.Log($"Interaction target timed out: {target.name}. Selecting new target.");
                isResetting = true;
                target = null; // Resetowanie celu
                interactionTimer = 0f;
                SetRandomTarget();
                isResetting = false;
            }
        }
        else
        {
            interactionTimer = 0f;
        }
    }

    private void HandleAgentDeath()
    {
        if (entity != null && entity.isDead)
        {
        Debug.Log($"[Agent AI] {entity.name} has died. Logging death...");
        // 2. Kara za śmierć (agent powinien unikać umierania)
        SetReward(-2.0f); 

        if (agentLogger != null)
        {
            agentLogger.LogAgentDeath(entity);
            Debug.Log($"[Agent AI] Death logged successfully.");
        }
        else
        {
            Debug.LogError("[Agent AI] AgentBehaviorLogger instance is NULL! Death log failed.");
        }

            Revive(); // Opóźnienie przed odrodzeniem
            EndEpisode();
        }
    }

    private void Revive()
    {
        Debug.Log($"[Agent {name}] Reviving...");
        //entity.isDead = false;
        entity.Revive();
        entity.stats.health = entity.stats.maxHealth;
            
        Vector3 spawnPosition = new Vector3(Random.Range(-50f, 50f), 1f, Random.Range(-50f, 50f));
        entity.Teleport(spawnPosition, Quaternion.identity);

        entity.SetCollidersEnabled(true);
        Debug.Log($"[Agent {name}] Revived with {entity.stats.health} HP at {spawnPosition}.");
    }

/*
    private void DebugAgentState()
    {
        Debug.Log($"Agent Position: {transform.position}, Target: {target?.name ?? "None"}, StuckTimer: {stuckTimer}, InteractionTimer: {interactionTimer}");
    }
*/

    void Update()
    {
        if (!entity.isAgent) return; // Tylko Agent AI może się poruszać tutaj
        //Debug.Log($"[Agent {name}] Velocity: {entity.velocity}, MoveSpeed: {entity.moveSpeed}");
        var statsManager = GetComponent<EntityStatsManager>();
    if (statsManager != null)
    {
        // Debug.Log($"[Agent {name}] Health: {statsManager.health}/{statsManager.maxHealth}");
    }
        CheckIfStuck();
        CheckInteractionTimeout();

        if (Time.frameCount % 30 == 0) // Log every 30 frames
        {
            // Debug.Log($"Frame: {Time.frameCount}, StepCount: {Academy.Instance.StepCount}");
        }
        HandleAgentDeath();
        RequestDecision();
    }
}
