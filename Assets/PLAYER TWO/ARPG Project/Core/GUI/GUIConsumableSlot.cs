using UnityEngine;
using UnityEngine.EventSystems;
using System.Reflection;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Consumable Slot")]
    public class GUIConsumableSlot : GUIItemSlot
    {
        protected Entity m_entity => Level.instance.player;

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
            base.Equip(guiItem);
            guiItem.SetPreviewActive(true);
            m_cachedRot = null;
        }

        public override void Unequip()
        {
            base.Unequip();
            if (m_tempItem) m_tempItem.SetPreviewActive(true);
        }

        protected override void HandleRightClick()
        {
            if (!item) return;

#if UNITY_STANDALONE || UNITY_WEBGL
            if (m_entity.inventory.instance.TryAddOrStack(item.item))
            {
                Unequip();
                Destroy(m_tempItem.gameObject);
                m_tempItem = null;
            }
            else GameAudio.instance.PlayDeniedSound();
#else
            m_entity.items.ConsumeItem(item.item);
#endif
        }

        public override bool CanEquip(GUIItem g) => !base.item && g.item.IsConsumable();
        public override bool CanUnequip()        => true;

        public virtual void Clear()
        {
            if (item)
            {
                item.SetPreviewActive(false);
                Destroy(item.gameObject);
            }
            item        = null;
            m_tempItem  = null;
            m_cachedRot = null;
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
