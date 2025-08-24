using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [CreateAssetMenu(fileName = "New Glyph", menuName = "PLAYER TWO/ARPG Project/Item/Glyph")]
    public class ItemGlyph : ItemEquippable, ItemQuest
    {
        [Header("Glyph Settings")]
        [Tooltip("Buffs applied when this glyph is equipped.")]
        public Buff[] buffsOnEquip;

        [Tooltip("Offset position applied to the glyph instance.")]
        public Vector3 positionOffset;

        [Tooltip("Offset rotation applied to the glyph instance.")]
        public Vector3 rotationOffset;

        public override GameObject Instantiate(Transform slot)
        {
            var instance = base.Instantiate(slot);
            if (instance)
            {
                instance.transform.localPosition += positionOffset;
                instance.transform.localRotation *= Quaternion.Euler(rotationOffset);
            }
            return instance;
        }

        public override ItemAttributes CreateDefaultAttributes()
        {
            return new GlyphAttributes();
        }
    }
}
