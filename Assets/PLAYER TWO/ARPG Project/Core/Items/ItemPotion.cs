using UnityEngine;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [CreateAssetMenu(fileName = "New Potion", menuName = "PLAYER TWO/ARPG Project/Item/Potion")]
    public class ItemPotion : ItemConsumable, ItemQuest
    {
        [Header("Healing Settings")]
        [Tooltip("The amount of health points this Potion recovers.")]
        public int healthAmount;

        [Tooltip("The amount of mana points this Potion recovers.")]
        public int manaAmount;

        public override void Consume(Entity entity)
        {
            base.Consume(entity);

            if (healthAmount > 0)
                entity.stats.health += healthAmount;

            if (manaAmount > 0)
                entity.stats.mana += manaAmount;

            // Log the potion usage in PlayerBehaviorLogger
            if (PlayerBehaviorLogger.Instance != null)
            {
                PlayerBehaviorLogger.Instance.potionsUsed++;
                Debug.Log("Potion consumed. Total potions used: " + PlayerBehaviorLogger.Instance.potionsUsed);
            }
            else
            {
                Debug.LogWarning("PlayerBehaviorLogger instance not found. Potion usage not logged.");
            }
        }
    }
}
