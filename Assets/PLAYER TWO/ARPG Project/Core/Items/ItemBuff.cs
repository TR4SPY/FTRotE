using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [CreateAssetMenu(fileName = "New Buff Potion", menuName = "PLAYER TWO/ARPG Project/Item/Buff Potion")]
    public class ItemBuff : ItemPotion
    {
        public override void Consume(Entity entity)
        {
            base.Consume(entity);
            ApplyBuffs(entity);
        }
    }
}