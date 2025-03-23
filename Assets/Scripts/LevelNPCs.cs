using UnityEngine;
using System.Collections.Generic;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    public class LevelNPCs : MonoBehaviour
    {
        public static LevelNPCs Instance { get; private set; }

        [Tooltip("Ręcznie przypisz QuestGiverów obecnych w tej scenie.")]
        public List<QuestGiver> questGivers = new List<QuestGiver>();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        /// <summary>
        /// Odświeża questy u wszystkich QuestGiverów.
        /// </summary>
        public void RefreshQuestGivers()
        {
            foreach (var giver in questGivers)
            {
                if (giver != null)
                    giver.InitializeStates(); 
            }
        }
    }
}