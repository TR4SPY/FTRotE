using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.AI;
using AI_DDA.Assets.Scripts;

public class AgentController : Agent
{
    private NavMeshAgent navMeshAgent;
    public Transform target; // Cel agenta

    public override void Initialize()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public override void OnEpisodeBegin()
    {
        // Resetuj pozycję agenta
        var playerInitializer = Object.FindFirstObjectByType<PlayerInitializer>();
        if (playerInitializer != null)
        {
            Vector3 startPosition = playerInitializer.transform.position;
            transform.position = startPosition;
        }
        else
        {
            Debug.LogWarning("PlayerInitializer not found. Using default position.");
            transform.position = new Vector3(0, 0.5f, 0); // Domyślna pozycja
        }

        // Ustaw nowy cel
        SetRandomTarget();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Przykład: Dodaj pozycję agenta (3 obserwacje dla X, Y, Z)
        sensor.AddObservation(transform.position);

        // Przykład: Dodaj pozycję celu (3 obserwacje dla X, Y, Z)
        if (target != null)
        {
            sensor.AddObservation(target.position);
        }
        else
        {
            sensor.AddObservation(Vector3.zero); // Jeśli brak celu, dodaj zerowe obserwacje
        }

        // Przykład: Dodaj odległość do celu (1 obserwacja)
        float distanceToTarget = target != null ? Vector3.Distance(transform.position, target.position) : float.MaxValue;
        sensor.AddObservation(distanceToTarget);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Konwertuj akcje na kierunek ruchu
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;
        Vector3 targetPosition = transform.position + moveDirection * Time.deltaTime * navMeshAgent.speed;

        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(hit.position);
        }

        // Nagroda za zbliżenie się do celu
        if (target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget < 1.0f)
            {
                SetReward(1.0f);
                EndEpisode();
            }
        }
        else
        {
            Debug.LogWarning("No target set for the agent.");
        }
    }

    private void SetRandomTarget()
    {
        // Ustaw losowy cel (np. jeden z NPC lub stref)
        var targets = Object.FindObjectsByType<ZoneTrigger>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (targets.Length > 0)
        {
            target = targets[Random.Range(0, targets.Length)].transform;
        }
        else
        {
            Debug.LogWarning("No ZoneTriggers found. Cannot set target.");
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Manualne sterowanie agentem (opcjonalne)
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal"); // Ruch w osi X
        continuousActions[1] = Input.GetAxis("Vertical");   // Ruch w osi Z
    }
}
