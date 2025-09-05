using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AI_DDA.Assets.Scripts
{
    public enum ReputationStatus
    {
        Hostile,
        Neutral,
        Friendly
    }

    /// <summary>
    /// Manages reputation values for different factions.
    /// </summary>
    [AddComponentMenu("PLAYER TWO/ARPG Project/Reputation Manager")]
    public class ReputationManager : MonoBehaviour
    {
        public static ReputationManager Instance { get; private set; }

        [System.Serializable]
        public class FactionIntEvent : UnityEvent<Faction, int> { }

        [System.Serializable]
        public class FactionStatusEvent : UnityEvent<Faction, ReputationStatus> { }

        public int hostileThreshold = -50;
        public int friendlyThreshold = 50;

        public FactionIntEvent onReputationChanged;
        public FactionStatusEvent onStatusChanged;

        private readonly Dictionary<Faction, int> reputations = new();
        private readonly Dictionary<Faction, ReputationStatus> statusCache = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public int Get(Faction faction)
        {
            return reputations.TryGetValue(faction, out var value) ? value : 0;
        }

        public Dictionary<Faction, int> GetAllReputations()
        {
            return new Dictionary<Faction, int>(reputations);
        }

        public void Set(Faction faction, int value)
        {
            reputations[faction] = value;
            statusCache[faction] = GetStatus(value);
        }

        public void Adjust(Faction faction, int delta)
        {
            int newValue = Get(faction) + delta;
            reputations[faction] = newValue;
            onReputationChanged?.Invoke(faction, newValue);

            var newStatus = GetStatus(newValue);
            if (!statusCache.TryGetValue(faction, out var oldStatus) || oldStatus != newStatus)
            {
                statusCache[faction] = newStatus;
                onStatusChanged?.Invoke(faction, newStatus);
            }
        }

        public void Load(Dictionary<Faction, int> values)
        {
            reputations.Clear();
            statusCache.Clear();
            foreach (var kvp in values)
            {
                reputations[kvp.Key] = kvp.Value;
                statusCache[kvp.Key] = GetStatus(kvp.Value);
            }
        }

        private ReputationStatus GetStatus(int value)
        {
            if (value <= hostileThreshold)
                return ReputationStatus.Hostile;
            if (value >= friendlyThreshold)
                return ReputationStatus.Friendly;
            return ReputationStatus.Neutral;
        }
    }
}

