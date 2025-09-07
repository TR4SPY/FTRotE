using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [System.Serializable]
    public class BankAccountSerializer
    {
        public int money;

        // Investment account fields
        public string name;
        public int offerIndex;
        public int principal;
        public long startUtcTicks;

        public BankAccountSerializer() { }

        public BankAccountSerializer(BankAccount account)
        {
            money = account.money;
        }

        public BankAccountSerializer(BankManager.InvestmentAccount account, int offerIndex)
        {
            name = account.name;
            this.offerIndex = offerIndex;
            principal = account.principal;
            startUtcTicks = account.startUtc.Ticks;
        }

        public virtual string ToJson() => JsonUtility.ToJson(this);

        public static BankAccountSerializer FromJson(string json) =>
            JsonUtility.FromJson<BankAccountSerializer>(json);
    }
}
