using Unity.VisualScripting;
using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Equipments")]
    public class GUIEquipments : MonoBehaviour
    {
        [Header("Equipment Slots")]
        [Tooltip("Reference to the GUI Equipment Slot that corresponds to the right hand slot.")]
        public GUIEquipmentSlot rightHandSlot;

        [Tooltip("Reference to the GUI Equipment Slot that corresponds to the left hand slot.")]
        public GUIEquipmentSlot leftHandSlot;

        [Tooltip("Reference to the GUI Equipment Slot that corresponds to the helm slot.")]
        public GUIEquipmentSlot helmSlot;

        [Tooltip("Reference to the GUI Equipment Slot that corresponds to the chest slot.")]
        public GUIEquipmentSlot chestSlot;

        [Tooltip("Reference to the GUI Equipment Slot that corresponds to the pants slot.")]
        public GUIEquipmentSlot pantsSlot;

        [Tooltip("Reference to the GUI Equipment Slot that corresponds to the gloves slot.")]
        public GUIEquipmentSlot glovesSlot;

        [Tooltip("Reference to the GUI Equipment Slot that corresponds to the boots slot.")]
        public GUIEquipmentSlot bootsSlots;

        [Tooltip("Reference to the GUI Equipment Slot that corresponds to the wings slot.")]
        public GUIEquipmentSlot wingsSlot;

        [Tooltip("Reference to the GUI Equipment Slot that corresponds to the left ring slot.")]
        public GUIEquipmentSlot leftRingSlot;

        [Tooltip("Reference to the GUI Equipment Slot that corresponds to the right ring slot.")]
        public GUIEquipmentSlot rightRingSlot;

        [Tooltip("Reference to the GUI Equipment Slot that corresponds to the necklace slot.")]
        public GUIEquipmentSlot necklaceSlot;

        [Tooltip("Reference to the GUI Equipment Slot that corresponds to the mount slot.")]
        public GUIEquipmentSlot mountSlot;

        [Tooltip("Reference to the GUI Equipment Slot that corresponds to the pet slot.")]
        public GUIEquipmentSlot petSlot;

        [Tooltip("Reference to the GUI Equipment Slot that corresponds to the charm slot.")]
        public GUIEquipmentSlot charmSlot;

        protected EntityItemManager m_equipments;

        /// <summary>
        /// Returns the reference to the Entity's Item Manager.
        /// </summary>
        public EntityItemManager equipments
        {
            get
            {
                if (!m_equipments)
                    m_equipments = Level.instance.player.items;

                return m_equipments;
            }
        }

        protected virtual void InitializeEquipments()
        {
            Equip(equipments.GetRightHand(), rightHandSlot);
            Equip(equipments.GetLeftHand(), leftHandSlot);
            Equip(equipments.GetHelm(), helmSlot);
            Equip(equipments.GetChest(), chestSlot);
            Equip(equipments.GetPants(), pantsSlot);
            Equip(equipments.GetGloves(), glovesSlot);
            Equip(equipments.GetBoots(), bootsSlots);
            Equip(equipments.GetWings(), wingsSlot);
            Equip(equipments.GetLeftRing(), leftRingSlot);
            Equip(equipments.GetRightRing(), rightRingSlot);
            Equip(equipments.GetNecklace(), necklaceSlot);
            Equip(equipments.GetMount(), mountSlot);
            Equip(equipments.GetPet(), petSlot);
            Equip(equipments.GetCharm(), charmSlot);
        }

        /// <summary>
        /// Tries to auto equip a given GUI Item on the first free slot
        /// that corresponds to the item slot.
        /// </summary>
        /// <param name="item">The GUI Item you want to equip.</param>
        /// <returns>Returns true if the item was equipped.</returns>
        public virtual bool TryAutoEquip(GUIItem item)
        {
            if (TryEquip(item, rightHandSlot)) return true;
            if (TryEquip(item, leftHandSlot)) return true;
            if (TryEquip(item, helmSlot)) return true;
            if (TryEquip(item, chestSlot)) return true;
            if (TryEquip(item, pantsSlot)) return true;
            if (TryEquip(item, glovesSlot)) return true;
            if (TryEquip(item, bootsSlots)) return true;
            if (TryEquip(item, wingsSlot)) return true;
            if (TryEquip(item, leftRingSlot)) return true;
            if (TryEquip(item, rightRingSlot)) return true;
            if (TryEquip(item, necklaceSlot)) return true;
            if (TryEquip(item, mountSlot)) return true;
            if (TryEquip(item, petSlot)) return true;
            if (TryEquip(item, charmSlot)) return true;

            return false;
        }

        /// <summary>
        /// Tries to equip a given GUI Item on a given GUI Equipment Slot.
        /// </summary>
        /// <param name="item">The GUI Item you want to equip.</param>
        /// <param name="slot">The GUI Equipment Slot you want to equip the item on.</param>
        /// <returns>Returns true if the item was equipped.</returns>
        public virtual bool TryEquip(GUIItem item, GUIEquipmentSlot slot)
        {
            if (!slot || !slot.CanEquip(item)) return false;

            slot.Equip(item);
            return true;
        }

        /// <summary>
        /// Equips an GUI Item on a given GUI Equipment Slot.
        /// </summary>
        /// <param name="item">The GUI Item you want to equip.</param>
        /// <param name="equipment">The GUI Equipment Slot you want to equip the item on.</param>
        public virtual void Equip(ItemInstance item, GUIEquipmentSlot equipment)
        {
            if (item != null && equipment)
            {
                equipment.Equip(GUI.instance.CreateGUIItem(item));
            }
        }

        protected virtual void Start()
        {
            InitializeEquipments();
        }
    }
}
