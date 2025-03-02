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
            public List<DialogOption> options = new List<DialogOption>();
            public bool showOnce = false;
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
        }

        public enum DialogAction
        {
            None, 
            OpenQuests,
            OpenShop,
            OpenBlacksmith,
            ContinueDialog,
            Exclusive
        }

        public List<DialogPage> pages = new List<DialogPage>();
        public string dialogTitle;
        private HashSet<int> viewedPages = new HashSet<int>();
        private Dictionary<int, int> selectedPaths = new Dictionary<int, int>();
        private int lastPathChoice = -1;
        
        public bool ShouldShowPage(int pageIndex)
        {
            var character = Game.instance.currentCharacter;
            return character != null && !character.viewedDialogPages.Contains(pageIndex);
        }

        public void MarkPageAsViewed(int pageIndex)
        {
            var character = Game.instance.currentCharacter;
            if (character != null)
            {
                character.viewedDialogPages.Add(pageIndex);
            }
        }

        public void SetPathChoice(int fromPage, int toPage)
        {
            var character = Game.instance.currentCharacter;
            if (character != null)
            {
                character.selectedDialogPaths[fromPage] = toPage;
            }
        }

        public int GetNextPageFromPath(int currentPage)
        {
            var character = Game.instance.currentCharacter;
            return character != null && character.selectedDialogPaths.ContainsKey(currentPage)
                ? character.selectedDialogPaths[currentPage]
                : -1;
        }

        public int GetLastPathChoice()
        {
            return lastPathChoice;
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