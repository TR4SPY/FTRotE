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

        [Tooltip("The list of all available Buffs and Debuffs.")]
        public List<Buff> buffs;

        [Tooltip("The list of all available< Quests.")]
        public List<Quest> quests;

        [Tooltip("The list of all available Quest Item Prefabs.")]
        public List<GameObject> questItems;

        [Tooltip("The list of all enemies in the game.")]
        public List<GameObject> enemies;


        /// <summary>
        /// Przypisz unikalne identyfikatory do każdego przedmiotu w liście.
        /// </summary>
        public void AssignItemIDs()
        {
            Dictionary<Item.ItemGroup, int> groupCounters = new();

            var itemsToFix = items
                .Where(i => i != null && i.id == 0)
                .OrderBy(i => (int)i.group)
                .ToList();

            foreach (var item in itemsToFix)
            {
                var group = item.group;
                int groupValue = (int)group;

                if (!groupCounters.ContainsKey(group))
                    groupCounters[group] = GetMaxIndexInGroup(group) + 1;

                int currentIndex = groupCounters[group];
                string rawID = groupValue.ToString() + currentIndex.ToString();
                int newID = int.Parse(rawID);

                item.id = newID;
                Debug.Log($"🆕 Ustawiono ID {newID} dla: {item.name}");

                groupCounters[group]++;
            }
        }
        
        private int GetMaxIndexInGroup(Item.ItemGroup group)
        {
            return items
                .Where(i => i != null && i.group == group && i.id != 0)
                .Select(i =>
                {
                    var idStr = i.id.ToString();
                    if (idStr.Length <= 1 || !int.TryParse(idStr.Substring(1), out var index))
                        return 0;

                    return index;
                })
                .DefaultIfEmpty(-1)
                .Max();
        }

        private void OnValidate()
        {
            // AssignItemIDs();
        }
    }
}
