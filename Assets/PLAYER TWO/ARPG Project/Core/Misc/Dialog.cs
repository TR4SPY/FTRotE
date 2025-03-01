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

        /// <summary>
        /// Pobiera maksymalnie 4 opcje dla aktualnej strony, uwzględniając typ gracza.
        /// </summary>
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
