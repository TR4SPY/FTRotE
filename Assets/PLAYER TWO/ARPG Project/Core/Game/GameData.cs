using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
            Dictionary<Item.ItemGroup, int> groupCounters = new();

            foreach (var item in items.Where(i => i != null))
            {
                var group = item.group;
                int groupValue = (int)group;

                if (!groupCounters.ContainsKey(group))
                    groupCounters[group] = 0;

                int index = groupCounters[group];
                string rawID = groupValue.ToString() + index.ToString();
                int expectedID = int.Parse(rawID);

                bool isInCorrectGroup = item.id / 100 == groupValue;

                if (item.id == 0)
                {
                    item.id = expectedID;
                }
                else
                {
                    int idGroup = int.Parse(item.id.ToString()[0].ToString());
                    if (idGroup != groupValue)
                    {
                        item.id = expectedID;
                    }
                }

                groupCounters[group]++;
            }
        }

        private void OnValidate()
        {
            // AssignItemIDs();
        }
    }
}
