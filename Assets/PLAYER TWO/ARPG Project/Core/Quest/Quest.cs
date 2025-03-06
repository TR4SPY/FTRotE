using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [CreateAssetMenu(fileName = "New Quest", menuName = "PLAYER TWO/ARPG Project/Quest/Quest")]
    public class Quest : ScriptableObject
    {
        public enum CompletingMode { ReachScene, Progress, Trigger, FetchAfterKill }

        [Header("Main Settings")]
        [Tooltip("The title of the Quest.")]
        public string title;

        [TextArea(5, 5)]
        [Tooltip("A description with details of the Quest lore.")]
        public string description;

        [Tooltip("A short description of the Quest objective.")]
        public string objective;

        [Tooltip("The amount of progression points required to complete the Quest.")]
        public int targetProgress;

        [Tooltip("The completing mode of the Quest.")]
        public CompletingMode completingMode;

        [Header("Reward Settings")]
        [Tooltip("The amount of experience points gained by completing this Quest.")]
        public int experience;

        [Tooltip("The amount of coins gained by completing this Quest.")]
        public int coins;

        [Tooltip("The items gained by completing this Quest.")]
        public QuestItemReward[] items;

        [Header("Progression Settings")]
        [Tooltip("The name of the destination scene used when the completing mode is 'Reach Scene.'")]
        public string destinationScene;

        [Tooltip("The key of the progress, e.g. name of the enemy, used when the completing mode is 'Progress.'")]
        public string progressKey;

        [Header("Fetch Quest Settings")]
        [Tooltip("The item the player needs to return.")]
        public QuestItemReward requiredItem;

        [Tooltip("The name of the NPC who will receive the item.")]
        public string returnToNPC;
        
        [Tooltip("If true, the player must manually complete the quest at the NPC.")]
        public bool requiresManualCompletion;

        [Header("Killer Bonus")]
        [Tooltip("Multiplier for the number of enemies required if the player is a Killer.")]
        public float killerMultiplier = 1.5f;
        [Tooltip("Gold multiplier for Killer when completing a quest.")]
        public float killerGoldMultiplier = 1.1f;
        [Tooltip("Exp multiplier for Killer.")]
        public float killerExpMultiplier = 0.8f;

        [Header("Socializer Bonus")]
        [Tooltip("Multiplier for the number of enemies required if the player is a Socializer.")]
        public float socializerMultiplier = 0.50f;
        [Tooltip("Gold multiplier for Socializer when completing a quest.")]
        public float socializerGoldMultiplier = 1.25f;
        [Tooltip("Exp multiplier for Socializer.")]
        public float socializerExpMultiplier = 1.0f;

        [Header("Explorer Bonus")]
        [Tooltip("Multiplier for the number of enemies required if the player is an Explorer.")]
        public float explorerMultiplier = 1.2f;
        [Tooltip("Exp multiplier for Explorer.")]
        public float explorerExpMultiplier = 1.25f;
        [Tooltip("Gold multiplier for Explorer when completing a quest.")]
        public float explorerGoldMultiplier = 0.9f;

        [Header("Achiever Bonus")]
        [Tooltip("Multiplier for the number of enemies required if the player is an Achiever.")]
        public float achieverMultiplier = 1.0f;
        [Tooltip("Multiplier for the experience reward if the player is an Achiever.")]
        public float achieverExpMultiplier = 1.5f;
        [Tooltip("Multiplier for the gold reward if the player is an Achiever.")]
        public float achieverGoldMultiplier = 1.5f;

        /// <summary>
        /// Returns true if this Quest has any rewards.
        /// </summary>
        public bool hasReward => experience > 0 || coins > 0 || items.Length > 0;

        /// <summary>
        /// Returns true if the completing mode of this Quest is 'Reach Scene.'
        /// </summary>
        public bool IsReachScene() => completingMode == CompletingMode.ReachScene;

        /// <summary>
        /// Returns true if the completing mode of this Quest is 'Progress.'
        /// </summary>
        public bool IsProgress() => completingMode == CompletingMode.Progress;

        /// <summary>
        /// Returns true if the completing mode of this Quest is 'Trigger.'
        /// </summary>
        public bool IsTrigger() => completingMode == CompletingMode.Trigger;

        public bool IsFetchAfterKill() => completingMode == CompletingMode.FetchAfterKill;

        /// <summary>
        /// Returns true if a given scene name matches the Quest's destination scene name.
        /// </summary>
        /// <param name="scene">The name of the scene you want to compare.</param>
        public bool IsDestinationScene(string scene) => scene.CompareTo(destinationScene) == 0;

        /// <summary>
        /// Returns true if a given progress key matches the Quest's progress key.
        /// </summary>
        /// <param name="key">The progress key you want to compare.</param>
        public bool IsProgressKey(string key) => progressKey.CompareTo(key) == 0;
        

        /// <summary>
        /// Returns the formatted reward text.
        /// </summary>
        public string GetRewardText()
        {
            var text = "";
            int finalExperience = GetTotalExperience();
            int finalCoins = GetTotalCoins();

            if (!hasReward) return "None";
            if (finalExperience > 0) text += $"{finalExperience} exp";
            if (finalCoins > 0) text += $"\n{finalCoins} coins";

            foreach (var item in items)
                if (item.data)
                    text += $"\n{item.data.name}";

            return text;
        }

        public int GetTotalExperience()
        {
            string playerType = Game.instance.currentCharacter.currentDynamicPlayerType;

            float multiplier = 1.0f;

            switch (playerType)
            {
                case "Achiever":
                    multiplier = achieverExpMultiplier;
                    break;
                case "Killer":
                    multiplier = killerExpMultiplier;
                    break;
                case "Socializer":
                    multiplier = socializerExpMultiplier;
                    break;
                case "Explorer":
                    multiplier = explorerExpMultiplier;
                    break;
            }

            return Mathf.CeilToInt(experience * multiplier);
        }

        public int GetTotalCoins()
        {
            string playerType = Game.instance.currentCharacter.currentDynamicPlayerType;

            float multiplier = 1.0f;

            switch (playerType)
            {
                case "Achiever":
                    multiplier = achieverGoldMultiplier;
                    break;
                case "Killer":
                    multiplier = killerGoldMultiplier;
                    break;
                case "Socializer":
                    multiplier = socializerGoldMultiplier;
                    break;
                case "Explorer":
                    multiplier = explorerGoldMultiplier;
                    break;
            }

            return Mathf.CeilToInt(coins * multiplier);
        }
        public string GetFormattedRewardText()
        {
            int baseExperience = experience;
            int finalExperience = GetTotalExperience();
            int baseCoins = coins;
            int finalCoins = GetTotalCoins();

            float expMultiplier = (float)finalExperience / baseExperience;
            float goldMultiplier = (float)finalCoins / baseCoins;

            string expColor = GetMultiplierColor(expMultiplier);
            string goldColor = GetMultiplierColor(goldMultiplier);

            string expChange = FormatMultiplierText(expMultiplier, expColor, baseExperience, finalExperience);
            string goldChange = FormatMultiplierText(goldMultiplier, goldColor, baseCoins, finalCoins);

            return $"{baseExperience} EXP {expChange}\n{baseCoins} Coins {goldChange}";
        }

        /// <summary>
        /// Formatuje tekst dla mnożnika nagrody i dodaje wartość bazową oraz końcową.
        /// </summary>
        private string FormatMultiplierText(float multiplier, string color, int baseValue, int finalValue)
        {
            if (multiplier > 1f)
                return $"(<color=#{color}>+{Mathf.RoundToInt((multiplier - 1) * 100)}%</color>)";
            if (multiplier < 1f)
                return $"(<color=#{color}>-{Mathf.RoundToInt((1 - multiplier) * 100)}%</color>)";

            return $"(<color=#{color}>-/-</color>)";
        }

        /// <summary>
        /// Zwraca kolor tekstu w zależności od mnożnika.
        /// </summary>
        private string GetMultiplierColor(float multiplier)
        {
            if (multiplier > 1f) return ColorUtility.ToHtmlStringRGB(GameColors.Green);
            if (multiplier < 1f) return ColorUtility.ToHtmlStringRGB(GameColors.LightRed);
            return ColorUtility.ToHtmlStringRGB(GameColors.LightBlue);
        }

        /// <summary>
        /// Formatuje tekst dla mnożnika nagrody.
        /// </summary>
        private string FormatMultiplierText(float multiplier, string color)
        {
            if (multiplier > 1f)
                return $"(<color=#{color}>+{Mathf.RoundToInt((multiplier - 1) * 100)}%</color>)";
            if (multiplier < 1f)
                return $"(<color=#{color}>-{Mathf.RoundToInt((1 - multiplier) * 100)}%</color>)";
            
            return $"(<color=#{color}>-/-</color>)";
        }

        public int GetTargetProgress()
        {
            if (Game.instance.currentCharacter.currentDynamicPlayerType == "Killer")
            {
                return Mathf.CeilToInt(targetProgress * killerMultiplier);
            }

            return targetProgress;
        }
    }
}
