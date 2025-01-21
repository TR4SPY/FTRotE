using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using PLAYERTWO.ARPGProject;
using AI_DDA.Assets.Scripts;

public class AgentController : Agent
{
    private Entity entity; // Referencja do Entity
    private EntityAI entityAI; // Referencja do EntityAI

    public Transform target; // Cel agenta
    public bool isAI = true; // Flaga do identyfikacji jako AI Agent

    public override void Initialize()
    {
        // Pobierz komponenty Entity i EntityAI
        entity = GetComponent<Entity>();
        entityAI = GetComponent<EntityAI>();

        if (entity == null || entityAI == null)
        {
            Debug.LogError("Entity or EntityAI component is missing on the AI Agent!");
        }

        // Ustaw domyślne parametry Entity
        entity.moveSpeed = 5f;
        entity.rotationSpeed = 720f;

        // Ustaw tagi do wykrywania stref (jeśli wymagane)
        entity.targetTags = new List<string> { "ZoneTrigger" };
    }

    public override void OnEpisodeBegin()
    {
        // Resetuj pozycję agenta
        Vector3 startPosition = new Vector3(Random.Range(-10f, 10f), 0.5f, Random.Range(-10f, 10f));
        entity.Teleport(startPosition, Quaternion.identity);

        // Ustaw nowy cel
        SetRandomTarget();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Dodaj pozycję agenta
        sensor.AddObservation(transform.position);

        // Dodaj pozycję celu, jeśli istnieje
        if (target != null)
        {
            sensor.AddObservation(target.position);
            sensor.AddObservation(Vector3.Distance(transform.position, target.position));
        }
        else
        {
            sensor.AddObservation(Vector3.zero); // Jeśli brak celu, dodaj zerowe obserwacje
            sensor.AddObservation(0f);          // Dodaj zerową odległość
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Pobierz akcje i wygeneruj kierunek ruchu
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;

        // Wykonaj ruch za pomocą Entity
        if (target != null)
        {
            entity.MoveTo(target.position);
        }
        else
        {
            entity.MoveTo(transform.position + moveDirection * entity.moveSpeed);
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
    }

    private void SetRandomTarget()
    {
        // Wybierz losowy cel z dostępnych ZoneTrigger
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
        }

        // Ustaw cel w Entity
        if (target != null)
        {
            entity.SetTarget(target);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Ręczne sterowanie agentem
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal"); // Ruch w osi X
        continuousActions[1] = Input.GetAxis("Vertical");   // Ruch w osi Z
    }
}
