using System;
using System.Collections;
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
        public Text solmireText;
        public Text lunarisText;
        public Text amberlingsText;
        public Text timeText;
        public Button withdrawButton;

        private Coroutine m_updateRoutine;
        private BankManager.InvestmentAccount m_account;

        public void Set(BankManager.InvestmentAccount account, UnityAction onWithdraw)
        {
            m_account = account;
            if (nameText) nameText.text = account.name;
            if (offerText && account.offer)
                offerText.text = account.offer.offerName;
            if (valueText && account.offer)
                valueText.text = $"{account.offer.interestRate * 100:0}%";
            if (withdrawButton)
            {
                withdrawButton.onClick.RemoveAllListeners();
                withdrawButton.onClick.AddListener(onWithdraw);
            }

            UpdateTime(account);
            if (gameObject.activeInHierarchy)
            {
                if (m_updateRoutine != null) StopCoroutine(m_updateRoutine);
                m_updateRoutine = StartCoroutine(UpdateTimeRoutine(m_account));
            }
        }

        public void UpdateTime(BankManager.InvestmentAccount account)
        {
            Currency.SplitCurrency(account.CurrentValue, out int solmire, out int lunaris, out int amberlings);
            if (solmireText) solmireText.text = solmire.ToString();
            if (lunarisText) lunarisText.text = lunaris.ToString();
            if (amberlingsText) amberlingsText.text = amberlings.ToString();

            if (timeText) timeText.text = account.Matured ? "Ready!" : FormatRemainingTime(account.RemainingTime);
            if (withdrawButton) withdrawButton.interactable = account.Matured;
        }

        private IEnumerator UpdateTimeRoutine(BankManager.InvestmentAccount account)
        {
            while (gameObject.activeInHierarchy)
            {
                UpdateTime(account);
                yield return new WaitForSeconds(1f);
            }
        }

        private static string FormatRemainingTime(float seconds)
        {
            var span = TimeSpan.FromSeconds(Mathf.Max(0f, seconds));
            int days = span.Days;
            int hours = span.Hours;
            int minutes = span.Minutes;
            int secs = span.Seconds;

            string dayPart = days > 0 ? $"{days} {(days == 1 ? "Day" : "Days")}, " : string.Empty;
            return $"{dayPart}{hours:00} Hrs, {minutes:00} Min, {secs:00} Sec";
        }
        
        private void OnEnable()
        {
            if (m_account != null && m_updateRoutine == null)
            {
                m_updateRoutine = StartCoroutine(UpdateTimeRoutine(m_account));
            }
        }

        private void OnDisable()
        {
            if (m_updateRoutine != null)
            {
                StopCoroutine(m_updateRoutine);
                m_updateRoutine = null;
            }
        }
    }
}