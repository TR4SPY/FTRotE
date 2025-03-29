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
        public int maxEntities = 20;

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
                return;
            }

            var random = Random.insideUnitSphere;
            var radius = Random.Range(minRadius, maxRadius);
            var direction = new Vector3(random.x, 0, random.y);
            var position = transform.position + direction * radius;

            if (Physics.Raycast(position + Vector3.up * 10f, Vector3.down, out RaycastHit hit,
                                Mathf.Infinity, LayerMask.GetMask("Terrain")))
            {
                position = hit.point + Vector3.up * 0.5f;
            }
            else
            {
                Debug.LogWarning("No terrain detected for spawning. Skipping spawn.");
                return;
            }

            var rotation = Quaternion.LookRotation(direction, Vector3.up);
            m_tempEntity = GetRandomEntity();
            var entity = Instantiate(m_tempEntity, position, rotation);
            Level.instance.AddEntityToTracking(entity);

            if (entity.stats != null)
            {
                entity.stats.Initialize();
                entity.stats.isNewlySpawned = true;
                DifficultyManager.Instance.ApplyCurrentDifficultyToNewEnemy(entity);
            }
            else
            {
                Debug.LogWarning($"[AI-DDA] Spawned entity {entity.name} has no stats. Skipping difficulty adjustment.");
            }

            m_entities.Add(entity);
            entity.onDie.AddListener(() => OnEntityDie(entity));

            // Debug.Log($"[AI-DDA] Spawned enemy {entity.name} at {position}. Current difficulty: {DifficultyManager.Instance.GetRawDifficulty()}");
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

            if (m_entities.Count < maxEntities)
                Spawn();
        }

        protected virtual void Start()
        {
            InitializeWaits();
            InitializeEntities();
        }

    }
}