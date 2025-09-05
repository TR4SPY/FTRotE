using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [System.Serializable]
    public class BankAccountSerializer
    {
        public int money;

        public BankAccountSerializer(BankAccount account)
        {
            money = account.money;
        }

        public virtual string ToJson() => JsonUtility.ToJson(this);

        public static BankAccountSerializer FromJson(string json) =>
            JsonUtility.FromJson<BankAccountSerializer>(json);
    }
}
