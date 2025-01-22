using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using PLAYERTWO.ARPGProject;
using AI_DDA.Assets.Scripts;
using Unity.MLAgents.Policies;

public class AgentController : Agent
{
    private Entity entity; // Referencja do Entity
    private EntityAI entityAI; // Referencja do EntityAI
    private int totalObservationsCalls = 0;
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
        // Dodaj pozycję agenta (3 obserwacje)
        Vector3 agentPosition = transform.position;
        sensor.AddObservation(agentPosition.x);
        sensor.AddObservation(agentPosition.y);
        sensor.AddObservation(agentPosition.z);

        if (target != null)
        {
            // Dodaj pozycję celu (3 obserwacje)
            Vector3 targetPosition = target.position;
            sensor.AddObservation(targetPosition.x);
            sensor.AddObservation(targetPosition.y);
            sensor.AddObservation(targetPosition.z);

            // Dodaj odległość do celu (1 obserwacja)
            float distanceToTarget = Vector3.Distance(agentPosition, targetPosition);
            sensor.AddObservation(distanceToTarget);
        }
        else
        {
            // Wypełnij zerami brakujące obserwacje
            sensor.AddObservation(0f); // Cel x
            sensor.AddObservation(0f); // Cel y
            sensor.AddObservation(0f); // Cel z
            sensor.AddObservation(0f); // Odległość do celu
        }

        // Debugowanie liczby dodanych obserwacji
        Debug.Log($"CollectObservations called. Observation count added: {sensor.ObservationSize()}");
        totalObservationsCalls++;
        Debug.Log($"CollectObservations call #{totalObservationsCalls} - Observation count added: {sensor.ObservationSize()}");
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Debug.Log("OnActionReceived called.");

        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;

        if (target != null)
        {
            // Ruch w kierunku celu
            entity.MoveTo(target.position);

            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget < 1.0f)
            {
                // Przyznaj nagrodę i zakończ epizod
                SetReward(1.0f);
                Debug.Log($"Target reached: {target.name}. Reward given.");
                EndEpisode();
            }
        }
        else
        {
            // Ruch oparty na akcjach, jeśli brak celu
            entity.MoveTo(transform.position + moveDirection * entity.moveSpeed);
            Debug.Log($"Moving based on actions: {moveDirection}");
        }

        // Debugowanie akcji i pozycji
        Debug.Log($"Action X: {moveX}, Action Z: {moveZ}, Position: {transform.position}");
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

        // Zakończ epizod
        EndAgentEpisode(); // Zamiast EndEpisode()
        Debug.Log("Episode ended after discovering zone.");
    }

    private void EndAgentEpisode()
    {
        // Dodaj własne logi lub operacje przed zakończeniem epizodu
        Debug.Log("Ending episode for AI Agent...");
        Debug.Log($"Reward before ending: {GetCumulativeReward()}"); // Sprawdź, czy nagroda jest prawidłowa
        // Wywołaj bazową metodę EndEpisode()
        EndEpisode();
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

    void Update()
    {
        if (Time.frameCount % 30 == 0) // Log every 30 frames
        {
            Debug.Log($"Frame: {Time.frameCount}, StepCount: {Academy.Instance.StepCount}");
        }

        RequestDecision();
    }


}
