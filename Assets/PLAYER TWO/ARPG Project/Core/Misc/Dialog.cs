using System;
using UnityEngine;
using System.Collections.Generic;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    [CreateAssetMenu(fileName = "NewDialog", menuName = "Game/Dialog")]
    public class Dialog : ScriptableObject
    {
        [System.Serializable]
        public class DialogPage
        {
            [TextArea(5, 10)]
            public string dialogText;
            public string specialCondition = "";

            public List<DialogOption> options = new List<DialogOption>();
            public bool showOnce = false;
            public bool isSpecial = false;
        }

        [System.Serializable]
        public class DialogOption
        {
            public string optionText;
            public bool isForSocializer;
            public bool isForAchiever;
            public bool isForKiller;
            public bool isForExplorer;
            public DialogAction action;
            public int nextPageIndex = -1;
            public Affinity specialConditionToSet = Affinity.None;
        }

        public enum DialogAction
        {
            None = 0, 
            OpenQuests = 1,
            OpenShop = 2,
            OpenCrafting = 3,
            OpenBlacksmith = 4,
            ContinueDialog = 5,
            Exclusive = 6,
            OpenClassUpgrade = 7,
            SetSpecialCondition = 8,
            OpenGuildmaster = 9
        }

        public List<DialogPage> pages = new List<DialogPage>();
        public string dialogTitle;
        private Dictionary<int, int> selectedPaths = new Dictionary<int, int>();
        private int lastPathChoice = -1;
        
        public bool ShouldShowPage(string npcID, int pageIndex)
        {
            var character = Game.instance.currentCharacter;
            if (character == null) return false;

            var currentPage = pages[pageIndex];

            Debug.Log($"[ShouldShowPage] NPC_ID={npcID}, pageIndex={pageIndex}, showOnce={currentPage.showOnce}, " +
                    $"alreadyViewed={character.HasViewedDialogPage(npcID, pageIndex)}");

            if (currentPage.isSpecial)
            {
                if (string.IsNullOrEmpty(currentPage.specialCondition)) return false;
                
                if (!Enum.TryParse<Affinity>(currentPage.specialCondition, out Affinity pageCondition) || character.specialCondition != pageCondition)
                {
                    return false;
                }
            }

            return !currentPage.showOnce || !character.HasViewedDialogPage(npcID, pageIndex);
        }

        public void SetPathChoice(int fromPage, int toPage)
        {
            selectedPaths[fromPage] = toPage;
            lastPathChoice = toPage;
        }

        public int GetNextPageFromPath(int currentPage)
        {
            return selectedPaths.ContainsKey(currentPage) ? selectedPaths[currentPage] : -1;
        }

        public int GetLastPathChoice()
        {
            Debug.Log($"[Dialog] lastPathChoice przed resetem: {lastPathChoice}");

            return lastPathChoice;
        }

        public void ResetPathChoices()
        {
            selectedPaths.Clear();
            lastPathChoice = -1;
        }

        public List<DialogOption> GetFilteredOptions(int pageIndex, string playerType, QuestGiver questGiver)
        {
            List<DialogOption> filteredOptions = new List<DialogOption>();

            if (pageIndex >= pages.Count) return filteredOptions;

            foreach (var option in pages[pageIndex].options)
            {
                bool isAllowed = true;

                if (option.isForSocializer && playerType != "Socializer") isAllowed = false;
                if (option.isForAchiever && playerType != "Achiever") isAllowed = false;
                if (option.isForExplorer && playerType != "Explorer") isAllowed = false;
                if (option.isForKiller && playerType != "Killer") isAllowed = false;

                if (option.action == DialogAction.OpenClassUpgrade)
                {
                    if (questGiver == null || questGiver.CurrentClassUpgradeQuest() == null)
                    {
                        Debug.LogWarning("[Dialog] Nie znaleziono QuestGivera.");
                        isAllowed = false;
                    }
                    else
                    {
                        var upgradeQuest = questGiver.CurrentClassUpgradeQuest();
                        if (upgradeQuest == null)
                        {
                            Debug.Log("[Dialog] Brak dostępnego Class Upgrade Questa.");
                        }
                        else
                        {
                            Debug.Log($"[Dialog] Znaleziono Class Upgrade Quest: {upgradeQuest.title}");
                        }

                        isAllowed = upgradeQuest != null;
                    }
                }

                if (isAllowed)
                {
                    filteredOptions.Add(option);
                    if (filteredOptions.Count >= 4) break;
                }

            }
            return filteredOptions;
        }
    }
}