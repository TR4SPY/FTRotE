using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [CreateAssetMenu(fileName = "New Saving Offer", menuName = "PLAYER TWO/ARPG Project/Banking/Saving Offer")]
    public class SavingOffer : ScriptableObject
    {
        public string offerName;
        public float durationHours;
        public float interestRate;
    }
}
