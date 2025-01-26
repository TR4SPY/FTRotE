using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using PLAYERTWO.ARPGProject;
using AI_DDA.Assets.Scripts;
using Unity.MLAgents.Policies;
using System.Linq;

public class AgentController : Agent
{
    private Entity entity; // Referencja do Entity
    private EntityAI entityAI; // Referencja do EntityAI
    private int totalObservationsCalls = 0;
    private Interactive lastInteractedObject; // Przechowuje ostatnio użyty obiekt interaktywny
    private float lastInteractionTime = 0f;
    private float interactionCooldown = 5f; // Czas w sekundach
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private float interactionTimeout = 10f; // Maksymalny czas na osiągnięcie celu (w sekundach)
    private float interactionTimer = 0f;
    private float lastAttackTime = -5f; // Ostatni czas ataku
    private float attackCooldown = 2f; // Cooldown ataku (w sekundach)
    private const float stuckThreshold = 2f; // Czas w sekundach, po którym agent jest uznany za zablokowanego
    private bool isResetting = false;
    private Transform targetEnemy; // Przechowuje referencję do najbliższego przeciwnika
    private GameDatabase gameDatabase;
    private AgentBehaviorLogger agentLogger;

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
        entity.moveSpeed = 5f;
        entity.rotationSpeed = 720f;

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
        var closestEnemy = GetComponent<EntityAreaScanner>()?.GetClosestTarget();
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

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;

        // Priorytet 1: Atak na przeciwników
        if (FindAndAttackEnemy())
            return;

        // Priorytet 2: Eksploracja stref
        if (ExploreZones())
            return;

        // Priorytet 3: Interakcja z NPC
        if (InteractWithNPC())
            return;

        // Priorytet 4: Poruszanie się w losowym kierunku (wędrowanie)
        WanderRandomly();
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
        return false; // Odczekaj do końca cooldownu

    var scanner = GetComponent<EntityAreaScanner>();
    var closestInteractive = scanner?.GetClosestInteractiveObject();

    if (closestInteractive != null && closestInteractive.CanAgentInteract)
    {
        entity.MoveTo(closestInteractive.transform.position);
        float distance = Vector3.Distance(transform.position, closestInteractive.transform.position);
        if (distance < 1.0f)
        {
            closestInteractive.Interact(entity);
            lastInteractionTime = Time.time;
            Debug.Log($"Agent AI interacted with {closestInteractive.name}.");
            return true;
        }
    }

    return false; // Brak NPC do interakcji
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
        Debug.Log($"Attack on cooldown. Time remaining: {attackCooldown - (Time.time - lastAttackTime):F2}s");
        return;
    }

    var skillManager = GetComponent<EntitySkillManager>();
    if (skillManager != null && skillManager.CanUseSkill())
    {
        Debug.Log($"Agent AI attempting to use skill on {enemy.name}");
        skillManager.PerformSkill(); // Użycie skilla
        lastAttackTime = Time.time;

        // Loguj użycie umiejętności
        Debug.Log($"Agent AI used skill {skillManager.current.name} on {enemy.name}");
    }
    else
    {
        Debug.LogWarning("Agent AI cannot use skill. SkillManager missing or no usable skill.");
    }
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

    private void AvoidEnemies()
    {
        var scanner = GetComponent<EntityAreaScanner>();
        if (scanner == null)
        {
            Debug.LogWarning("EntityAreaScanner is missing on Agent AI.");
            return;
        }

        var closestEnemy = scanner.GetClosestTarget();

        if (closestEnemy != null && closestEnemy.CompareTag("Entity/Enemy"))
        {
            float closestDistance = Vector3.Distance(transform.position, closestEnemy.position);

            if (closestDistance <= 1.5f)
            {
                Debug.Log($"Enemy {closestEnemy.name} is within attack range. Not avoiding.");
                return; // Wróg w zasięgu ataku, nie unikaj
            }

            if (closestDistance < 5f)
            {
                Vector3 fleeDirection = (transform.position - closestEnemy.position).normalized;
                entity.MoveTo(transform.position + fleeDirection * entity.moveSpeed);
                Debug.Log($"Avoiding enemy: {closestEnemy.name} at distance {closestDistance}");

                // Nagroda za unikanie
                SetReward(0.1f);
                agentLogger?.LogEnemyAvoided(closestEnemy.name);
            }
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
        statsManager.level = 1;
        statsManager.strength = 10;
        statsManager.dexterity = 8;
        statsManager.vitality = 12;
        statsManager.energy = 5;
        statsManager.health = 500;
        statsManager.mana = 500;
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
        PlayerBehaviorLogger.Instance.SaveAgentLogsToFile();
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
            entity.MoveTo(transform.position + avoidDirection * 3f); // Przesunięcie o 3 jednostki od przeszkody
        }
    }

    private void CheckIfStuck()
    {
        if (isResetting) return;

        if (Vector3.Distance(transform.position, lastPosition) < 0.2f) // Tolerancja na ruch
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > stuckThreshold)
            {
                Debug.Log("AI Agent is stuck! Resetting position or target.");
                isResetting = true;
                ResetAgent();
                stuckTimer = 0f;
                isResetting = false;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = transform.position;
    }

    private void ResetAgent()
    {
        if (target != null)
        {
            Vector3 fleeDirection = (transform.position - target.position).normalized;
            Vector3 newPosition = transform.position + fleeDirection * 3f;
            entity.MoveTo(newPosition);
            Debug.Log($"Agent reset to avoid being stuck. New position: {newPosition}");
        }
        else
        {
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized;

            Vector3 newPosition = transform.position + randomDirection * 3f;
            entity.MoveTo(newPosition);
            Debug.Log($"Agent reset. Moving to new random position: {newPosition}");
        }
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

    private void DebugAgentState()
    {
        Debug.Log($"Agent Position: {transform.position}, Target: {target?.name ?? "None"}, StuckTimer: {stuckTimer}, InteractionTimer: {interactionTimer}");
    }

    void Update()
    {
        CheckIfStuck();
        CheckInteractionTimeout();

        if (Time.frameCount % 30 == 0) // Log every 30 frames
        {
            // Debug.Log($"Frame: {Time.frameCount}, StepCount: {Academy.Instance.StepCount}");
        }

        RequestDecision();
    }


}
