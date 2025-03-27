using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [CreateAssetMenu(fileName = "New Jewel", menuName = "PLAYER TWO/ARPG Project/Item/Jewel")]
    public class ItemJewel : Item, ItemQuest
    {
        [Tooltip("The success rate when used for crafting.")]
        public float successRate;
    }
}
