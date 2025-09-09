using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

        public static int GetMinimumDeposit(SavingOffer offer)
        {
            if (offer == null || offer.interestRate <= 0f) return 0;
            return Mathf.CeilToInt(1f / offer.interestRate);
        }

        public bool OpenAccount(string playerAccountName, int principal, SavingOffer offer)
        {
            if (!HasAvailableSlot || offer == null)
            {
                return false;
            }
            
            int minimum = GetMinimumDeposit(offer);
            if (principal < minimum)
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


        public BankAccountSerializer[] GetSerializedAccounts()
        {
            var result = new BankAccountSerializer[m_accounts.Count];

            for (int i = 0; i < m_accounts.Count; i++)
            {
                var account = m_accounts[i];
                int offerIndex = offers.IndexOf(account.offer);
                result[i] = new BankAccountSerializer(account, offerIndex);
            }

            return result;
        }

        public void LoadAccounts(BankAccountSerializer[] data)
        {
            m_accounts.Clear();

            if (data == null)
                return;

            foreach (var serialized in data)
            {
                if (serialized.offerIndex < 0 || serialized.offerIndex >= offers.Count)
                    continue;

                var account = new InvestmentAccount
                {
                    name = serialized.name,
                    offer = offers[serialized.offerIndex],
                    principal = serialized.principal,
                    startUtc = new DateTime(serialized.startUtcTicks, DateTimeKind.Utc)
                };

                m_accounts.Add(account);
            }
        }
    }
}

