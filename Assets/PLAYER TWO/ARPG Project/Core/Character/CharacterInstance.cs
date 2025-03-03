using System;
using UnityEngine;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    public enum SpecialCondition
        {
            None,
            Light,
            Dark
        }

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

        public HashSet<string> visitedZones = new HashSet<string>();
        public HashSet<int> activatedWaypoints = new HashSet<int>();
        public HashSet<int> viewedDialogPages = new HashSet<int>();

        public Dictionary<int, int> selectedDialogPaths = new Dictionary<int, int>();

        public List<string> unlockedAchievements = new List<string>();

        public CharacterEquipments equipments;
        public CharacterInventory inventory;
        public CharacterSkills skills;
        public CharacterQuests quests;
        public CharacterScenes scenes;

        public int playerDeaths = 0;
        public int enemiesDefeated = 0;
        public float totalCombatTime = 0f;
        public int potionsUsed = 0;
        public int difficultyMultiplier = 0;
        public int zonesDiscovered = 0;
       // public int achievementsUnlocked = 0;
        public int npcInteractions = 0;
        public int questsCompleted = 0;
        public int waypointsDiscovered = 0;
        public int achievementsUnlocked = 0;
        public bool questionnaireCompleted = false;
        public string playerType = "Undefined";
        public string currentDynamicPlayerType = "Unknown";
        public float totalPlayTime = 0f;
        protected Entity m_entity;

        public Vector3 currentPosition => m_entity ? m_entity.position : initialPosition;
        public Quaternion currentRotation => m_entity ? m_entity.transform.rotation : initialRotation;
        public bool HasViewedDialogPage(int pageIndex) => viewedDialogPages.Contains(pageIndex);
        
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

        public Dictionary<string, GameObject> GetEquippedPrefabs()
        {
            var equippedPrefabs = new Dictionary<string, GameObject>();

            if (equipments.currentRightHand?.data?.prefab != null)
            {
                equippedPrefabs.Add("RightHand", equipments.currentRightHand.data.prefab);
                Debug.Log($"RightHand prefab: {equipments.currentRightHand.data.prefab.name}");
                
            }

            if (equipments.currentLeftHand?.data?.prefab != null)
            {
                equippedPrefabs.Add("LeftHand", equipments.currentLeftHand.data.prefab);
                Debug.Log($"LeftHand prefab: {equipments.currentLeftHand.data.prefab.name}");
            }

            if (equipments.currentHelm?.data?.prefab != null)
            {
                equippedPrefabs.Add("Head", equipments.currentHelm.data.prefab);
                Debug.Log($"Head prefab: {equipments.currentHelm.data.prefab.name}");
            }

            if (equipments.currentChest?.data?.prefab != null)
            {
                equippedPrefabs.Add("Chest", equipments.currentChest.data.prefab);
                Debug.Log($"Chest prefab: {equipments.currentChest.data.prefab.name}");
            }

            if (equipments.currentPants?.data?.prefab != null)
            {
                equippedPrefabs.Add("Pants", equipments.currentPants.data.prefab);
                Debug.Log($"Pants prefab: {equipments.currentPants.data.prefab.name}");
            }

            if (equipments.currentGloves?.data?.prefab != null)
            {
                equippedPrefabs.Add("Gloves", equipments.currentGloves.data.prefab);
                Debug.Log($"Gloves prefab: {equipments.currentGloves.data.prefab.name}");
            }

            if (equipments.currentBoots?.data?.prefab != null)
            {
                equippedPrefabs.Add("Boots", equipments.currentBoots.data.prefab);
                Debug.Log($"Boots prefab: {equipments.currentBoots.data.prefab.name}");
            }

            return equippedPrefabs;
        }

        public void MarkDialogPageAsViewed(int pageIndex)
        {
            if (!viewedDialogPages.Contains(pageIndex))
            {
                viewedDialogPages.Add(pageIndex);
            }
        }

        public void SetDialogPathChoice(int fromPage, int toPage)
        {
            selectedDialogPaths[fromPage] = toPage;
        }

        public int GetDialogNextPage(int currentPage)
        {
            return selectedDialogPaths.ContainsKey(currentPage) ? selectedDialogPaths[currentPage] : -1;
        }

        public string GetSpecialConditionAsString()
        {
            return specialCondition.ToString();
        }

        public SpecialCondition specialCondition = SpecialCondition.None; // ✅ Domyślnie ustawiamy `None`

            public void SetSpecialCondition(SpecialCondition condition)
            {
                specialCondition = condition;
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

                playerDeaths = serializer.playerDeaths,
                enemiesDefeated = serializer.enemiesDefeated,
                totalCombatTime = serializer.totalCombatTime,
                potionsUsed = serializer.potionsUsed,
                difficultyMultiplier = serializer.difficultyMultiplier,
                zonesDiscovered = serializer.zonesDiscovered,
                npcInteractions = serializer.npcInteractions,
                questsCompleted = serializer.questsCompleted,
                waypointsDiscovered = serializer.waypointsDiscovered,
                questionnaireCompleted = serializer.questionnaireCompleted,
                playerType = serializer.playerType,
                currentDynamicPlayerType = serializer.currentDynamicPlayerType,
                totalPlayTime = serializer.totalPlayTime,

                viewedDialogPages = serializer.viewedDialogPages.Count == 0 ? new HashSet<int>() : new HashSet<int>(serializer.viewedDialogPages),
                selectedDialogPaths = new Dictionary<int, int>(),

                visitedZones = serializer.visitedZones != null
                    ? new HashSet<string>(serializer.visitedZones)
                    : new HashSet<string>(),

                activatedWaypoints = serializer.activatedWaypoints != null
                    ? new HashSet<int>(serializer.activatedWaypoints)
                    : new HashSet<int>(),

                unlockedAchievements = serializer.unlockedAchievements != null
                    ? new List<string>(serializer.unlockedAchievements)
                    : new List<string>(),

                achievementsUnlocked = serializer.unlockedAchievements != null
                    ? serializer.unlockedAchievements.Count
                    : 0
            };

            if (!Enum.TryParse(serializer.specialCondition, out SpecialCondition parsedCondition))
            {
                Debug.LogError($"[AI-DDA] Błąd wczytywania specialCondition: {serializer.specialCondition}. Ustawiono 'None'.");
                parsedCondition = SpecialCondition.None;
            }
            characterInstance.specialCondition = parsedCondition;
            
            // Wczytaj mnożniki trudności
            characterInstance.SetMultiplier("Dexterity", serializer.dexterityMultiplier);
            characterInstance.SetMultiplier("Strength", serializer.strengthMultiplier);
            characterInstance.SetMultiplier("Vitality", serializer.vitalityMultiplier);
            characterInstance.SetMultiplier("Energy", serializer.energyMultiplier);

            return characterInstance;
        }
    }
}
