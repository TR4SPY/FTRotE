using UnityEngine;
using System.Collections.Generic;
using AI_DDA.Assets.Scripts.Achievements;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    public class AchievementManager : MonoBehaviour
    {
        private Coroutine currentCoroutine;
        public GUIAchievementsHUD GUIAchievementsHUD;

        [SerializeField]
        public List<AchievementData> achievements = new List<AchievementData>();

        public void CheckAchievements(PlayerBehaviorLogger logger)
        {
            var character = Game.instance?.currentCharacter;
            if (character == null) return;

            var entity = character.Instantiate();

            foreach (var achievement in achievements)
            {
                if (achievement == null) continue;

                if (!logger.unlockedAchievements.Contains(achievement.id) && achievement.IsMet(logger, character))
                {
                    logger.LogAchievement(achievement.id);
                    GiveAchievementReward(character, entity, achievement.experience, achievement.coins, achievement.extraRewards, achievement.additionalExperience, achievement.additionalCoins);
                    ShowAchievementInUI("Achievement Unlocked!", achievement.achievementName, achievement.description);
                }
            }
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