using UnityEngine;
using System.Collections.Generic;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Misc/Zone Trigger")]
    public class ZoneTrigger : MonoBehaviour
    {
        public string zoneStatus = "New Area Discovered";

        [Tooltip("The name of the zone.")]
        public string zoneName;

        [Tooltip("The description of the zone.")]
        public string zoneDescription;

        private GUIZoneHUD guiZoneHUD;
        protected Collider m_collider;

        protected virtual void InitializeCollider()
        {
            m_collider = GetComponent<Collider>();
            m_collider.isTrigger = true;
        }

        private void Start()
        {
            // Inicjalizacja Collidera
            InitializeCollider();

#if UNITY_2023_1_OR_NEWER
            guiZoneHUD = Object.FindFirstObjectByType<GUIZoneHUD>();
#else
            guiZoneHUD = Object.FindObjectOfType<GUIZoneHUD>();
#endif

            if (guiZoneHUD == null)
            {
                Debug.LogError("GUIZoneHUD not found in the scene.");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(GameTags.Player)) return;

            var currentCharacter = Game.instance.currentCharacter;

            if (currentCharacter == null)
            {
                Debug.LogError("Current character is null. Cannot log zone discovery.");
                return;
            }

            if (string.IsNullOrEmpty(zoneName))
            {
                Debug.LogError("Zone name is null or empty. Cannot log zone discovery.");
                return;
            }

            if (currentCharacter.visitedZones == null)
            {
                Debug.LogError("Visited zones list is null. Initializing it.");
                currentCharacter.visitedZones = new HashSet<string>(); // Utworzenie nowego HashSet
            }

            // Sprawdź, czy strefa została już odwiedzona
            if (currentCharacter.visitedZones.Contains(zoneName))
            {
                Debug.Log($"Zone '{zoneName}' has already been visited.");
                return;
            }

            // Dodaj strefę do odwiedzonych
            currentCharacter.visitedZones.Add(zoneName);

            // Zapis stanu gry
            GameSave.instance.Save();

            // Logowanie odkrycia strefy
            PlayerBehaviorLogger.Instance?.LogAreaDiscovered(zoneName);

            // Wyświetlenie informacji o strefie w GUI
            guiZoneHUD?.ShowZone(zoneStatus, zoneName, zoneDescription);

            Debug.Log($"Zone '{zoneName}' discovered and saved for character '{currentCharacter.name}'.");
        }
    }
}
