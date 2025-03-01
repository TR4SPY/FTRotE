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

        [Header("Extra Reward Settings")]
        [Tooltip("If checked, additional rewards will be applied for Achiever-type players.")]
        public bool extraRewards;

        [Tooltip("Extra experience points for Achiever-type players.")]
        public int additionalExperience;

        [Tooltip("Extra coins for Achiever-type players.")]
        public int additionalCoins;

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
            int finalExperience = experience;
            int finalCoins = coins;

            if (extraRewards && Game.instance.currentCharacter.currentDynamicPlayerType == "Achiever")
            {
                finalExperience += additionalExperience;
                finalCoins += additionalCoins;
            }

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
            if (extraRewards && Game.instance.currentCharacter.currentDynamicPlayerType == "Achiever")
                return experience + additionalExperience;

            return experience;
        }

        public int GetTotalCoins()
        {
            if (extraRewards && Game.instance.currentCharacter.currentDynamicPlayerType == "Achiever")
                return coins + additionalCoins;

            return coins;
        }
    }
}
