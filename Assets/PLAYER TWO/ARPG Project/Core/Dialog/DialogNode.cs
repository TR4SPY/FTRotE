using System.Collections.Generic;
using UnityEngine;

namespace AI_DDA.Assets.Scripts
{
    [System.Serializable]
    public class DialogNode
    {
        [TextArea(5,10)]
        public string dialogText;
        public string specialCondition = "";
        public List<DialogOption> options = new List<DialogOption>();
        public bool showOnce = false;
        public bool isSpecial = false;
    }
}
