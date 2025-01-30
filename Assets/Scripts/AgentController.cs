using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using PLAYERTWO.ARPGProject;
using AI_DDA.Assets.Scripts;
using Unity.MLAgents.Policies;
using System.Linq;
using System.Collections.Generic;

public class AgentController : Agent
{
    [SerializeField] private float stuckDistanceThreshold = 0.5f; // minimalny ruch
    [SerializeField] private float stuckTimeThreshold = 6f;       // sekundy
    private Entity entity; // Referencja do Entity
    private EntityAI entityAI; // Referencja do EntityAI
    private Interactive lastInteractedObject; // Przechowuje ostatnio użyty obiekt interaktywny
    private float lastInteractionTime = 0f;
    private float interactionCooldown = 5f; // Czas w sekundach
    private float stuckTimer = 0f;
    private Vector3 lastPosition;
    private bool isResetting = false;
    private bool agentWantsToMove = false;
    private float interactionTimeout = 10f; // Maksymalny czas na osiągnięcie celu (w sekundach)
    private float interactionTimer = 0f;
    private float lastAttackTime = -5f; // Ostatni czas ataku
    private float attackCooldown = 2f; // Cooldown ataku (w sekundach)
    private const float stuckThreshold = 2f; // Czas w sekundach, po którym agent jest uznany za zablokowanego
    private Transform targetEnemy; // Przechowuje referencję do najbliższego przeciwnika
    private GameDatabase gameDatabase;
    private AgentBehaviorLogger agentLogger;
    private HashSet<string> interactedNPCs = new HashSet<string>(); // Lista interakcji
        private EntityAreaScanner scanner;

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
        Vector3 startPosition = new Vector3(Random.Range(-50f, 50f), 0.5f, Random.Range(-50f, 50f));
        entity.Teleport(startPosition, Quaternion.identity);

        Debug.Log("Episode has started");
        Debug.Log($"Agent position: {transform.position}");
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

        // Wyciągamy wektor ruchu z ML-Agents
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        Vector3 moveDir = new Vector3(moveX, 0f, moveZ).normalized;

        // 1) Walka jest priorytetem
        if (FightEnemyIfInRange()) return;

        // 2) Interakcja z NPC (jeżeli jest blisko)
        if (InteractWithNPCIfInRange()) return;

        // 3) Odkrycie Waypointu (jeżeli stoimy tuż obok)
        if (DiscoverWaypointIfClose()) return;

        // 4) Odkrycie Strefy (zone) (jeżeli stoimy tuż obok)
        if (DiscoverZoneIfClose()) return;

