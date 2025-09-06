using System;
using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    /// <summary>
    /// Represents a bank account that accumulates interest over time.
    /// </summary>
    [Serializable]
    public class SavingsAccount
    {
        public string name;
        public Currency currency = new();
        [Tooltip("Interest rate applied when claiming interest.")]
        public float interestRate = 0.05f;
        [Tooltip("Time in seconds between interest claims.")]
        public float interestIntervalSeconds = 60f;

        private ITimeProvider m_timeProvider;
        private DateTime m_startUtc;
        private DateTime m_lastClaimUtc;

        /// <summary>
        /// Initializes the account and captures the start time using the given time provider.
        /// </summary>
        public void Initialize(ITimeProvider provider = null)
        {
            m_timeProvider = provider ?? new SystemTimeProvider();
            m_startUtc = m_timeProvider.UtcNow;
            m_lastClaimUtc = m_startUtc;
        }

        /// <summary>
        /// Returns the elapsed time since <see cref="Initialize"/> was called.
        /// </summary>
        public TimeSpan Elapsed => m_timeProvider.UtcNow - m_startUtc;

        private TimeSpan ElapsedSinceLastClaim => m_timeProvider.UtcNow - m_lastClaimUtc;

        public bool CanClaim => ElapsedSinceLastClaim >= TimeSpan.FromSeconds(interestIntervalSeconds);

        public float RemainingTime => Mathf.Max(0f, (float)(TimeSpan.FromSeconds(interestIntervalSeconds) - ElapsedSinceLastClaim).TotalSeconds);

        public int ClaimInterest()
        {
            if (!CanClaim) return 0;
            int reward = Mathf.FloorToInt(currency.GetTotalAmberlings() * interestRate);
            currency.AddAmberlings(reward);
            m_lastClaimUtc = m_timeProvider.UtcNow;
            return reward;
        }
    }
}

