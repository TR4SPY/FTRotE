using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Misc/Fountain")]
    public class Fountain : Interactive
    {
        [Header("Fountain Settings")]
        [Tooltip("If true, restores the Entity health points on interacting.")]
        public bool resetHealth;

        [Tooltip("If true, restores the Entity mana points on interacting.")]
        public bool resetMana;

        [Tooltip("The Game Object that represents the content of the fountain.")]
        public GameObject content;

        protected bool m_canUse = true;
        public bool CanUse => m_canUse;

        protected override void OnInteract(object other)
        {
            if (!(other is Entity entity) || !m_canUse) return;

            if (resetHealth)
            {
                entity.stats.ResetHealth();
                Debug.Log($"Health reset by Fountain for: {entity.name}");
            }

            if (resetMana)
            {
                entity.stats.ResetMana();
                Debug.Log($"Mana reset by Fountain for: {entity.name}");
            }

            m_canUse = false;
            content.SetActive(false);

            Debug.Log($"Fountain used by: {entity.name}, isPlayer: {entity.isPlayer}");
        }
    }
}
