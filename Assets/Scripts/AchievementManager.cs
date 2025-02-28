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
            if (!logger.unlockedAchievements.Contains("First Blood") && logger.enemiesDefeated >= 1)
            {
                logger.LogAchievement("First Blood");
                ShowAchievementInUI("Achievement Unlocked!", "First Blood", "You defeated your first enemy.");
            }

            if (!logger.unlockedAchievements.Contains("Monster Slayer") && logger.enemiesDefeated >= 20)
            {
                logger.LogAchievement("Monster Slayer");
                ShowAchievementInUI("Achievement Unlocked!", "Monster Slayer", "You have slain 20 enemies.");
            }

            if (!logger.unlockedAchievements.Contains("Level Master"))
            {
                var character = Game.instance?.currentCharacter;
                if (character != null && character.stats.currentLevel >= 10)
                {
                    logger.LogAchievement("Level Master");
                    ShowAchievementInUI("Achievement Unlocked!", "Level Master", "You've reached level 10!");
                }
            }

            if (!logger.unlockedAchievements.Contains("Quest Hero") && logger.questsCompleted >= 4)
            {
                logger.LogAchievement("Quest Hero");
                ShowAchievementInUI("Achievement Unlocked!", "Quest Hero", "You've completed 4 quests.");
            }

            if (!logger.unlockedAchievements.Contains("Adventurer") && (logger.zonesDiscovered >= 5 || logger.waypointsDiscovered >= 5))
            {
                Debug.Log("Achievement unlocked: Adventurer!");
                logger.LogAchievement("Adventurer");
                ShowAchievementInUI("Achievement Unlocked!", "Adventurer", "You explored 5 zones or waypoints.");
            }

            if (!logger.unlockedAchievements.Contains("Unstoppable") && logger.playerDeaths == 0 && logger.enemiesDefeated >= 100)
            {
                Debug.Log("Achievement unlocked: Unstoppable!");
                logger.LogAchievement("Unstoppable");
                ShowAchievementInUI("Achievement Unlocked!", "Unstoppable", "You defeated 100 enemies without dying.");
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