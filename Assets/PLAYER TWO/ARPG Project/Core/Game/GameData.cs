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
            int currentID = 1;

            // Iteruj przez listę przedmiotów i przypisuj ID
            foreach (var item in items)
            {
                if (item != null && item.id == 0) // Jeśli przedmiot nie ma jeszcze ID
                {
                    item.id = currentID++;
                }
                else if (item != null) // Jeśli przedmiot już ma ID, upewnij się, że jest ono unikalne
                {
                    currentID = Mathf.Max(currentID, item.id + 1);
                }
            }
            //Debug.Log("Item IDs have been automatically assigned.");
        }

        private void OnValidate()
        {
            AssignItemIDs(); // Przypisz ID automatycznie podczas każdej zmiany
        }
    }
}
