using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Banking/Bank Manager")]
    public class BankManager : Singleton<BankManager>
    {
        [Serializable]
        public class InvestmentAccount
        {
            public string name;
            public SavingOffer offer;
            public int principal;
            [SerializeField] private long m_startUtcTicks;

            public DateTime startUtc
            {
                get => new DateTime(m_startUtcTicks, DateTimeKind.Utc);
                set => m_startUtcTicks = value.Ticks;
            }

            public bool Matured => DateTime.UtcNow >= startUtc.AddHours(offer.durationHours);

            public float RemainingTime => Mathf.Max(0f, (float)(startUtc.AddHours(offer.durationHours) - DateTime.UtcNow).TotalSeconds);

            public int CurrentValue
            {
                get
                {
                    var elapsed = DateTime.UtcNow - startUtc;
                    float progress = Mathf.Clamp01((float)elapsed.TotalHours / offer.durationHours);
                    float value = principal * (1f + offer.interestRate * progress);
                    return Mathf.RoundToInt(value);
                }
            }
        }

        [Header("Offers")]
        [Tooltip("Available savings offers.")]
        public List<SavingOffer> offers = new();

        [SerializeField]
        private List<InvestmentAccount> m_accounts = new();
        public IReadOnlyList<InvestmentAccount> accounts => m_accounts;

        private const int MaxAccounts = 5;
        public bool HasAvailableSlot => m_accounts.Count < MaxAccounts;

        public bool OpenAccount(string playerAccountName, int principal, SavingOffer offer)
        {
            if (!HasAvailableSlot || offer == null)
            {
                return false;
            }

            var account = new InvestmentAccount
            {
                name = playerAccountName,
                offer = offer,
                principal = principal,
                startUtc = DateTime.UtcNow
            };

            m_accounts.Add(account);
            return true;
        }

        public int Withdraw(int index)
        {
            if (index < 0 || index >= m_accounts.Count)
            {
                return 0;
            }

            var account = m_accounts[index];
            if (!account.Matured)
            {
                return 0;
            }

            int value = account.CurrentValue;
            m_accounts.RemoveAt(index);
            return value;
        }
    }
}

