//  ZMODYFIKOWANO 31 GRUDNIA 2024

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [System.Serializable]
    public struct SpecializationEntry
    {
        public int key;
        public int value;
    }

    [System.Serializable]
    public struct ReputationEntry
    {
        public Faction faction;
        public int value;
    }


    [System.Serializable]
    public class CharacterSerializer
    {
        public Dictionary<int, int> selectedDialogPaths = new Dictionary<int, int>();
        public Dictionary<string, List<int>> viewedDialogPages = new Dictionary<string, List<int>>();

        public List<SpecializationEntry> selectedSpecializations = new List<SpecializationEntry>();
        public List<SpecializationEntry> specializationSkillPoints = new List<SpecializationEntry>();
        public List<ReputationEntry> reputationEntries = new List<ReputationEntry>();

        public List<string> unlockedAchievements;
        public List<string> visitedZones = new List<string>();

        public List<int> activatedWaypoints = new List<int>();
        public List<int> unlockedSpecializationTiers = new List<int>();
        public List<BestiaryEntrySaveData> bestiaryEntries = new List<BestiaryEntrySaveData>();

        public UnitySerializer.Vector3 position;
        public UnitySerializer.Vector3 rotation;

        public StatsSerializer stats;
        public EquipmentsSerializer equipments;
        public InventorySerializer inventory;
        public SkillsSerializer skills;
        public QuestsSerializer quests;
        public ScenesSerializer scenes;
        public BuffsSerializer buffs;

        public float dexterityMultiplier = 1.0f;
        public float strengthMultiplier = 1.0f;
        public float vitalityMultiplier = 1.0f;
        public float energyMultiplier = 1.0f;
        public float savedDifficulty;
        public float totalCombatTime;
        public float totalPlayTime = 0f;

        public int characterId;
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

        public string name;
        public string scene;
        public string playerType;
        public string currentDynamicPlayerType;
        public string specialCondition;
        public string guildName;
        public string guildCrestData;

        public bool questionnaireCompleted = false;
        public bool storylineCompleted = false;

        public CharacterSerializer(CharacterInstance character)
        {
            characterId = GameDatabase.instance.GetElementId<Character>(character.data);
            name = character.name;
            position = new UnitySerializer.Vector3(character.currentPosition);
            rotation = new UnitySerializer.Vector3(character.currentRotation.eulerAngles);
            scene = character.currentScene;
            var buffManager = character.Entity != null ? character.Entity.GetComponent<EntityBuffManager>() : null;
            stats = new StatsSerializer(character.stats, buffManager);
            equipments = new EquipmentsSerializer(character.equipments);
            inventory = new InventorySerializer(character.inventory);
            skills = new SkillsSerializer(character.skills);
            quests = new QuestsSerializer(character.quests);
            scenes = new ScenesSerializer(character.scenes);
            if (buffManager != null)
                buffs = new BuffsSerializer(buffManager);
            else if (character.buffs != null)
                buffs = character.buffs;
            else
                buffs = new BuffsSerializer();

            health = character.savedHealth;
            mana   = character.savedMana;

            dexterityMultiplier = character.GetMultiplier("Dexterity");
            strengthMultiplier = character.GetMultiplier("Strength");
            vitalityMultiplier = character.GetMultiplier("Vitality");
            energyMultiplier = character.GetMultiplier("Energy");

            savedDifficulty = character.savedDifficulty; 

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
            
            bestiaryEntries = character.bestiarySaveData != null && character.bestiarySaveData.Count > 0
                ? character.bestiarySaveData
                    .OrderBy(e => e.enemyId)
                    .Select(e => new BestiaryEntrySaveData
                    {
                        enemyId = e.enemyId,
                        encounters = e.encounters,
                        kills = e.kills
                    })
                    .ToList()
                : character.bestiary != null
                    ? character.bestiary.Values
                        .OrderBy(e => e.enemyId)
                        .Select(e => new BestiaryEntrySaveData
                        {
                            enemyId = e.enemyId,
                            encounters = e.encounters,
                            kills = e.kills
                        })
                        .ToList()
                    : new List<BestiaryEntrySaveData>();

            storylineCompleted = character.storylineCompleted;
            questionnaireCompleted = character.questionnaireCompleted;
            specialCondition = character.specialCondition.ToString();

            guildName = character.guildName;
            guildCrestData = character.guildCrestData;

            if (character.reputation != null)
            {
                foreach (var kvp in character.reputation)
                {
                    reputationEntries.Add(new ReputationEntry { faction = kvp.Key, value = kvp.Value });
                }
            }

            
            if (character.specializations != null)
            {
                foreach (var kvp in character.specializations.selected)
                {
                    if (kvp.Value != null)
                        selectedSpecializations.Add(new SpecializationEntry { key = kvp.Key, value = kvp.Value.id });
                }

                foreach (var kvp in character.specializations.skillPoints)
                {
                    if (kvp.Key != null)
                        specializationSkillPoints.Add(new SpecializationEntry { key = kvp.Key.id, value = kvp.Value });
                }

                unlockedSpecializationTiers = new List<int>(character.specializations.GetUnlockedTiersInstance());
            }
            else
            {
                unlockedSpecializationTiers = new List<int>();
            }

            unlockedAchievements = character.unlockedAchievements != null ? new List<string>(character.unlockedAchievements) : new List<string>();
            achievementsUnlocked = unlockedAchievements.Count;

            visitedZones = character.visitedZones != null ? new List<string>(character.visitedZones) : new List<string>();
            activatedWaypoints = character.activatedWaypoints != null ? new List<int>(character.activatedWaypoints) : new List<int>();

            viewedDialogPages = new Dictionary<string, List<int>>();
            foreach (var entry in character.viewedDialogPages)
            {
                viewedDialogPages[entry.Key] = new List<int>(entry.Value);
            }

            selectedDialogPaths = character.selectedDialogPaths != null
                ? new Dictionary<int, int>(character.selectedDialogPaths)
                : new Dictionary<int, int>();

            if (!Enum.TryParse(character.specialCondition.ToString(), out Affinity validCondition))
            {
                validCondition = Affinity.None;
            }
            specialCondition = validCondition.ToString();
        }

        public virtual string ToJson() => JsonUtility.ToJson(this);

        public static CharacterSerializer FromJson(string json)
        {
            var serializer = JsonUtility.FromJson<CharacterSerializer>(json);
            if (serializer.unlockedSpecializationTiers == null)
                serializer.unlockedSpecializationTiers = new List<int>();
            if (serializer.bestiaryEntries == null)
                serializer.bestiaryEntries = new List<BestiaryEntrySaveData>();
            if (serializer.reputationEntries == null)
                serializer.reputationEntries = new List<ReputationEntry>();
            return serializer;
        }
    }
}