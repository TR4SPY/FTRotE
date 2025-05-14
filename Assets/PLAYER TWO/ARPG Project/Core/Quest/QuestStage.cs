using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PLAYERTWO.ARPGProject
{
    [System.Serializable]
    public class QuestStage
    {
        public string description;
        public Quest.CompletingMode completingMode;
        public int targetProgress = 1;
        public string progressKey;
        public string destinationScene;
        public QuestItemReward requiredItem;
        public string returnToNPC;

        public bool requiresManualCompletion = false;
    }
}