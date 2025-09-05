using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Game/Game Bank")]
    public class GameBank : Singleton<GameBank>
    {
        [Header("Settings")]
        [Tooltip("The amount of bank accounts available.")]
        public int amount = 5;

        /// <summary>
        /// Returns all the bank accounts.
        /// </summary>
        public BankAccount[] accounts { get; protected set; }

        protected virtual void InitializeAccounts()
        {
            if (accounts != null) return;

            accounts = new BankAccount[amount];

            for (int i = 0; i < amount; i++)
            {
                accounts[i] = new BankAccount();
            }
        }

        /// <summary>
        /// Gets a bank account by its index.
        /// </summary>
        public BankAccount GetAccount(int index)
        {
            if (index < 0 || index >= accounts.Length) return null;
            return accounts[index];
        }

        /// <summary>
        /// Loads the data of the bank from a given BankAccountSerializer array.
        /// </summary>
        public virtual void LoadData(BankAccountSerializer[] data)
        {
            if (data == null) return;

            accounts = new BankAccount[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                accounts[i] = new BankAccount
                {
                    money = data[i].money
                };
            }
        }

        protected override void Initialize()
        {
            InitializeAccounts();
        }
    }
}
