using UnityEngine;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [CreateAssetMenu(fileName = "New Game Data", menuName = "PLAYER TWO/ARPG Project/Game/Game Data")]
    [ExecuteAlways]
    public class GameData : ScriptableObject
    {
        [Tooltip("The list of all available Characters.")]
        public List<Character> characters;

        [Tooltip("The list of all available Items.")]
        public List<Item> items;

        [Tooltip("The list of all available Skills.")]
        public List<Skill> skills;

        [Tooltip("The list of all available< Quests.")]
        public List<Quest> quests;

        [Tooltip("The list of all available Quest Item Prefabs.")]
        public List<GameObject> questItems;

        /// <summary>
        /// Przypisz unikalne identyfikatory do każdego przedmiotu w liście.
        /// </summary>
        public void AssignItemIDs()
        {
            Dictionary<Item.ItemGroup, int> groupCounters = new Dictionary<Item.ItemGroup, int>();

            foreach (var item in items)
            {
                if (item == null) continue;

                var group = item.group;
                int groupValue = (int)group;

                if (!groupCounters.ContainsKey(group))
                    groupCounters[group] = 0;

                int index = groupCounters[group];

                string rawID = groupValue.ToString() + index.ToString();
                item.id = int.Parse(rawID);

                groupCounters[group]++;
            }

            Debug.Log("[GameData] ID nadane jako: G + index (np. 312 = Staffs, #12).");
        }

        private void OnValidate()
        {
            AssignItemIDs(); // Przypisz ID automatycznie podczas każdej zmiany
        }
    }
}
