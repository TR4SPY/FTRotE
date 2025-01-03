using UnityEngine;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    public class CharacterInstance
    {
        public Character data;

        public string name;
        public string currentScene;

        public Vector3 initialPosition;
        public Quaternion initialRotation;

        public CharacterStats stats;

        // Słownik do przechowywania mnożników trudności
        private Dictionary<string, float> multipliers = new Dictionary<string, float>
        {
            { "Dexterity", 1.0f },
            { "Strength", 1.0f },
            { "Speed", 1.0f }
        };

        // Zbiór odwiedzonych stref
        public HashSet<string> visitedZones = new HashSet<string>();

        // Zbiór odwiedzonych waypointów
        public HashSet<int> activatedWaypoints = new HashSet<int>();

        public CharacterEquipments equipments;
        public CharacterInventory inventory;
        public CharacterSkills skills;
        public CharacterQuests quests;
        public CharacterScenes scenes;

        // Logi i statystyki gracza
        public int playerDeaths = 0;
        public int enemiesDefeated = 0;
        public float totalCombatTime = 0f;
        public int potionsUsed = 0;
        public int difficultyMultiplier = 0;
        public int zonesDiscovered = 0;
        public int npcInteractions = 0;
        public int questsCompleted = 0;
        public int waypointsDiscovered = 0;

        protected Entity m_entity;

        public Vector3 currentPosition => m_entity ? m_entity.position : initialPosition;
        public Quaternion currentRotation => m_entity ? m_entity.transform.rotation : initialRotation;

        public CharacterInstance() { }

        public CharacterInstance(Character data, string name)
        {
            this.data = data;
            this.name = name;
            currentScene = data.initialScene;
            stats = new CharacterStats(data);
            equipments = new CharacterEquipments(data);
            inventory = new CharacterInventory(data);
            skills = new CharacterSkills(data);
            quests = new CharacterQuests();
            scenes = new CharacterScenes();
        }

        /// <summary>
        /// Pobierz wartość mnożnika dla danej statystyki.
        /// </summary>
        public float GetMultiplier(string statName)
        {
            if (multipliers.ContainsKey(statName))
            {
                return multipliers[statName];
            }
            return 1.0f; // Domyślna wartość, jeśli statystyka nie istnieje
        }

        /// <summary>
        /// Ustaw wartość mnożnika dla danej statystyki.
        /// </summary>
        public void SetMultiplier(string statName, float value)
        {
            if (multipliers.ContainsKey(statName))
            {
                multipliers[statName] = value;
            }
            else
            {
                multipliers.Add(statName, value);
            }
        }

        // Metoda do sprawdzania, czy waypoint został odwiedzony
        public bool HasActivatedWaypoint(int waypointID) => activatedWaypoints.Contains(waypointID);

        // Metoda do oznaczania waypointu jako odwiedzonego
        public void MarkWaypointAsActivated(int waypointID)
        {
            if (!activatedWaypoints.Contains(waypointID))
            {
                activatedWaypoints.Add(waypointID);
                waypointsDiscovered++; // Aktualizuj licznik waypointów
                Debug.Log($"Waypoint '{waypointID}' marked as visited for character '{name}'.");
            }
        }

        // Metoda do sprawdzania, czy strefa została odwiedzona
        public bool HasVisitedZone(string zoneId) => visitedZones.Contains(zoneId);

        // Metoda do oznaczania strefy jako odwiedzonej
        public void MarkZoneAsVisited(string zoneId)
        {
            if (!visitedZones.Contains(zoneId))
            {
                visitedZones.Add(zoneId);
                Debug.Log($"Zone '{zoneId}' marked as visited for character '{name}'.");
            }
        }

        /// <summary>
        /// Instantiates a new Entity from this Character Instance data.
        /// </summary>
        public virtual Entity Instantiate()
        {
            if (m_entity == null)
            {
                m_entity = GameObject.Instantiate(data.entity);
                stats.InitializeStats(m_entity.stats);
                equipments.InitializeEquipments(m_entity.items);
                inventory.InitializeInventory(m_entity.inventory);
                skills.InitializeSkills(m_entity.skills);
                quests.InitializeQuests();
                scenes.InitializeScenes();
            }

            return m_entity;
        }

        public static CharacterInstance CreateFromSerializer(CharacterSerializer serializer)
        {
            var data = GameDatabase.instance.FindElementById<Character>(serializer.characterId);

            var characterInstance = new CharacterInstance()
            {
                data = data,
                name = serializer.name,
                currentScene = serializer.scene,
                initialPosition = serializer.position.ToUnity(),
                initialRotation = Quaternion.Euler(serializer.rotation.ToUnity()),
                stats = CharacterStats.CreateFromSerializer(serializer.stats),
                equipments = CharacterEquipments.CreateFromSerializer(serializer.equipments),
                inventory = CharacterInventory.CreateFromSerializer(serializer.inventory),
                skills = CharacterSkills.CreateFromSerializer(serializer.skills),
                quests = CharacterQuests.CreateFromSerializer(serializer.quests),
                scenes = CharacterScenes.CreateFromSerializer(serializer.scenes),

                // Wczytaj logi
                playerDeaths = serializer.playerDeaths,
                enemiesDefeated = serializer.enemiesDefeated,
                totalCombatTime = serializer.totalCombatTime,
                potionsUsed = serializer.potionsUsed,
                difficultyMultiplier = serializer.difficultyMultiplier,
                zonesDiscovered = serializer.zonesDiscovered,
                npcInteractions = serializer.npcInteractions,
                questsCompleted = serializer.questsCompleted,
                waypointsDiscovered = serializer.waypointsDiscovered, // Dodaj wczytywanie waypointów

                // Wczytaj odwiedzone strefy
                visitedZones = serializer.visitedZones != null
                    ? new HashSet<string>(serializer.visitedZones)
                    : new HashSet<string>(),

                // Wczytaj odwiedzone waypointy
                activatedWaypoints = serializer.activatedWaypoints != null
                    ? new HashSet<int>(serializer.activatedWaypoints)
                    : new HashSet<int>()
            };

            // Wczytaj mnożniki trudności
            characterInstance.SetMultiplier("Dexterity", serializer.dexterityMultiplier);
            characterInstance.SetMultiplier("Strength", serializer.strengthMultiplier);
            characterInstance.SetMultiplier("Speed", serializer.speedMultiplier);

            characterInstance.playerDeaths = serializer.playerDeaths;
            characterInstance.enemiesDefeated = serializer.enemiesDefeated;
            characterInstance.totalCombatTime = serializer.totalCombatTime;
            characterInstance.npcInteractions = serializer.npcInteractions;
            characterInstance.questsCompleted = serializer.questsCompleted;
            characterInstance.potionsUsed = serializer.potionsUsed;
            characterInstance.waypointsDiscovered = serializer.waypointsDiscovered;
            characterInstance.zonesDiscovered = serializer.zonesDiscovered;

            return characterInstance;
        }
    }
}
