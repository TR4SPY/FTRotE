using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Bank Account")]
    public class GUIBankAccount : MonoBehaviour
    {
        public Text nameText;
        public Text offerText;
        public Text valueText;
        public Text timeText;
        public Button withdrawButton;

        public void Set(BankManager.InvestmentAccount account, UnityAction onWithdraw)
        {
            if (nameText) nameText.text = account.name;
            if (offerText && account.offer)
                offerText.text = account.offer.offerName;
            if (withdrawButton)
            {
                withdrawButton.onClick.RemoveAllListeners();
                withdrawButton.onClick.AddListener(onWithdraw);
            }
            UpdateTime(account);
        }

        public void UpdateTime(BankManager.InvestmentAccount account)
        {
            if (valueText) valueText.text = Currency.FormatCurrencyString(account.CurrentValue);
            if (timeText) timeText.text = account.Matured ? "Ready!" : $"{account.RemainingTime:0}s";
            if (withdrawButton) withdrawButton.interactable = account.Matured;
        }
    }
}
