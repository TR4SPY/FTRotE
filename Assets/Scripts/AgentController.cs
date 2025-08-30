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
    private Entity entity;
    private EntityAI entityAI;
    //private float stuckTimer = 0f;
    //private Vector3 lastPosition;
    private bool isResetting = false;
    private bool agentWantsToMove = false;
    private float interactionTimeout = 10f;
    private float interactionTimer = 0f;
    private float lastAttackTime = -5f;
    private float attackCooldown = 2f;
    private Vector3 wanderTarget;
    private float combatStartTime = 0f;
    private float totalCombatTime = 0f;
    private float combatTimeInEpisode = 0f; 
    private bool inCombat = false;
    private float lastWanderChangeTime;
    private GameDatabase gameDatabase;
    private AgentBehaviorLogger agentLogger;
    private EntityAreaScanner scanner;
    private EntitySkillManager skillManager;
    private Vector3? currentExplorationTarget = null;
    private HashSet<string> interactedNPCs = new HashSet<string>();
    private HashSet<string> discoveredZones = new HashSet<string>();  
    private HashSet<int> visitedWaypoints = new HashSet<int>();
    private float inactivityTimer = 0f;
    private const float inactivityThreshold = 60f;
    private int actionsTaken = 0; 
    //private const int maxActionsPerEpisode = 1000;
    private int maxZones;
    private int maxNPCs;
    private int  maxWaypoints; 
private int collisionCount = 0;
private float lastCollisionTime = 0f; 

