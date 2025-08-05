using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    public class AchievementManager : MonoBehaviour
    {
        private Coroutine currentCoroutine;
        public GUIAchievementsHUD GUIAchievementsHUD;

        public void CheckAchievements(PlayerBehaviorLogger logger)
        {
            var character = Game.instance?.currentCharacter;
            if (character == null) return;

            var entity = character.Instantiate();

            Dictionary<string, (int experience, int coins, bool extraRewards, int additionalExperience, int additionalCoins)> achievements = new()
            {
                { "First Blood", (500, 1000, false, 0, 0) },
                { "Monster Slayer", (100, 25, false, 0, 0) },
                { "Level Master", (2000, 500, true, 1000, 200) },
                { "Quest Hero", (1500, 1000, true, 5000, 2000) },
                { "Adventurer", (100, 200, true, 500, 500) },
                { "Unstoppable", (500, 100, true, 200, 50) }
            };

            foreach (var achievement in achievements)
            {
                string name = achievement.Key;
                int experience = achievement.Value.experience;
                int coins = achievement.Value.coins;
                bool extraRewards = achievement.Value.extraRewards;
                int additionalExperience = achievement.Value.additionalExperience;
                int additionalCoins = achievement.Value.additionalCoins;

                if (!logger.unlockedAchievements.Contains(name) && AchievementConditionMet(name, logger, character))
                {
                    logger.LogAchievement(name);
                    GiveAchievementReward(character, entity, experience, coins, extraRewards, additionalExperience, additionalCoins);
                    ShowAchievementInUI("Achievement Unlocked!", name, GetAchievementDescription(name));
                }
            }
        }

        private bool AchievementConditionMet(string achievementName, PlayerBehaviorLogger logger, CharacterInstance character)
        {
            return achievementName switch
            {
                "First Blood" => logger.enemiesDefeated >= 1,
                "Monster Slayer" => logger.enemiesDefeated >= 20,
                "Level Master" => character.stats.currentLevel >= 10,
                "Quest Hero" => logger.questsCompleted >= 4,
                "Adventurer" => logger.zonesDiscovered >= 5 || logger.waypointsDiscovered >= 5,
                "Unstoppable" => logger.playerDeaths == 0 && logger.enemiesDefeated >= 100,
                _ => false
            };
        }

        private void GiveAchievementReward(CharacterInstance character, Entity entity, int experience, int coins, bool extraRewards, int additionalExperience, int additionalCoins)
        {
            Debug.Log($"[AI-DDA] Checking GiveAchievementReward() - Character: {(character != null ? character.name : "NULL")}, Entity: {(entity != null ? entity.name : "NULL")}");

            if (character == null)
            {
                Debug.LogError("[AI-DDA] CharacterInstance is NULL in GiveAchievementReward!");
                return;
            }
            
            if (entity == null)
            {
                Debug.LogError("[AI-DDA] Entity is NULL in GiveAchievementReward!");
                return;
            }

            int finalExperience = experience;
            int finalCoins = coins;

            if (extraRewards && character.currentDynamicPlayerType == "Achiever")
            {
                finalExperience += additionalExperience;
                finalCoins += additionalCoins;
            }

            if (entity.stats != null)
            {
                    entity.stats.AddExperience(finalExperience);
                Debug.Log($"[AI-DDA] {entity.name} received {finalExperience} EXP.");
            }
            else
            {
                Debug.LogError($"[AI-DDA] Entity.stats is NULL for {entity.name}!");
            }

            if (entity.inventory != null)
            {                
                int fcoins = finalCoins;
                if (entity.stats != null)
                    fcoins = Mathf.RoundToInt(fcoins * (1f + entity.stats.additionalMoneyRewardPercent / 100f));
                entity.inventory.instance.money += fcoins;
            }
            else
            {
                Debug.LogError($"[AI-DDA] Entity.inventory is NULL for {entity.name}!");
            }
        }

        private string GetAchievementDescription(string achievementName)
        {
            return achievementName switch
            {
                "First Blood" => "You defeated your first enemy.",
                "Monster Slayer" => "You have slain 20 enemies.",
                "Level Master" => "You've reached level 10!",
                "Quest Hero" => "You've completed 4 quests.",
                "Adventurer" => "You explored 5 zones or waypoints.",
                "Unstoppable" => "You defeated 100 enemies without dying.",
                _ => "Unknown achievement."
            };
        }

        private Queue<(string status, string name, string description)> achievementQueue = new Queue<(string, string, string)>();

        public void ShowAchievementInUI(string status, string name, string description)
        {
            if (GUIAchievementsHUD == null)
            {
                #if UNITY_2023_1_OR_NEWER
                GUIAchievementsHUD = Object.FindFirstObjectByType<GUIAchievementsHUD>();
                #else
                GUIAchievementsHUD = Object.FindObjectOfType<GUIAchievementsHUD>();
                #endif

                if (GUIAchievementsHUD == null)
                {
                    Debug.LogWarning($"GUIAchievementsHUD is NULL. Queuing achievement: {name}");
                    achievementQueue.Enqueue((status, name, description));
                    return;
                }
            }

            Debug.Log($"Displaying achievement: {name}");
            GUIAchievementsHUD.ShowAchievement(status, name, description);
        }

        private void Update()
        {
            if (GUIAchievementsHUD != null && achievementQueue.Count > 0)
            {
                var achievement = achievementQueue.Dequeue();
                Debug.Log($"Displaying queued achievement: {achievement.name}");
                GUIAchievementsHUD.ShowAchievement(achievement.status, achievement.name, achievement.description);
            }
        }

        private void Awake()
        {
            if (GUIAchievementsHUD == null)
            {
                #if UNITY_2023_1_OR_NEWER
                GUIAchievementsHUD = Object.FindFirstObjectByType<GUIAchievementsHUD>();
                #else
                GUIAchievementsHUD = Object.FindObjectOfType<GUIAchievementsHUD>();
                #endif

                if (GUIAchievementsHUD == null)
                {
                   // Debug.LogError("GUIAchievementsHUD not found in the scene during Awake!");
                }
                else
                {
                   // Debug.Log("GUIAchievementsHUD successfully initialized in Awake.");
                }
            }
        }
    }
}