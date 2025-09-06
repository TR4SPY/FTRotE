using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Bank Offer")]
    public class GUIBankOffer : MonoBehaviour
    {
        public Text nameText;
        public Text rateText;
        public Text durationText;
        public Button selectButton;

        public void Set(SavingOffer offer, UnityAction onSelect)
        {
            if (nameText) nameText.text = offer.offerName;
            if (rateText) rateText.text = $"{offer.interestRate:P0}";
            if (durationText) durationText.text = $"{offer.durationHours:0}h";
            if (selectButton)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(onSelect);
            }
        }
    }
}
