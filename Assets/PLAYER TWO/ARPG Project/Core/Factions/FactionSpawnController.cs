using UnityEngine;

namespace AI_DDA.Assets.Scripts
{
    /// <summary>
    /// Spawns different prefabs based on the player's standing with a faction.
    /// Scenes can use this to dynamically reflect faction control.
    /// </summary>
    [AddComponentMenu("PLAYER TWO/ARPG Project/Factions/Faction Spawn Controller")]
    public class FactionSpawnController : MonoBehaviour
    {
        public Faction faction = Faction.None;
        public GameObject neutralPrefab;
        public GameObject friendlyPrefab;
        public GameObject hostilePrefab;

        private GameObject m_current;

        private void Start()
        {
            if (ReputationManager.Instance != null)
            {
                ReputationManager.Instance.onStatusChanged.AddListener(OnStatusChanged);
                UpdateSpawn(ReputationManager.Instance.GetStatus(faction));
            }
        }

        private void OnDestroy()
        {
            if (ReputationManager.Instance != null)
                ReputationManager.Instance.onStatusChanged.RemoveListener(OnStatusChanged);
        }

        private void OnStatusChanged(Faction changed, ReputationStatus status)
        {
            if (changed != faction) return;
            UpdateSpawn(status);
        }

        private void UpdateSpawn(ReputationStatus status)
        {
            if (m_current != null)
                Destroy(m_current);

            GameObject prefab = status switch
            {
                ReputationStatus.Friendly => friendlyPrefab,
                ReputationStatus.Hostile => hostilePrefab,
                _ => neutralPrefab
            };

            if (prefab != null)
                m_current = Instantiate(prefab, transform.position, transform.rotation, transform);
        }
    }
}
