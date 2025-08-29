using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using AI_DDA.Assets.Scripts;
using System.Collections;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Level/Level")]
    public class Level : Singleton<Level>
    {
        [Tooltip("The transform that represents the initial position and rotation of the Character.")]
        public Transform playerOrigin;

        [Header("Tracking Lists")]
        [HideInInspector]
        [Tooltip("The list of all entities in which the Level tracks.")]
        public Entity[] entities;

        [Tooltip("The list of all interactives objects in which the Level tracks.")]
        public Interactive[] interactives;

        [Tooltip("The list of all Game Objects in which the Level tracks.")]
        public GameObject[] gameObjects;

        /// <summary>
        /// Returns the Entity that represents the current player.
        /// </summary>
        public Entity player { get; protected set; }

        public LevelQuests quests => LevelQuests.instance;
        public LevelWaypoints waypoints => LevelWaypoints.instance;
        public CharacterInstance currentCharacter => Game.instance.currentCharacter;
        public Scene currentScene => SceneManager.GetActiveScene();

        protected virtual void InitializePlayer()
        {
            if (Physics.Raycast(playerOrigin.position, Vector3.down, out var hit))
            {
                player = currentCharacter.Instantiate();

                StartCoroutine(RestoreVitalsNextFrame());

                if (currentCharacter.currentScene.CompareTo(currentScene.name) == 0 &&
                    (currentCharacter.initialPosition != Vector3.zero || currentCharacter.initialRotation.eulerAngles != Vector3.zero))
                {
                    player.Teleport(currentCharacter.initialPosition, currentCharacter.initialRotation);
                }
                else
                {
                    var position = hit.point + Vector3.up;
                    var rotation = playerOrigin.rotation;

                    player.Teleport(position, rotation);
                }
            }
        }

        private IEnumerator RestoreVitalsNextFrame()
        {
            yield return null;
            currentCharacter.RestoreSavedVitals();
        }

        protected virtual void RestoreState()
        {
            if (!currentCharacter.scenes.TryGetScene(currentScene.name, out var scene)) return;

            UpdateTrackedEntities();

            for (int i = 0; i < scene.entities.Length; i++)
            {
                if (i >= entities.Length) break;

                var position = scene.entities[i].position;
                var rotation = Quaternion.Euler(scene.entities[i].rotation);

                if (scene.entities[i].health == 0)
                {
                    entities[i].gameObject.SetActive(false);
                }
                else
                {
                    if (entities[i].stats != null)
                    {
                        entities[i].stats.Initialize();
                        entities[i].stats.health = scene.entities[i].health;
                        entities[i].Teleport(position, rotation);
                    }
                    else
                    {
                        Debug.LogWarning($"Entity {entities[i].name} does not have stats initialized!");
                    }
                }
            }

            for (int i = 0; i < scene.waypoints.Length; i++)
            {
                if (i >= waypoints.waypoints.Count) break;

                if (waypoints.waypoints[i].title.CompareTo(scene.waypoints[i].title) != 0)
                    continue;

                waypoints.waypoints[i].active = scene.waypoints[i].active;
            }

            if (scene.currentWaypointIndex >= 0 &&
                scene.currentWaypointIndex < waypoints.waypoints.Count)
                waypoints.currentWaypoint = waypoints.waypoints[scene.currentWaypointIndex];

            RestoreQuestItems(scene);
            RestoreGameObjects(scene);
            ApplyDifficultyToEntities();
        }

        protected virtual void RestoreQuestItems(CharacterScenes.Scene scene)
        {
            if (scene.interactives == null) return;

            for (int i = 0; i < scene.interactives.Length; i++)
            {
                if (interactives == null || i >= interactives.Length) break;

                interactives[i].interactive = scene.interactives[i].interactive;
            }
        }

        protected virtual void RestoreGameObjects(CharacterScenes.Scene scene)
        {
            if (scene.gameObjects == null) return;

            for (int i = 0; i < scene.gameObjects.Length; i++)
            {
                if (gameObject == null || i >= gameObjects.Length) break;

                gameObjects[i].transform.position = scene.gameObjects[i].position;
                gameObjects[i].transform.rotation = Quaternion.Euler(scene.gameObjects[i].rotation);
                gameObjects[i].SetActive(scene.gameObjects[i].active);
            }
        }

        protected virtual void EvaluateQuestScene() =>
            Game.instance.quests?.ReachedScene(currentScene.name);
            
        protected override void Initialize()
        {
            Debug.Log("Initializing Level...");

            InitializePlayer();

            RestoreState();
            UpdateTrackedEntities();
            ApplyDifficultyToEntities();
            EvaluateQuestScene();

            Debug.Log("Level initialized successfully.");
        }

        protected virtual void UpdateTrackedEntities()
        {
            var foundEntities = Object.FindObjectsByType<Entity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(entity => entity.CompareTag("Entity/Enemy") && entity.gameObject.layer == LayerMask.NameToLayer("Entities"))
                .ToArray();

            entities = foundEntities;

            //Debug.Log($"Tracked {entities.Length} entities:");
            foreach (var entity in entities)
            {
               // Debug.Log($" - {entity.name}");
            }
        }

        public virtual void AddEntityToTracking(Entity entity)
        {
            if (entity == null || !entity.CompareTag("Entity/Enemy") || entity.gameObject.layer != LayerMask.NameToLayer("Entities"))
                return;

            var tempList = entities.ToList();
            if (!tempList.Contains(entity))
            {
                tempList.Add(entity);
                entities = tempList.ToArray();

                // Debug.Log($"Entity {entity.name} added to tracking. Total entities: {entities.Length}");
            }
        }

        public void SetPlayer(Entity entity)
        {
            player = entity;
        }

        /// <summary>
        /// Applies the current difficulty settings to all tracked entities.
        /// </summary>
        public virtual void ApplyDifficultyToEntities()
        {
            if (entities == null || entities.Length == 0)
            {
                Debug.LogWarning("No entities found to apply difficulty.");
                return;
            }

            float difficulty = DifficultyManager.Instance != null
                ? DifficultyManager.Instance.GetRawDifficulty()
                : 5.0f; // Fallback – domyślna wartość

            foreach (var entity in entities)
            {
                if (entity == null || !entity.CompareTag("Entity/Enemy") || entity.stats == null)
                    continue;

                entity.stats.Initialize();

                if (entity.stats.wasBoosted)
                {
                    // Debug.LogWarning($"[Diff-Manager] {entity.name} był już boostowany – pomijam ApplyDifficultyToEntities");
                    continue;
                }

                entity.stats.strength  = DifficultyManager.Instance.GetCurvedStat(entity.stats.GetBaseStrength(),  difficulty, 0.25f, 5.0f);
                entity.stats.dexterity = DifficultyManager.Instance.GetCurvedStat(entity.stats.GetBaseDexterity(), difficulty, 0.25f, 6.0f);
                entity.stats.vitality  = DifficultyManager.Instance.GetCurvedStat(entity.stats.GetBaseVitality(),  difficulty, 0.25f, 6.0f);
                entity.stats.energy    = DifficultyManager.Instance.GetCurvedStat(entity.stats.GetBaseEnergy(),    difficulty, 0.25f, 5.0f);

                entity.stats.wasBoosted = true;
                entity.stats.Recalculate();
            }
        }

        protected virtual void Start() => EvaluateQuestScene();
    }
}