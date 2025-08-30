using UnityEngine;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [System.Flags]
    public enum PlayerType
    {
        None       = 0,
        Achiever   = 1 << 0,
        Killer     = 1 << 1,
        Socializer = 1 << 2,
        Explorer   = 1 << 3
    }

    public enum QuestType
    {
        Normal = 0,
        Exclusive = 1,
        ClassUpgrade = 2,
        Specialization = 3
    }

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

        [Tooltip("Can this quest be completed from Quest Log level?")]
        public bool allowLogCompletion = false;

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

        [Header("Quest Type")]
        public QuestType questType = QuestType.Normal;
        public CharacterClassRestrictions requiredClass;
        public bool requireMaxLevel;

        [Header("Multi-Stage Quest")]
        public bool isMultiStage = false;
        public List<QuestStage> stages = new();


        [Header("Exclusive Settings")]
        public bool forKiller;
        public bool forAchiever;
        public bool forExplorer;
        public bool forSocializer;


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
        /// Determines whether this quest can be undertaken by a character of the
        /// given class and player type.
        /// </summary>
        /// <param name="cls">The character class of the player.</param>
        /// <param name="playerType">The dynamic player type (e.g. Killer, Explorer).</param>
        public bool IsAvailableFor(CharacterClassRestrictions cls, string playerType)
        {
            bool classAllowed = requiredClass == CharacterClassRestrictions.None ||
                                (requiredClass & cls) != 0;
            if (!classAllowed)
                return false;

            // If no exclusivity flags are set, quest is available for all types
            bool anyExclusive = forKiller || forAchiever || forExplorer || forSocializer;
            if (!anyExclusive)
                return true;

            return (forKiller && playerType == "Killer") ||
                   (forAchiever && playerType == "Achiever") ||
                   (forExplorer && playerType == "Explorer") ||
                   (forSocializer && playerType == "Socializer");
        }

        /// <summary>
        /// Returns the formatted reward text.
        /// </summary>
        public string GetRewardText()
        {
            if (!hasReward) return "None";

            var parts = new List<string>();

            int finalExperience = GetTotalExperience();
            if (finalExperience > 0)
                parts.Add($"{finalExperience} EXP");

            int totalCoins = GetTotalCoins();
            if (totalCoins > 0)
            {
                int solmire, lunaris, amberlings;
                Currency.SplitCurrency(totalCoins, out solmire, out lunaris, out amberlings);

                if (solmire > 0)
                    parts.Add($"{solmire} Solmires");

                if (lunaris > 0)
                    parts.Add($"{lunaris} Lunaris");

                if (amberlings > 0)
                    parts.Add($"{amberlings} Amberlings");
            }

            foreach (var item in items)
            {
                if (item.data != null)
                    parts.Add(item.data.name);
            }

            return string.Join("\n", parts);
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

            float expMultiplier = baseExperience > 0 ? (float)finalExperience / baseExperience : 1f;
            float goldMultiplier = baseCoins > 0 ? (float)finalCoins / baseCoins : 1f;

            Color expColor = GetMultiplierColor(expMultiplier);
            Color goldColor = GetMultiplierColor(goldMultiplier);

            string expChange = FormatMultiplierText(expMultiplier, expColor, baseExperience, finalExperience);
            string goldChange = FormatMultiplierText(goldMultiplier, goldColor, baseCoins, finalCoins);

            return $"{baseExperience} EXP {expChange}\n{baseCoins} Coins {goldChange}";
        }

        /// <summary>
        /// Formatuje tekst dla mnożnika nagrody i dodaje wartość bazową oraz końcową.
        /// </summary>
        private string FormatMultiplierText(float multiplier, Color color, int baseValue, int finalValue)
        {
            string text;

            if (multiplier > 1f)
                text = $"+{Mathf.RoundToInt((multiplier - 1) * 100)}%";
            else if (multiplier < 1f)
                text = $"-{Mathf.RoundToInt((1 - multiplier) * 100)}%";
            else
                text = "-/-";

            return $"({StringUtils.StringWithColor(text, color)})";
        }

        /// <summary>
        /// Zwraca kolor tekstu w zależności od mnożnika.
        /// </summary>
        private Color GetMultiplierColor(float multiplier)
        {
            if (multiplier > 1f) return GameColors.Green;
            if (multiplier < 1f) return GameColors.LightRed;
            return GameColors.LightBlue;
        }

        /// <summary>
        /// Formatuje tekst dla mnożnika nagrody.
        /// </summary>
        private string FormatMultiplierText(float multiplier, Color color)
        {
            string text;

            if (multiplier > 1f)
                text = $"+{Mathf.RoundToInt((multiplier - 1) * 100)}%";
            else if (multiplier < 1f)
                text = $"-{Mathf.RoundToInt((1 - multiplier) * 100)}%";
            else
                text = "-/-";

            return $"({StringUtils.StringWithColor(text, color)})";
        }

        public int GetTargetProgress()
        {
            if (Game.instance.currentCharacter.currentDynamicPlayerType == "Killer")
            {
                return Mathf.CeilToInt(targetProgress * killerMultiplier);
            }

            return targetProgress;
        }

        public int GetTargetProgressForPlayerType(string type)
        {
            switch (type)
            {
                case "Killer":
                    return Mathf.CeilToInt(targetProgress * killerMultiplier);
                case "Socializer":
                    return Mathf.CeilToInt(targetProgress * socializerMultiplier);
                case "Explorer":
                    return Mathf.CeilToInt(targetProgress * explorerMultiplier);
                case "Achiever":
                    return Mathf.CeilToInt(targetProgress * achieverMultiplier);
                default:
                
                return targetProgress;
            }
        }

        public int GetCoinsForPlayerType(string type)
        {
            switch (type)
            {
                case "Killer": return Mathf.CeilToInt(coins * killerGoldMultiplier);
                case "Socializer": return Mathf.CeilToInt(coins * socializerGoldMultiplier);
                case "Explorer": return Mathf.CeilToInt(coins * explorerGoldMultiplier);
                case "Achiever": return Mathf.CeilToInt(coins * achieverGoldMultiplier);
                default:
                
                return coins;
            }
        }

        public int GetExpForPlayerType(string type)
        {
            switch (type)
            {
                case "Killer": return Mathf.CeilToInt(experience * killerExpMultiplier);
                case "Socializer": return Mathf.CeilToInt(experience * socializerExpMultiplier);
                case "Explorer": return Mathf.CeilToInt(experience * explorerExpMultiplier);
                case "Achiever": return Mathf.CeilToInt(experience * achieverExpMultiplier);
                default:
                
                return experience;
            }
        }
    }
}