        // 5) Jeśli nic innego się nie dzieje, poruszaj się losowo
        WanderAround(moveDir);
    }

    // -------------------------------------------------
    // Metody walki, interakcji, odkrywania, roamowania
    // -------------------------------------------------

    /// <summary>
    /// Jeśli w zasięgu jest wróg, atakujemy go i zwracamy true.
    /// </summary>
    private bool FightEnemyIfInRange()
    {
        // Szukamy najbliższego wroga, np. za pomocą skanera
        Transform closestEnemy = scanner?.GetClosestTarget();
        if (closestEnemy == null) return false;

        // Sprawdź dystans
        float dist = Vector3.Distance(transform.position, closestEnemy.position);
        if (dist < 10f) // Przykładowy zasięg "widzenia" / ataku
        {
            // Podejdź i zaatakuj
            agentWantsToMove = true;
            entity.MoveToAttack(closestEnemy);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Jeśli w pobliżu jest NPC i możemy z nim wejść w interakcję, robimy to i zwracamy true.
    /// </summary>
    private bool InteractWithNPCIfInRange()
    {
        // Zakładam, że scanner ma metodę GetClosestInteractiveObject(), 
        // i NPC mają np. 'CanAgentInteract = true'
        var npc = scanner?.GetClosestInteractiveObject();
        if (npc == null) return false;

        float dist = Vector3.Distance(transform.position, npc.transform.position);
        // Załóżmy, że 3f to dystans, z którego możemy zacząć interakcję
        if (dist <= 3f && npc.CanAgentInteract)
        {
            // Interakcja
            agentWantsToMove = true;
            entity.MoveTo(npc.transform.position);
            // Gdy jesteśmy bardzo blisko (1.5f), npc.Interact(entity) 
            // może zostać wywołane wewnątrz OnTriggerEnter, etc.
            // Lub:
            // if (dist < 1.5f) npc.Interact(entity);

            // Log
            agentLogger?.LogNpcInteraction();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Jeśli agent stoi blisko Waypointu, odkryj go i zwróć true.
    /// </summary>
    private bool DiscoverWaypointIfClose()
    {
        // Zakładam, że stany waypointów rozpoznajesz np. 
        // po tagu "Waypoint" lub innym.
        // Lub mogłeś w scannerze mieć listę waypointów...
        var waypoint = GetClosestWaypoint(); // Musisz zaimplementować
        if (waypoint == null) return false;

        float dist = Vector3.Distance(transform.position, waypoint.position);
        if (dist < 2f) // Załóżmy strefa odkrycia = 2
        {
            // Logi w AgentBehaviorLogger / PlayerBehaviorLogger
            agentLogger?.LogWaypointDiscovery(waypoint.GetInstanceID());
            Debug.Log($"[Agent] Discovered Waypoint: {waypoint.name}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Jeśli agent stoi blisko strefy (zone), odkryj i zwróć true.
    /// </summary>
    private bool DiscoverZoneIfClose()
    {
        // Zakładam, że "strefa" to np. collidery z tagiem "Zone" 
        // i chcesz logować w agentLogger
        var zone = GetClosestZone(); // Implementacja zależna od Twojego systemu
        if (zone == null) return false;

        float dist = Vector3.Distance(transform.position, zone.position);
        if (dist < 3f)
        {
            agentLogger?.LogAgentZoneDiscovery(zone.name);
            Debug.Log($"[Agent] Discovered Zone: {zone.name}");
            return true;
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

    // -------------------------------------------------
    // (Poniżej) Przykładowe metody do wyłuskania 
    // najbliższego waypoint / zone, dopasuj do swojego systemu.
    // -------------------------------------------------

    private Transform GetClosestWaypoint()
    {
        // np. w scenie obiekty z tagiem "Waypoint"
        var waypoints = GameObject.FindGameObjectsWithTag("Waypoint");
        if (waypoints.Length == 0) return null;

        Transform best = null;
        float bestDist = Mathf.Infinity;
        foreach (var go in waypoints)
        {
            float d = Vector3.Distance(transform.position, go.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = go.transform;
            }
        }
        return best;
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
                return true;
            }
        }

        return false; // Brak przeciwników
    }

    private bool ExploreZones()
    {
        if (target == null)
            SetRandomTarget();

        if (target != null)
        {
            agentWantsToMove = true;
            entity.MoveTo(target.position);
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget < 1.0f)
            {
                DiscoverZone(target.name); // Odkryj strefę
                return true;
            }
        }
        return false; // Brak stref do odkrycia
    }

    private bool InteractWithNPC()
{
    if (Time.time - lastInteractionTime < interactionCooldown)
        return false;

    var scanner = GetComponent<EntityAreaScanner>();
    var closestInteractive = scanner?.GetClosestInteractiveObject();

    if (closestInteractive != null && closestInteractive.CanAgentInteract)
    {
        if (interactedNPCs.Contains(closestInteractive.name))
        {
            Debug.Log($"Agent AI already interacted with {closestInteractive.name}. Skipping.");
            return false;
        }

        agentWantsToMove = true;
        entity.MoveTo(closestInteractive.transform.position);
        float distance = Vector3.Distance(transform.position, closestInteractive.transform.position);
        if (distance < 1.5f)
        {
            closestInteractive.Interact(entity);
            lastInteractionTime = Time.time;
            interactedNPCs.Add(closestInteractive.name);
            Debug.Log($"Agent AI interacted with {closestInteractive.name}.");
            agentLogger?.LogNpcInteraction();
            return true;
        }
    }

    return false;
}

    private Vector3 wanderTarget; // Docelowy punkt wędrowania
    private float wanderChangeInterval = 5f; // Co ile sekund zmieniać punkt wędrowania
    private float lastWanderChangeTime;

    private void WanderRandomly()
    {
        // Jeśli czas na zmianę celu minął lub cel nie jest ustawiony
        if (Time.time - lastWanderChangeTime > wanderChangeInterval || wanderTarget == Vector3.zero)
        {
            // Wybierz losowy kierunek w promieniu od aktualnej pozycji
            float wanderRadius = 10f; // Promień, w którym agent będzie wędrować
            wanderTarget = transform.position + new Vector3(
                Random.Range(-wanderRadius, wanderRadius),
                0,
                Random.Range(-wanderRadius, wanderRadius)
            );

            lastWanderChangeTime = Time.time;
            Debug.Log($"Agent AI changed wander target to: {wanderTarget}");
        }

        // Poruszaj się w kierunku wybranego celu
        agentWantsToMove = true;
        entity.MoveTo(wanderTarget);

        // Jeśli agent osiągnął cel (bliskość mniejsza niż 1 jednostka), ustaw nowy cel
        if (Vector3.Distance(transform.position, wanderTarget) < 1f)
        {
            wanderTarget = Vector3.zero; // Reset celu
            Debug.Log("Agent AI reached wander target.");
        }
    }

    private void PerformAttack(Transform enemy)
    {
        if (Time.time - lastAttackTime < attackCooldown)
        {
            Debug.Log($"[Agent {name}] Attack on cooldown. Time remaining: {attackCooldown - (Time.time - lastAttackTime):F2}s");
            return;
        }

        if (enemy == null || enemy.GetComponent<Entity>().isDead)
        {
            Debug.Log($"[Agent {name}] Attack failed: target is null or dead.");
            return;
        }

        var targetEntity = enemy.GetComponent<Entity>();

        // Sprawdzenie, czy przeciwnik zginął po ataku
        if (targetEntity.stats.health - 10 <= 0) 
        {
            Debug.Log($"[Agent {name}] Killed enemy: {targetEntity.name}");
            agentLogger?.LogEnemyKilled(targetEntity.name);
        }

        var skillManager = entity.GetComponent<EntitySkillManager>();

        // Jeśli agent ma skille, próbujemy użyć skilla
        if (skillManager != null && skillManager.CanUseSkill())
        {
            Debug.Log($"[Agent {name}] Using skill on {enemy.name}");
            skillManager.PerformSkill();
        }
        else
        {
            // Jeśli brak skilli do użycia, wykonaj zwykły atak fizyczny
            Debug.Log($"[Agent {name}] Attacking {enemy.name} with basic attack!");
            targetEntity.Damage(entity, 10, false); // Wywołanie ataku fizycznego
        }

        lastAttackTime = Time.time;
    }

    private void FindClosestEnemy()
    {
        var scanner = GetComponent<EntityAreaScanner>();
        if (scanner == null)
        {
            Debug.LogWarning("EntityAreaScanner is missing on Agent AI.");
            targetEnemy = null;
            return;
        }

        targetEnemy = scanner.GetClosestTarget();

        if (targetEnemy != null && !targetEnemy.CompareTag("Entity/Enemy"))
        {
            Debug.Log($"Ignored non-enemy target: {targetEnemy.name}");
            targetEnemy = null;
        }

        if (targetEnemy == null)
        {
            Debug.Log("No enemies found in the area.");
        }
        else
        {
            Debug.Log($"Closest enemy found: {targetEnemy.name}");
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

    public void DiscoverZone(string zoneName)
    {
        Debug.Log($"AI Agent discovered zone: {zoneName}");

        // Przyznaj nagrodę za odkrycie strefy
        SetReward(1.0f);
        Debug.Log("Reward given for discovering zone.");

        // Logowanie odkrycia strefy w AgentBehaviorLogger
        agentLogger?.LogAgentZoneDiscovery(zoneName);

        // Zakończ epizod
        EndAgentEpisode(); // Zamiast EndEpisode()
        Debug.Log("Episode ended after discovering zone.");
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


/*
    private void EquipDefaultItems()
    {
        var itemManager = GetComponent<EntityItemManager>();
        if (itemManager != null && gameDatabase != null)
        {
            // Wyszukiwanie przedmiotów na podstawie ID
            var defaultWeapon = new ItemInstance(gameDatabase.items.OfType<ItemWeapon>().FirstOrDefault(item => item.id == 33));    // Keroisa Sword
            var defaultShield = new ItemInstance(gameDatabase.items.OfType<ItemShield>().FirstOrDefault(item => item.id == 19));     // 

            if (defaultWeapon != null) itemManager.TryEquip(defaultWeapon, ItemSlots.RightHand);
            if (defaultShield != null) itemManager.TryEquip(defaultShield, ItemSlots.LeftHand);

            Debug.Log("Agent AI equipped with default items.");
        }
        else
        {
            Debug.LogWarning("ItemManager or GameDatabase is missing. Cannot equip default items.");
        }
    }
*/

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


    private void ClearLastInteractedObject()
    {
        lastInteractedObject = null;
    }

    private void EndAgentEpisode()
    {
        // Dodaj własne logi lub operacje przed zakończeniem epizodu
        Debug.Log("Ending episode for AI Agent...");
        Debug.Log($"Reward before ending: {GetCumulativeReward()}"); // Sprawdź, czy nagroda jest prawidłowa
        // Wywołaj bazową metodę EndEpisode()
        EndEpisode();
        agentLogger.SaveLogsToFile();
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
        float movementThreshold = 0.2f; // Minimalna odległość, jaką agent powinien pokonać

        if (!agentWantsToMove)
        {
            stuckTimer = 0f;
            lastPosition = transform.position;
            return;
        }

        float distMoved = Vector3.Distance(transform.position, lastPosition);
        if (distMoved < movementThreshold)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > stuckThreshold)
            {
                Debug.Log($"[Agent {name}] Agent appears stuck! Choosing a new wander target.");
                stuckTimer = 0f;
                wanderTarget = Vector3.zero; // Natychmiastowy reset celu
                WanderAround(new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized);
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = transform.position;
    }

    private void AttemptUnstuck()
    {
        // 1. Krok w tył
        Vector3 stepBackDir = -transform.forward;       // cofanie
        float stepDistance = 2f;                        // o ile cofamy?
        Vector3 newPosition = transform.position + stepBackDir * stepDistance;

        // 2. Sprawdź, czy mamy NavMeshPath do newPosition (opcjonalnie)
        //    Jeżeli tak, MoveTo -> agent sam tam pójdzie
        if (entity.TryCalculatePath(newPosition))
        {
            agentWantsToMove = true;
            entity.MoveTo(newPosition);
            
            Debug.Log($"[AgentController] AttemptUnstuck: step back {stepDistance}m");
            return;
        }

        // 3. Jeżeli cofanie się nie powiodło (np. brak path),
        //    spróbuj obrócić się o losowy kąt i pójść do przodu
        float randomAngle = Random.Range(-90f, 90f);
        Vector3 rotated = Quaternion.Euler(0, randomAngle, 0) * transform.forward;
        newPosition = transform.position + rotated * 3f;

        if (entity.TryCalculatePath(newPosition))
        {
            agentWantsToMove = true;
            entity.MoveTo(newPosition);
            Debug.Log($"[AgentController] AttemptUnstuck: rotate {randomAngle}° and move forward 3m");
            return;
        }

        // 4. Jeśli obie próby się nie udały, można zdecydować się na
        //    losowe teleportowanie lub zignorować (ostatnia deska ratunku)
        Debug.LogWarning("[AgentController] AttemptUnstuck: both stepBack & rotate random failed. Possibly agent is truly blocked...");
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
