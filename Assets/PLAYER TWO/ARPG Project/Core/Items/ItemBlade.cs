using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [CreateAssetMenu(fileName = "New Weapon", menuName = "PLAYER TWO/ARPG Project/Item/Blade")]
    public class ItemBlade : ItemWeapon, ItemQuest
    {
        public enum Type { OneHand, TwoHand }

        [Header("Blade Settings")]
        [Tooltip("The handing type of the Blade.")]
        public Type type;

        [Header("Right Hand Settings")]
        [Tooltip("The offset position in local space applied to the prefab on the Entity's right hand.")]
        public Vector3 rightHandPosition;

        [Tooltip("The offset rotation in local space applied to the prefab on the Entity's right hand.")]
        public Vector3 rightHandRotation;

        [Header("Left Hand Settings")]
        [Tooltip("The offset position in local space applied to the prefab on the Entity's left hand.")]
        public Vector3 leftHandPosition;

        [Tooltip("The offset rotation in local space applied to the prefab on the Entity's left hand.")]
        public Vector3 leftHandRotation;

        [Header("Quest Settings")]
        [Tooltip("Is this blade a quest item?")]
        public bool isQuestSpecific = false;

        /// <summary>
        /// Returns true if this Blade is handled by one hand.
        /// </summary>
        public virtual bool IsOneHanded() => type == Type.OneHand;

        /// <summary>
        /// Returns true if this Blade is handled by two hands.
        /// </summary>
        public virtual bool IsTwoHanded() => type == Type.TwoHand;


        /// <summary>
        /// Returns true if this item is quest related.
        /// </summary>
        public bool IsQuestSpecific => isQuestSpecific;

        /// <summary>
        /// Instantiates the Item's prefab, applying the Weapons's right hand offsets, as a child of a given Transform.
        /// </summary>
        /// <param name="slot">The transform to assign the prefab to.</param>
        /// <returns>Returns the instance of the newly instantiated prefab.</returns>
        public virtual GameObject InstantiateRightHand(Transform slot)
        {
            var instance = Instantiate(prefab, slot);
            instance.transform.localPosition += rightHandPosition;
            instance.transform.localRotation *= Quaternion.Euler(rightHandRotation);
            return instance;
        }

        /// <summary>
        /// Instantiates the Item's prefab, applying the Weapons's left hand offsets, as a child of a given Transform.
        /// </summary>
        /// <param name="slot">The transform to assign the prefab to.</param>
        /// <returns>Returns the instance of the newly instantiated prefab.</returns>
        public virtual GameObject InstantiateLeftHand(Transform slot)
        {
            var instance = Instantiate(prefab, slot);
            instance.transform.localPosition += leftHandPosition;
            instance.transform.localRotation *= Quaternion.Euler(leftHandRotation);
            return instance;
        }
    }
}
