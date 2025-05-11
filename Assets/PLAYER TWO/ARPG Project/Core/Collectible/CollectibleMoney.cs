using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Collectible/Collectible Money")]
    public class CollectibleMoney : Collectible
    {
        [Header("Multiple Currency Amounts in ONE Prefab")]
        [Tooltip("How many Solmire coins are in this drop? (1 Solmire = 1000 Lunaris = 100,000 Amberlings)")]
        public int solmire;

        [Tooltip("How many Lunaris coins are in this drop? (1 Lunaris = 100 Amberlings)")]
        public int lunaris;

        [Tooltip("How many Amberlings coins are in this drop?")]
        public int amberlings;

        [Header("Child objects for each currency (optional)")]
        [Tooltip("FBX or mesh for Solmire coins (will be toggled off if solmire=0)")]
        public GameObject objSolmire;

        [Tooltip("FBX or mesh for Lunaris coins (will be toggled off if lunaris=0)")]
        public GameObject objLunaris;

        [Tooltip("FBX or mesh for Amberlings coins (will be toggled off if amberlings=0)")]
        public GameObject objAmberlings;

        [Header("Icons for coins UI")]
        public Sprite iconSolmire;
        public Sprite iconLunaris;
        public Sprite iconAmberlings;

        [Header("Audio Settings")]
        [Tooltip("SFX played once when this drop spawns/enables in the world.")]
        public AudioClip dropClip;

        [Tooltip("SFX played when collecting the money.")]
        public AudioClip collectClip;

        protected GameAudio m_audio => GameAudio.instance;

        /// <summary>
        /// Returns a combined name, for example: "S:2 L:35 A:12"
        /// </summary>
        public override string GetName()
        {
            return $"Sol: {solmire}, Lun: {lunaris}, Amb: {amberlings}";
        }

        /// <summary>
        /// Gathers coins altogether, Solmire, Lunaris and Amberlings, calculate everything to Amberlings and adds to the inventory - so it makes sense... somehow.
        /// </summary>
        protected override bool TryCollect(Inventory inventory)
        {
            /*
                First a number of Amberlings (in Solmire and Lunaris):
                1 Solmire = 1000 Lunaris = 100,000 Amberlings
                1 Lunaris = 100 Amberlings
            */

            int totalAmberlings = (solmire * 100000) + (lunaris * 100) + amberlings;
            inventory.currency.AddAmberlings(totalAmberlings);

            inventory.onMoneyChanged?.Invoke();

            if (m_audio) m_audio.PlayEffect(collectClip);

            return true;
        }

        /// <summary>
        /// Sound for the object appearence in the scene: "dropClip".
        /// </summary>
        protected virtual void OnEnable()
        {
            if (m_audio) m_audio.PlayEffect(dropClip);
        }

        /// <summary>
        /// Base start function with added turning on/off a child objects for currency.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (objSolmire)  objSolmire.SetActive(solmire > 0);
            if (objLunaris)  objLunaris.SetActive(lunaris > 0);
            if (objAmberlings) objAmberlings.SetActive(amberlings > 0);
        }
    }
}
