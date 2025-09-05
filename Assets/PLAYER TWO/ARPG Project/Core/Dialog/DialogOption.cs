using UnityEngine;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    [System.Serializable]
    public class DialogOption
    {
        public string optionText;
        public bool isForSocializer;
        public bool isForAchiever;
        public bool isForKiller;
        public bool isForExplorer;
        public Dialog.DialogAction action;
        public int nextPageIndex = -1;
        public Affinity specialConditionToSet = Affinity.None;
        public bool hideIfAlreadyInGuild;
        public Buff buff;
    }
}
