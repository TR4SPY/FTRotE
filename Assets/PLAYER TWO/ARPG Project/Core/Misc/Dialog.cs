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
            public SpecialCondition specialConditionToSet = SpecialCondition.None;
        }

        public enum DialogAction
        {
            None, 
            OpenQuests,
            OpenShop,
            OpenBlacksmith,
            ContinueDialog,
            Exclusive,
            SetSpecialCondition
        }

        public List<DialogPage> pages = new List<DialogPage>();
        public string dialogTitle;
        private Dictionary<int, int> selectedPaths = new Dictionary<int, int>();
        private int lastPathChoice = -1;
        
        public bool ShouldShowPage(int pageIndex)
        {
            var character = Game.instance.currentCharacter;
            if (character == null) return false;

            var currentPage = pages[pageIndex];

            Debug.Log($"[ShouldShowPage] pageIndex={pageIndex}, showOnce={currentPage.showOnce}, " +
              $"special={currentPage.isSpecial}, cond={currentPage.specialCondition}, " +
              $"charCondition={character.specialCondition}, alreadyViewed={character.HasViewedDialogPage(pageIndex)}");

            if (currentPage.isSpecial)
            {
                if (string.IsNullOrEmpty(currentPage.specialCondition)) return false;
                
                if (!Enum.TryParse<SpecialCondition>(currentPage.specialCondition, out SpecialCondition pageCondition) || character.specialCondition != pageCondition)
                {
                    return false;
                }
            }

            return !currentPage.showOnce || !character.HasViewedDialogPage(pageIndex);
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

        public List<DialogOption> GetFilteredOptions(int pageIndex, string playerType)
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