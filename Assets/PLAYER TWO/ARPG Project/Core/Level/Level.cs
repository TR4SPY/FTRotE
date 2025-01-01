using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using AI_DDA.Assets.Scripts;

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

                if (currentCharacter.currentScene.CompareTo(currentScene.name) == 0 &&
                    (currentCharacter.initialPosition != Vector3.zero ||
                    currentCharacter.initialRotation.eulerAngles != Vector3.zero))
                    player.Teleport(currentCharacter.initialPosition, currentCharacter.initialRotation);
                else
                {
                    var position = hit.point + Vector3.up;
                    var rotation = playerOrigin.rotation;

                    player.Teleport(position, rotation);
                }
            }
        }

        protected virtual void RestoreState()
        {
            // Sprawdź, czy scena istnieje w danych postaci
            if (!currentCharacter.scenes.TryGetScene(currentScene.name, out var scene)) return;

            // Zaktualizuj dynamicznie listę entities
            UpdateTrackedEntities();

            // Przywróć stan dla każdej zapisanej jednostki w scenie
            for (int i = 0; i < scene.entities.Length; i++)
            {
                if (i >= entities.Length) break;

                var position = scene.entities[i].position;
                var rotation = Quaternion.Euler(scene.entities[i].rotation);

                if (scene.entities[i].health == 0)
                {
                    // Wyłącz obiekt, jeśli zdrowie wynosi 0
                    entities[i].gameObject.SetActive(false);
                }
                else
                {
                    // Upewnij się, że statystyki jednostki są poprawnie zainicjalizowane
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

            // Przywróć stan punktów nawigacyjnych
            for (int i = 0; i < scene.waypoints.Length; i++)
            {
                if (i >= waypoints.waypoints.Count) break;

                if (waypoints.waypoints[i].title.CompareTo(scene.waypoints[i].title) != 0)
                    continue;

                waypoints.waypoints[i].active = scene.waypoints[i].active;
            }

            // Przywróć bieżący punkt nawigacyjny, jeśli istnieje
            if (scene.currentWaypointIndex >= 0 &&
                scene.currentWaypointIndex < waypoints.waypoints.Count)
                waypoints.currentWaypoint = waypoints.waypoints[scene.currentWaypointIndex];

            // Przywróć stan przedmiotów misji i innych obiektów w scenie
            RestoreQuestItems(scene);
            RestoreGameObjects(scene);

            // Dostosuj statystyki istniejących jednostek do aktualnego poziomu trudności
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
            Game.instance.quests.ReachedScene(currentScene.name);

        protected override void Initialize()
        {
            Debug.Log("Initializing Level...");

            // Inicjalizuj gracza
            InitializePlayer();

            // Przywróć stan świata z zapisanego stanu
            RestoreState();

            // Zaktualizuj listę przeciwników w scenie
            UpdateTrackedEntities();

            // Zastosuj bieżące mnożniki trudności do wszystkich przeciwników
            ApplyDifficultyToEntities();

            // Sprawdź, czy wszystkie dane misji w scenie są aktualne
            EvaluateQuestScene();

            Debug.Log("Level initialized successfully.");
        }

        protected virtual void UpdateTrackedEntities()
        {
            // Znajdź wszystkie obiekty z tagiem "Entity/Enemy" na warstwie "Entities"
            var foundEntities = Object.FindObjectsByType<Entity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(entity => entity.CompareTag("Entity/Enemy") && entity.gameObject.layer == LayerMask.NameToLayer("Entities"))
                .ToArray();

            // Przypisz znalezione obiekty do listy entities
            entities = foundEntities;

            // Debugowanie
            Debug.Log($"Tracked {entities.Length} entities:");
            foreach (var entity in entities)
            {
                Debug.Log($" - {entity.name}");
            }
        }

        public virtual void AddEntityToTracking(Entity entity)
        {
            // Sprawdź, czy przeciwnik jest poprawny
            if (entity == null || !entity.CompareTag("Entity/Enemy") || entity.gameObject.layer != LayerMask.NameToLayer("Entities"))
                return;

            // Dodaj przeciwnika do listy
            var tempList = entities.ToList();
            if (!tempList.Contains(entity))
            {
                tempList.Add(entity);
                entities = tempList.ToArray();

                Debug.Log($"Entity {entity.name} added to tracking. Total entities: {entities.Length}");
            }
        }


        /// <summary>
        /// Applies the current difficulty settings to all tracked entities.
        /// </summary>
        protected virtual void ApplyDifficultyToEntities()
        {
            if (entities == null || entities.Length == 0)
            {
                Debug.LogWarning("No entities found to apply difficulty.");
                return;
            }

            foreach (var entity in entities)
            {
                if (entity != null && entity.CompareTag("Entity/Enemy"))
                {
                    if (entity.stats != null)
                    {
                        entity.stats.dexterity = (int)(entity.stats.dexterity * DifficultyManager.Instance.CurrentDexterityMultiplier);
                        entity.stats.strength = (int)(entity.stats.strength * DifficultyManager.Instance.CurrentStrengthMultiplier);

                        Debug.Log($"Adjusted stats for {entity.name}: Dexterity={entity.stats.dexterity}, Strength={entity.stats.strength}");
                    }
                    else
                    {
                        Debug.LogError($"Entity {entity.name} does not have stats initialized!");
                    }
                }
            }
        }

        protected virtual void Start() => EvaluateQuestScene();
    }
}