//  ZMODYFIKOWANO 31 GRUDNIA 2024

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Misc/Spawner")]
    public class Spawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [Tooltip("The amount of entities to spawn.")]
        public int count = 5;

        [Tooltip("The minimum distance from the spawn origin to spawn entities.")]
        public float minRadius = 8;

        [Tooltip("The maximum distance from the spawn origin to spawn entities.")]
        public float maxRadius = 10;

        [Tooltip("The duration in seconds between respawn.")]
        public float respawnDelay = 10f;

        [Tooltip("The list of all entities that can be spawned.")]
        public Entity[] entities;

        [Tooltip("The maximum number of active entities.")]
        public int maxEntities = 20; // Limit aktywnych przeciwników

        protected Entity m_tempEntity;
        protected List<Entity> m_entities = new();

        protected WaitForSeconds m_waitForRespawnDelay;

        /// <summary>
        /// Returns a random entity from the entities list.
        /// </summary>
        protected Entity GetRandomEntity() => entities[Random.Range(0, entities.Length)];

        protected virtual void InitializeWaits()
        {
            m_waitForRespawnDelay = new WaitForSeconds(respawnDelay);
        }

        protected virtual void InitializeEntities()
        {
            for (int i = 0; i < count; i++) Spawn();
        }

        protected virtual void Spawn()
        {
            if (m_entities.Count >= maxEntities)
            {
                Debug.Log("Max entity count reached. Spawn aborted.");
                return; // Nie spawnuj więcej, jeśli osiągnięto limit
            }

            var random = Random.insideUnitSphere;
            var radius = Random.Range(minRadius, maxRadius);
            var direction = new Vector3(random.x, 0, random.y);
            var position = transform.position + direction * radius;

            // Użyj Physics.Raycast, aby upewnić się, że pozycja jest nad terenem
            if (Physics.Raycast(position + Vector3.up * 10f, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Terrain")))
            {
                position = hit.point + Vector3.up * 0.5f; // Ustaw pozycję nad powierzchnią
            }
            else
            {
                Debug.LogWarning("No terrain detected for spawning. Skipping spawn.");
                return; // Anuluj spawn, jeśli nie znaleziono terenu
            }

            var rotation = Quaternion.LookRotation(direction, Vector3.up);
            m_tempEntity = GetRandomEntity();

            // Instancjuj jednostkę
            var entity = Instantiate(m_tempEntity, position, rotation);

            // Dodaj do listy w Level.cs
            Level.instance.AddEntityToTracking(entity);

            // Upewnij się, że DifficultyManager.Instance istnieje
            if (DifficultyManager.Instance == null)
            {
                Debug.LogWarning("DifficultyManager.Instance is null. Enemy stats not adjusted.");
            }
            else
            {
                // Dostosowanie trudności
                if (entity.stats != null)
                {
                    entity.stats.dexterity = entity.stats.dexterity > 0 
                        ? Mathf.Max(1, (int)(entity.stats.dexterity * DifficultyManager.Instance.CurrentDexterityMultiplier))
                        : 10; // Domyślna wartość, jeśli brak inicjalizacji

                    entity.stats.strength = entity.stats.strength > 0 
                        ? Mathf.Max(1, (int)(entity.stats.strength * DifficultyManager.Instance.CurrentStrengthMultiplier))
                        : 5; // Domyślna wartość, jeśli brak inicjalizacji

                    entity.stats.vitality = entity.stats.vitality > 0 
                        ? Mathf.Max(1, (int)(entity.stats.vitality * DifficultyManager.Instance.CurrentVitalityMultiplier))
                        : 8; // Domyślna wartość, jeśli brak inicjalizacji

                    entity.stats.energy = entity.stats.energy > 0 
                        ? Mathf.Max(1, (int)(entity.stats.energy * DifficultyManager.Instance.CurrentEnergyMultiplier))
                        : 6; // Domyślna wartość, jeśli brak inicjalizacji

                    // Automatyczna aktualizacja statystyk
                    entity.stats.Recalculate();

                    // Oznacz przeciwnika jako "starego" po spawnie, aby nie pokazywać DifficultyText
                    entity.stats.isNewlySpawned = false;

                    Debug.Log($"Spawned enemy {entity.name}: " +
                        $"Dexterity={entity.stats.dexterity}, " +
                        $"Strength={entity.stats.strength}, " +
                        $"Vitality={entity.stats.vitality}, " +
                        $"Energy={entity.stats.energy}, " +
                        $"Multipliers: Dexterity={DifficultyManager.Instance.CurrentDexterityMultiplier}, " +
                        $"Strength={DifficultyManager.Instance.CurrentStrengthMultiplier}, " +
                        $"Vitality={DifficultyManager.Instance.CurrentVitalityMultiplier}, " +
                        $"Energy={DifficultyManager.Instance.CurrentEnergyMultiplier}");
                }
                else
                {
                    Debug.LogWarning($"Entity {entity.name} has no stats. Difficulty adjustment skipped.");
                }
            }

            // Dodaj przeciwnika do listy lokalnej i nasłuchuj śmierci
            m_entities.Add(entity);
            entity.onDie.AddListener(() => OnEntityDie(entity));
        }

        protected virtual void OnEntityDie(Entity entity)
        {
            if (m_entities.Contains(entity))
                m_entities.Remove(entity);

            if (!gameObject.activeSelf) return;

            StartCoroutine(RespawnRoutine());
        }

        protected virtual IEnumerator RespawnRoutine()
        {
            yield return m_waitForRespawnDelay;

            if (m_entities.Count < maxEntities) // Sprawdzaj limit przed dodaniem
                Spawn();
        }

        protected virtual void Start()
        {
            InitializeWaits();
            InitializeEntities();
        }

    }
}