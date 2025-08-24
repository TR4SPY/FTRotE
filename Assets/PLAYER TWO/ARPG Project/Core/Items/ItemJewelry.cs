using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [CreateAssetMenu(fileName = "New Jewelry", menuName = "PLAYER TWO/ARPG Project/Item/Jewelry")]
    public class ItemJewelry : ItemEquippable, ItemQuest
    {
        public enum JewelryType
        {
            Necklace,
            Ring
        }

        [Header("Jewelry Settings")]
        [Tooltip("Defines whether this item is a necklace or a ring.")]
        public JewelryType type;

        [Header("Transform Settings")]
        [Tooltip("Offset position applied when equipped on the left ring slot.")]
        public Vector3 leftRingPosition;
        [Tooltip("Offset rotation applied when equipped on the left ring slot.")]
        public Vector3 leftRingRotation;

        [Tooltip("Offset position applied when equipped on the right ring slot.")]
        public Vector3 rightRingPosition;
        [Tooltip("Offset rotation applied when equipped on the right ring slot.")]
        public Vector3 rightRingRotation;

        [Tooltip("Offset position applied when equipped on the necklace slot.")]
        public Vector3 necklacePosition;
        [Tooltip("Offset rotation applied when equipped on the necklace slot.")]
        public Vector3 necklaceRotation;

        public override ItemAttributes CreateDefaultAttributes()
        {
            return new JewelryAttributes();
        }

        public override GameObject Instantiate(Transform slot)
        {
            var instance = base.Instantiate(slot);

            if (slot != null)
            {
                var lower = slot.name.ToLowerInvariant();
                if (lower.Contains("left"))
                {
                    instance.transform.localPosition += leftRingPosition;
                    instance.transform.localRotation *= Quaternion.Euler(leftRingRotation);
                }
                else if (lower.Contains("right"))
                {
                    instance.transform.localPosition += rightRingPosition;
                    instance.transform.localRotation *= Quaternion.Euler(rightRingRotation);
                }
                else
                {
                    instance.transform.localPosition += necklacePosition;
                    instance.transform.localRotation *= Quaternion.Euler(necklaceRotation);
                }
            }

            return instance;
        }
    }
}
