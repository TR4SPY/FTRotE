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
        [Tooltip("Chance of successful application in percentage (0-100).")]
        [Range(0, 100)]
        public int successRate = 100;

        [Tooltip("Optional description of the item (e.g. for jewels/crystals).")]
        [TextArea(2, 4)]
        public string description;
    }
}
