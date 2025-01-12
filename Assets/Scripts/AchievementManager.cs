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

        private bool firstBloodUnlocked = false;
        private bool adventurerUnlocked = false;
        private bool unstoppableUnlocked = false;

    public void CheckAchievements(PlayerBehaviorLogger logger)
    {
        // Używamy warunku z !firstBloodUnlocked
        if (!firstBloodUnlocked && logger.enemiesDefeated >= 1)
        {
            firstBloodUnlocked = true; // Zaznaczamy, że osiągnięcie przyznano
            Debug.Log("Achievement unlocked: First Blood!");
            logger.LogAchievement("First Blood");
            ShowAchievementInUI("Achievement Unlocked!", "First Blood", "You defeated your first enemy.");
        }

        if (!adventurerUnlocked && (logger.zonesDiscovered >= 5 || logger.waypointsDiscovered >= 5))
        {
            adventurerUnlocked = true;
            Debug.Log("Achievement unlocked: Adventurer!");
            logger.LogAchievement("Adventurer");
            ShowAchievementInUI("Achievement Unlocked!", "Adventurer", "You explored 5 zones or waypoints.");
        }

        if (!unstoppableUnlocked && logger.playerDeaths == 0 && logger.enemiesDefeated >= 100)
        {
            unstoppableUnlocked = true;
            Debug.Log("Achievement unlocked: Unstoppable!");
            logger.LogAchievement("Unstoppable");
            ShowAchievementInUI("Achievement Unlocked!", "Unstoppable", "You defeated 100 enemies without dying.");
        }
    }

        public void ShowAchievementInUI(string status, string name, string description)
        {
            #if UNITY_2023_1_OR_NEWER
            GUIAchievementsHUD = Object.FindFirstObjectByType<GUIAchievementsHUD>();
#else
            GUIAchievementsHUD = Object.FindObjectOfType<GUIAchievementsHUD>();
#endif

            if (GUIAchievementsHUD == null)
            {
                Debug.LogError("GUIAchievementsHUD not found in the scene.");
            }
        }
    }
}