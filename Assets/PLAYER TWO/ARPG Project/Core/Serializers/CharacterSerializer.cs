//  ZMODYFIKOWANO 31 GRUDNIA 2024
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [System.Serializable]
    public class CharacterSerializer
    {
        public int characterId;

        public string name;
        public string scene;
        public string playerType;
        public string currentDynamicPlayerType;
        public string specialCondition;
        public Dictionary<int, int> selectedDialogPaths = new Dictionary<int, int>();
        public Dictionary<string, List<int>> viewedDialogPages = new Dictionary<string, List<int>>();

        public List<string> unlockedAchievements;
        public List<string> visitedZones = new List<string>();
        public List<int> activatedWaypoints = new List<int>();

        public UnitySerializer.Vector3 position;
        public UnitySerializer.Vector3 rotation;

        public StatsSerializer stats;
        public EquipmentsSerializer equipments;
        public InventorySerializer inventory;
        public SkillsSerializer skills;
        public QuestsSerializer quests;
        public ScenesSerializer scenes;

        public float dexterityMultiplier = 1.0f;
        public float strengthMultiplier = 1.0f;
        public float vitalityMultiplier = 1.0f;
        public float energyMultiplier = 1.0f;
        public float totalCombatTime;
        public float totalPlayTime = 0f; 

        public int health;
        public int mana;
        public int playerDeaths;
        public int enemiesDefeated;
        public int potionsUsed;
        public int difficultyMultiplier;
        public int zonesDiscovered;
        public int achievementsUnlocked;
        public int npcInteractions;
        public int questsCompleted;
        public int waypointsDiscovered = 0;
        
        public bool questionnaireCompleted = false;

        public CharacterSerializer(CharacterInstance character)
        {
            characterId = GameDatabase.instance.GetElementId<Character>(character.data);
            name = character.name;
            position = new UnitySerializer.Vector3(character.currentPosition);
            rotation = new UnitySerializer.Vector3(character.currentRotation.eulerAngles);
            scene = character.currentScene;
            stats = new StatsSerializer(character.stats);
            equipments = new EquipmentsSerializer(character.equipments);
            inventory = new InventorySerializer(character.inventory);
            skills = new SkillsSerializer(character.skills);
            quests = new QuestsSerializer(character.quests);
            scenes = new ScenesSerializer(character.scenes);

            health = character.savedHealth;
            mana   = character.savedMana;

            dexterityMultiplier = character.GetMultiplier("Dexterity");
            strengthMultiplier = character.GetMultiplier("Strength");
            vitalityMultiplier = character.GetMultiplier("Vitality");
            energyMultiplier = character.GetMultiplier("Energy");

            // Save logs
            playerDeaths = character.playerDeaths;
            enemiesDefeated = character.enemiesDefeated;
            totalCombatTime = character.totalCombatTime;
            npcInteractions = character.npcInteractions;
            questsCompleted = character.questsCompleted;
            potionsUsed = character.potionsUsed;
            waypointsDiscovered = character.waypointsDiscovered;
            zonesDiscovered = character.zonesDiscovered;
            playerType = character.playerType;
            currentDynamicPlayerType = character.currentDynamicPlayerType;
            totalPlayTime = character.totalPlayTime;

            questionnaireCompleted = character.questionnaireCompleted;
            specialCondition = character.specialCondition.ToString();

            unlockedAchievements = character.unlockedAchievements != null ? new List<string>(character.unlockedAchievements) : new List<string>();
            achievementsUnlocked = unlockedAchievements.Count;

            visitedZones = character.visitedZones != null ? new List<string>(character.visitedZones) : new List<string>();
            activatedWaypoints = character.activatedWaypoints != null ? new List<int>(character.activatedWaypoints) : new List<int>();

            viewedDialogPages = new Dictionary<string, List<int>>();
            foreach (var entry in character.viewedDialogPages)
            {
                viewedDialogPages[entry.Key] = new List<int>(entry.Value);
            }

            selectedDialogPaths = character.viewedDialogPages.Count == 0 ? new Dictionary<int, int>() : new Dictionary<int, int>(character.selectedDialogPaths);
        
            if (!Enum.TryParse(character.specialCondition.ToString(), out Affinity validCondition))
            {
                validCondition = Affinity.None;
            }
            specialCondition = validCondition.ToString();
        }

        public virtual string ToJson() => JsonUtility.ToJson(this);

        public static CharacterSerializer FromJson(string json) =>
            JsonUtility.FromJson<CharacterSerializer>(json);
    }
}