private Vector3 stuckCheckLastPosition;
private int stuckCounter = 0;
private const int stuckThreshold = 50;
private const float stuckDistanceThreshold = 0.1f;

    public Transform target;
    public bool isAI = true;

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

        entity.isAgent = true;

        if (entity.isAgent)
        {
            entity.moveSpeed = 5f;
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
        ResetAgent();
        InitializeDynamicLimits();

        // Vector3 startPosition = new Vector3(Random.Range(-50f, 50f), 0.5f, Random.Range(-50f, 50f));
        // entity.Teleport(startPosition, Quaternion.identity);

        Debug.Log("Episode has started");

        // Debug.Log($"Agent position: {transform.position}");
        SetRandomTarget();

        // Debug.Log($"Target: {target?.name ?? "No target set"}");
        // Debug.Log($"Agent {name} initialized. Entity moveSpeed: {entity.moveSpeed}");
    }
    
    private void ResetAgent()
    {
        interactedNPCs.Clear();         // Reset interakcji z NPC
        discoveredZones.Clear();        // Reset odkrytych stref
        visitedWaypoints.Clear();       // Reset odwiedzonych waypointów
        inactivityTimer =0f;            // Reset licznika nieaktywności
        inCombat = false;               // Agent nie jest w walce na starcie epizodu
    }

    private void InitializeDynamicLimits()
    {
        maxZones = LevelZones.instance?.zones.Count ?? 0;
        maxWaypoints = Object.FindObjectsByType<Waypoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
        maxNPCs = Object.FindObjectsByType<Interactive>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                    .Count(npc => npc.CanAgentInteract);

        Debug.Log($"Limity na epizod: Zones={maxZones}, Waypoints={maxWaypoints}, NPCs={maxNPCs}");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 agentPosition = transform.position;
        sensor.AddObservation(agentPosition.x);
        sensor.AddObservation(agentPosition.y);
        sensor.AddObservation(agentPosition.z);

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

        // Debug.Log("CollectObservations: Agent now observes enemies and target zones.");
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        agentWantsToMove = false;

        actionsTaken++;

        inactivityTimer = 0f;

        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        Vector3 moveDir = new Vector3(moveX, 0f, moveZ).normalized;

        if (FindAndAttackEnemy()) return;
        if (InteractWithNPC()) return;

        var zone = GetClosestZone();
        if (zone != null && !HasDiscoveredZone(zone.name))
        {
            target = zone;
            DiscoverZone(zone.name);
            inactivityTimer = 0f;
        }

        if (DiscoverWaypoint())
        {
            inactivityTimer = 0f;
            return;
        }

        ContinueExploration(moveDir);
        
        inactivityTimer += Time.deltaTime;

        if (ShouldEndEpisode())
        {
            Debug.Log($"[Agent {name}] Ending episode due to conditions met.");
            EndEpisode();
        }
    }

    private IEnumerator RunCoroutineAndSetFlag(IEnumerator coroutine, System.Action onComplete)
    {
        yield return StartCoroutine(coroutine);
        onComplete?.Invoke();
    }

    private void ContinueExploration(Vector3 moveDir)
    {
        if (currentExplorationTarget != null)
        {
           // Debug.Log($"[Agent {name}] Continuing exploration towards: {currentExplorationTarget}");
            entity.MoveTo(currentExplorationTarget.Value);
        }
        else
        {
            WanderAround(moveDir);
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
                AddReward(-0.05f);
                return false;
            }

            if (distance > 2f)
            {
                entity.MoveTo(closestNPC.transform.position);
                return true;
            }
            else
            {
                interactedNPCs.Add(closestNPC.name);
                inactivityTimer = 0f;

                closestNPC.Interact(entity);
                agentLogger?.LogNpcInteraction();

                AddReward(1.0f);
                Debug.Log($"[Agent {name}] Successfully interacted with {closestNPC.name}. Reward: +1.0");

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
            Waypoint waypoint = closestWaypoint.GetComponent<Waypoint>();

            if (waypoint == null)
            {
                return false;
            }

            int waypointID = waypoint.waypointID;

            if (visitedWaypoints.Contains(waypointID))
            {
                AddReward(-0.05f);
                Debug.Log($"[Agent {name}] Penalty for revisiting waypoint {waypointID}: -0.1");
                return false;
            }

            float distance = Vector3.Distance(transform.position, closestWaypoint.transform.position);

            if (distance > 3f)
            {
                entity.MoveTo(closestWaypoint.transform.position);
                return true;
            }
            else
            {
                visitedWaypoints.Add(waypointID);
                inactivityTimer = 0f;

                if (!agentLogger.discoveredWaypoints.Contains(waypointID))
                {
                    agentLogger.LogWaypointDiscovery(waypointID);
                }

                AddReward(0.4f);
                Debug.Log($"[Agent {name}] Discovered waypoint {waypointID}. Reward: +0.4");

                int totalWaypoints = Object.FindObjectsByType<Waypoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
                if (visitedWaypoints.Count == totalWaypoints)
                {
                    AddReward(1.0f);
                    Debug.Log($"[Agent {name}] All waypoints discovered. Bonus reward: +1.0");
                }

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
        float minDistance = 5f;
        float wanderRadius = 20f;

        if (wanderTarget == Vector3.zero || Vector3.Distance(transform.position, wanderTarget) < 2f)
        {
           // Debug.Log($"[Agent {name}] Choosing a new wander target...");

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

          //  Debug.Log($"[Agent {name}] New wander target set: {wanderTarget}");
        }

        if (Vector3.Distance(transform.position, wanderTarget) > 2f)
        {
           // Debug.Log($"[Agent {name}] Moving to {wanderTarget}");
            entity.MoveTo(wanderTarget);
        }
        else
        {
           // Debug.Log($"[Agent {name}] Reached wander target, selecting a new one...");
            wanderTarget = Vector3.zero;
        }
    }

    private Transform GetClosestZone()
    {
        var zone = LevelZones.instance?.GetClosestZone(transform.position);
        return zone ? zone.transform : null;
    }

    private bool FindAndAttackEnemy()
    {
        var scanner = GetComponent<EntityAreaScanner>();
        var closestEnemy = scanner?.GetClosestTarget();

        if (closestEnemy != null && closestEnemy.CompareTag("Entity/Enemy"))
        {
            float distanceToEnemy = Vector3.Distance(transform.position, closestEnemy.position);

            if (!inCombat)
            {
                combatStartTime = Time.time;
                inCombat = true;
                inactivityTimer = 0f;
                agentLogger?.StartCombat();
            }

            if (distanceToEnemy <= 1.5f)
            {
                PerformAttack(closestEnemy);
                return true;
            }
            else
            {
                agentWantsToMove = true;
                entity.MoveTo(closestEnemy.position);
                return true;
            }
        }

        if (inCombat && closestEnemy == null)
        {
            float combatDuration = Time.time - combatStartTime;
            totalCombatTime += combatDuration;
            combatTimeInEpisode += combatDuration;
            inCombat = false;

            agentLogger?.EndCombat(combatDuration);
            Debug.Log($"[Agent {name}] Exited combat. Duration: {combatDuration}s | Total Combat Time: {totalCombatTime}s | Combat Time in Episode: {combatTimeInEpisode}s");
        }

        return false;
    }

    private void ResumeExplorationAfterCombat()
    {
        if (currentExplorationTarget != null)
        {
           // Debug.Log($"[Agent {name}] Resuming exploration towards: {currentExplorationTarget}");
            entity.MoveTo(currentExplorationTarget.Value);
        }
    }

    private void PerformAttack(Transform enemy)
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        if (enemy == null || enemy.GetComponent<Entity>().isDead)
        {
           // Debug.Log($"[Agent {name}] Attack failed: target is null or already dead.");
            return;
        }

        var targetEntity = enemy.GetComponent<Entity>();

        if (skillManager != null && skillManager.CanUseSkill())
        {
          //  Debug.Log($"[Agent {name}] Using skill on {enemy.name}");
            skillManager.PerformSkill();
        }
        else
        {
           // Debug.Log($"[Agent {name}] Attacking {enemy.name} with basic attack!");
            targetEntity.Damage(entity, 10, false);
        }

        lastAttackTime = Time.time;

        StartCoroutine(WaitForEnemyDeath(targetEntity));
    }
    private IEnumerator WaitForEnemyDeath(Entity targetEntity)
    {
        float timeout = 2f;
        float elapsed = 0f;

        while (!targetEntity.isDead && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (targetEntity.isDead)
        {
            if (!targetEntity.isAgent)
            {
                Debug.Log($"[Agent {name}] Killed enemy: {targetEntity.name}");
                agentLogger?.LogEnemyKilled(targetEntity.name);

                AddReward(1.0f); 
                inactivityTimer = 0f;
                Debug.Log($"[Agent {name}] Reward +1.5 for killing an enemy.");
            }
            else
            {
                Debug.LogWarning($"[Agent {name}] Attempted to log own death as enemy kill. Skipping...");
            }
        }
    }

    private void SetRandomTarget()
    {
        var targets = LevelZones.instance?.zones;
        int count = targets?.Count ?? 0;
        Debug.Log($"Found {count} ZoneTriggers.");

        if (count > 0)
        {
            target = targets[Random.Range(0, count)].transform;
            Debug.Log($"Target set to: {target.name}");
        }
        else
        {
            Debug.LogWarning("No ZoneTriggers found. Cannot set target.");
            EndAgentEpisode();
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
            Debug.Log($"[Agent {name}] Zone '{zoneName}' already discovered. Skipping.");
            AddReward(-0.1f);
            return;
        }

        Debug.Log($"[Agent {name}] Discovering new zone: {zoneName}");

        discoveredZones.Add(zoneName);
        inactivityTimer = 0f;

        agentLogger?.LogAgentZoneDiscovery(zoneName);
        AddReward(0.75f);

        if (!confirmedByTrigger)
        {
            SetRandomTarget();
        }

        if (ShouldEndEpisode())
        {
            EndAgentEpisode();
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
            statsManager.Revitalize();
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
        if (!ShouldEndEpisode()) 
        {
            Debug.Log($"[Agent {name}] Episode should not end yet. Continuing.");
            return;
        }

        Debug.Log($"[Agent {name}] All exploration goals met. Ending episode...");

        if (inCombat)
        {
            float combatDuration = Time.time - combatStartTime;
            totalCombatTime += combatDuration;
            combatTimeInEpisode += combatDuration;
            inCombat = false;

            Debug.Log($"[Agent {name}] Exited combat before ending episode. Final Combat Duration: {combatDuration}s | Total Combat Time: {totalCombatTime}s | Combat Time in Episode: {combatTimeInEpisode}s");
        }

        agentLogger?.LogCombatTime(totalCombatTime);
        agentLogger?.LogCombatTimePerEpisode(combatTimeInEpisode);

        Debug.Log($"[Agent {name}] Total Combat Time logged: {totalCombatTime} seconds (Global)");
        Debug.Log($"[Agent {name}] Combat Time in Episode logged: {combatTimeInEpisode} seconds (Per Episode)");

        combatTimeInEpisode = 0f;

        if (agentLogger != null)
        {
            int oldValue = agentLogger.episodeCount;
            agentLogger.episodeCount++;
            agentLogger.SaveLogs(oldValue, agentLogger.episodeCount);
            Debug.Log($"[Agent {name}] Episode Count: {agentLogger.episodeCount}");
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
        bool allZonesDiscovered = discoveredZones.Count >= maxZones;
        bool allNPCsInteracted = interactedNPCs.Count >= maxNPCs;
        bool allWaypointsDiscovered = visitedWaypoints.Count >= maxWaypoints;
        bool inactivityExceeded = inactivityTimer >= inactivityThreshold;

        Debug.Log($"[Agent {name}] Checking end conditions: Zones {discoveredZones.Count}/{maxZones}, Waypoints {visitedWaypoints.Count}/{maxWaypoints}, NPCs {interactedNPCs.Count}/{maxNPCs}, Inactivity: {inactivityTimer}/{inactivityThreshold}");

        return (allZonesDiscovered && allWaypointsDiscovered && allNPCsInteracted) || inactivityExceeded;
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
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log($"[Agent {name}] Obstacle detected: {collision.gameObject.name}. Adjusting path...");

            collisionCount++;
            lastCollisionTime = Time.time;

            Vector3 escapeDirection = (transform.position - collision.contacts[0].point).normalized;

            float escapeDistance = Mathf.Clamp(3f + collisionCount * 1.5f, 3f, 10f);

            if (collisionCount > 3)
            {
                escapeDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                collisionCount = 0;
            }

            entity.MoveTo(transform.position + escapeDirection * escapeDistance);
            agentWantsToMove = true;
        }
    }

    private void CheckIfStuck()
    {
        float distanceMoved = Vector3.Distance(transform.position, stuckCheckLastPosition);

        if (distanceMoved < stuckDistanceThreshold)
        {
            stuckCounter++;

            if (stuckCounter > stuckThreshold)
            {
                Debug.Log($"[Agent {name}] Stuck detected! Attempting to escape...");

                Vector3 escapeDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                entity.MoveTo(transform.position + escapeDirection * 5f);

                stuckCounter = 0;
            }
        }
        else
        {
            stuckCounter = 0;
        }

        stuckCheckLastPosition = transform.position;
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
                target = null;
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

    private bool deathLogged = false;

    private void HandleAgentDeath()
    {
        if (entity != null && entity.isDead)
        {
            Debug.Log($"[Agent AI] {entity.name} has died. Logging death...");

            AddReward(-1.5f);

            if (agentLogger != null)
            {
                agentLogger.LogAgentDeath(entity);
                Debug.Log($"[Agent AI] Death logged successfully.");
                agentLogger.episodeCount++;
            }

            if (ShouldEndEpisode())
            {
                Debug.Log($"[Agent {name}] Agent has died, but all objectives are complete. Ending episode.");
                EndAgentEpisode();
            }
            else
            {
                Revive();
            }
        }
    }

    private void Revive()
    {
        Debug.Log($"[Agent {name}] Reviving...");
        entity.Revive();
        entity.stats.health = entity.stats.maxHealth;

        Vector3 spawnPosition = new Vector3(Random.Range(-50f, 50f), 1f, Random.Range(-50f, 50f));
        entity.Teleport(spawnPosition, Quaternion.identity);

        entity.SetCollidersEnabled(true);
        agentLogger?.ResetAgentDeathLog();

        Debug.Log($"[Agent {name}] Revived with {entity.stats.health} HP at {spawnPosition}.");
    }

/*
    private void DebugAgentState()
    {
        Debug.Log($"Agent Position: {transform.position}, Target: {target?.name ?? "None"}, StuckTimer: {stuckTimer}, InteractionTimer: {interactionTimer}");
    }
*/
    private void CheckInactivityPenalty()
    {
        inactivityTimer += Time.deltaTime;

        if (inactivityTimer >= 30f && inactivityTimer < 60f)
        {
            AddReward(-0.5f);
            Debug.Log($"[Agent {name}] Inactivity penalty applied: -0.5 after 30 seconds.");
        }

        if (inactivityTimer >= 60f)
        {
            AddReward(-1.0f);
            Debug.Log($"[Agent {name}] Inactivity penalty applied: -1.0 after 60 seconds. Ending episode.");
            EndEpisode();
        }
    }

    void Update()
    {
        if (!entity.isAgent) return;
        //Debug.Log($"[Agent {name}] Velocity: {entity.velocity}, MoveSpeed: {entity.moveSpeed}");
        var statsManager = GetComponent<EntityStatsManager>();
    if (statsManager != null)
    {
        // Debug.Log($"[Agent {name}] Health: {statsManager.health}/{statsManager.maxHealth}");
    }
        CheckIfStuck();
        CheckInteractionTimeout();
        CheckInactivityPenalty();

        if (Time.frameCount % 30 == 0)
        {
            // Debug.Log($"Frame: {Time.frameCount}, StepCount: {Academy.Instance.StepCount}");
        }
        HandleAgentDeath();
        RequestDecision();
    }
}
