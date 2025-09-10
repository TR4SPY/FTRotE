using UnityEngine;
using UnityEngine.Events;

namespace AI_DDA.Assets.Scripts
{
    /// <summary>
    /// Utility component that listens to reputation changes and invokes events
    /// when access for a given faction is granted or revoked. It can be used to
    /// enable quest givers, vendors or trigger hostile takeover logic.
    /// </summary>
    [AddComponentMenu("PLAYER TWO/ARPG Project/Factions/Faction Reputation Gate")]
    public class FactionReputationGate : MonoBehaviour
    {
        public Faction faction = Faction.None;
        [Tooltip("Minimum reputation required to unlock." )]
        public int requiredReputation = 0;

        public UnityEvent onUnlocked;
        public UnityEvent onLocked;
        public UnityEvent onHostile;

        private void OnEnable()
        {
            ReputationManager.Instance?.onReputationChanged.AddListener(OnReputationChanged);
            UpdateState();
        }

        private void OnDisable()
        {
            ReputationManager.Instance?.onReputationChanged.RemoveListener(OnReputationChanged);
        }

        private void OnReputationChanged(Faction changed, int value)
        {
            if (changed != faction) return;
            UpdateState(value);
        }

        private void UpdateState()
        {
            UpdateState(ReputationManager.Instance?.Get(faction) ?? 0);
        }

        private void UpdateState(int value)
        {
            if (value <= (ReputationManager.Instance?.hostileThreshold ?? -50))
            {
                onHostile?.Invoke();
                onLocked?.Invoke();
            }
            else if (value >= requiredReputation)
            {
                onUnlocked?.Invoke();
            }
            else
            {
                onLocked?.Invoke();
            }
        }
    }
}
