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
                scenes = CharacterScenes.CreateFromSerializer(serializer.scenes)
            };

            // Wczytaj mnożniki trudności
            characterInstance.SetMultiplier("Dexterity", serializer.dexterityMultiplier);
            characterInstance.SetMultiplier("Strength", serializer.strengthMultiplier);
            characterInstance.SetMultiplier("Speed", serializer.speedMultiplier);

            return characterInstance;
        }
    }
}
