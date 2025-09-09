using UnityEngine;
using UnityEngine.EventSystems;
using System.Reflection;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Equipment Slot")]
    public class GUIEquipmentSlot : GUIItemSlot
    {
        [Header("Equipment Settings")]
        public ItemSlots slot;

        protected Entity m_entity             => Level.instance.player;
        protected GUIWindowsManager m_windows => GUIWindowsManager.instance;
        protected GUIInventory m_inventory    => m_windows.GetInventory();
        protected GUIBlacksmith m_blacksmith  => m_windows.blacksmith;

        private RectTransform   m_rect;
        private GUIItemRotation m_cachedRot;
        private static readonly FieldInfo s_itemRotationField =
            typeof(GUIItem).GetField("itemRotation",
                BindingFlags.NonPublic | BindingFlags.Instance);

        protected override void Awake()
        {
            base.Awake();
            m_rect = transform as RectTransform;
        }

        public override void Equip(GUIItem guiItem)
        {
            if (!guiItem) return;

            if (!m_entity.items.TryEquip(guiItem.item, slot))
            {
                GameAudio.instance.PlayDeniedSound();
                return;
            }

            if (base.item)
            {
                if (!m_inventory.TryAutoInsert(base.item))
                {
                    m_entity.items.TryEquip(base.item.item, slot);
                    GameAudio.instance.PlayDeniedSound();
                    return;
                }

                base.Unequip();
            }

            base.Equip(guiItem);

            guiItem.SetPreviewActive(true);
            m_cachedRot = null;
        }

        public override void Unequip()
        {
            if (!item) return;

            m_entity.items.RemoveItem(slot);
            base.Unequip();

            if (m_tempItem)
                m_tempItem.SetPreviewActive(true);           
        }

        protected override void HandleRightClick()
        {
            if (!item || !CanUnequip()) return;

            var rot = GetItemRotation();
            if (rot != null)
                rot.isHovered = false;
            GUIItemInspector.instance.Hide();

            if (m_blacksmith.isOpen && m_blacksmith.slot.CanEquip(item))
            {
                var guiItem = item;
                Unequip();
                m_blacksmith.slot.Equip(guiItem);
            }
            else if (m_inventory.TryAutoInsert(item))
            {
                Unequip();
            }
        }

        protected override void Update()
        {
            base.Update();

            if (item == null) return;

            var rot = GetItemRotation();
            if (rot == null) return; 

            Vector2 mousePos;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if (Mouse.current == null) return;
            mousePos = Mouse.current.position.ReadValue();
#else
            mousePos = Input.mousePosition;
#endif
            bool over = RectTransformUtility.RectangleContainsScreenPoint(
                            m_rect, mousePos, null); 

            rot.isHovered = over && !rot.isDragging; 
        }

        public override bool CanEquip(GUIItem guiItem)
        {
            if (!guiItem || (base.item && (!CanUnequip() || !m_inventory.CanAutoInsert(base.item))))
                return false;

            return m_entity.items.CanEquip(guiItem.item, slot);
        }

        public override bool CanUnequip()
        {
            if (item && item.item.data is ItemMisc misc && misc.cannotUnequip)
                return false;
            if (slot != ItemSlots.RightHand) return true;
            return !m_entity.items.IsUsingWeaponLeft();
        }

        private GUIItemRotation GetItemRotation()
        {
            if (m_cachedRot != null) return m_cachedRot;

            m_cachedRot = item.GetComponent<GUIItemRotation>();
            if (m_cachedRot != null) return m_cachedRot;

            if (s_itemRotationField != null)
                m_cachedRot = s_itemRotationField.GetValue(item) as GUIItemRotation;

            return m_cachedRot;
        }
    }
}
