//  ZMODYFIKOWANO 31 GRUDNIA 2024

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
        
        public Dictionary<int, int> selectedDialogPaths = new Dictionary<int, int>();

        public List<string> unlockedAchievements;
        public List<string> visitedZones = new List<string>();
        public List<int> activatedWaypoints = new List<int>();
        public List<int> viewedDialogPages = new List<int>();

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

            unlockedAchievements = character.unlockedAchievements != null ? new List<string>(character.unlockedAchievements) : new List<string>();
            achievementsUnlocked = unlockedAchievements.Count;

            visitedZones = character.visitedZones != null ? new List<string>(character.visitedZones) : new List<string>();
            activatedWaypoints = character.activatedWaypoints != null ? new List<int>(character.activatedWaypoints) : new List<int>();

            viewedDialogPages = new List<int>(character.viewedDialogPages);
            selectedDialogPaths = new Dictionary<int, int>(character.selectedDialogPaths);
            
            Debug.Log($"Character '{name}' serialized with visited zones: {string.Join(", ", visitedZones)}, activated waypoints: {string.Join(", ", activatedWaypoints)}, and logs: PlayerDeaths={playerDeaths}, EnemiesDefeated={enemiesDefeated}, AchievementsUnlocked={achievementsUnlocked}");
        }

        public virtual string ToJson() => JsonUtility.ToJson(this);

        public static CharacterSerializer FromJson(string json) =>
            JsonUtility.FromJson<CharacterSerializer>(json);
    }
}