using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using System.Linq;
using Kamgam.UGUIWorldImage;
using System.Collections;

namespace PLAYERTWO.ARPGProject
{
    static class PreviewPositionPool
    {
        private static readonly Queue<int> freeIndices = new Queue<int>();
        private static readonly HashSet<int> usedIndices = new HashSet<int>();

        private const int MaxSlots = 1024;

        static PreviewPositionPool()
        {
            for (int i = 0; i < MaxSlots; i++)
                freeIndices.Enqueue(i);
        }

        public static int GetNext()
        {
            if (freeIndices.Count == 0)
                return -1;
            int index = freeIndices.Dequeue();
            usedIndices.Add(index);
            return index;
        }

        public static void Release(int index)
        {
            if (usedIndices.Remove(index))
                freeIndices.Enqueue(index);
        }
    }

    [RequireComponent(typeof(RectTransform), typeof(Image), typeof(CanvasGroup))]
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Item")]
    public class GUIItem : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler
#if UNITY_ANDROID || UNITY_IOS
        ,IDragHandler,
        IEndDragHandler,
        IDropHandler,
        IDeselectHandler
#endif
    {
        [Tooltip("A reference to the Text component that represents the stack size.")]
        public Text stackText;
        
        private Vector2Int lastResolution;
        private Vector3 frozenModelLocalPos;
        private GUIItemRotation itemRotation;
        private GameObject renderAnchorObject;
        private int previewSlotIndex = -1;

        protected Image m_image;
        protected CanvasGroup m_group;
        protected GUIItemSlot m_lastSlot;
        protected GUIInventory m_lastInventory;

        protected bool m_hovering;
        protected bool m_selected;
        protected InventoryCell m_lastInventoryPosition;

        [SerializeField] private WorldImage worldImage;

        protected float m_lastClickTime;

        protected const float k_doubleClickThreshold = 0.3f;

        /// <summary>
        /// Returns the GUI Merchant associated to this GUI Item.
        /// </summary>
        public GUIMerchant merchant { get; set; }

        /// <summary>
        /// Returns the Item Instance that this GUI Item represents.
        /// </summary>
        public ItemInstance item { get; protected set; }

        /// <summary>
        /// Returns the Image component of this GUI Item.
        /// </summary>
        public Image image
        {
            get
            {
                if (!m_image)
                    m_image = GetComponent<Image>();

                return m_image;
            }
        }

        /// <summary>
        /// Returns the Canvas Group of this GUI Item.
        /// </summary>
        public CanvasGroup group
        {
            get
            {
                if (!m_group)
                    m_group = GetComponent<CanvasGroup>();

                return m_group;
            }
        }

        /// <summary>
        /// Returns true if this GUI Item is interactable.
        /// </summary>
        public bool interactable
        {
            get { return group.blocksRaycasts; }
            set { group.blocksRaycasts = value; }
        }

        /// <summary>
        /// Returns true if this item on a Merchant.
        /// </summary>
        public bool onMerchant => merchant;

        protected Entity player => Level.instance.player;

        /// <summary>
        /// Returns the current size of the GUI Item transform.
        /// </summary>
        public Vector2 size => ((RectTransform)transform).sizeDelta;

        protected GUIWindowsManager windowsManager => GUIWindowsManager.instance;
        protected GUIBlacksmith m_blacksmith => windowsManager.blacksmith;
        protected GUIWindow m_stash => windowsManager.stashWindow;
        protected GUIWindow m_merchant => windowsManager.merchantWindow;
        protected GUIWindow m_craftman => windowsManager.craftmanWindow;
        protected GUIInventory m_inventory => windowsManager.GetInventory();

        /// <summary>
        /// Selects this GUI Item.
        /// </summary>
        public virtual void Select()
        {
            group.blocksRaycasts = false;

            var layout = GetComponent<LayoutElement>();
            if (layout != null)
                layout.ignoreLayout = true;

            var rt = (RectTransform)transform;
            rt.SetAsLastSibling();
            rt.localScale = Vector3.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(item.columns, item.rows) * Inventory.CellSize;

            if (itemRotation != null)
                itemRotation.isDragging = true;
        }


        /// <summary>
        /// Deselects this GUI Item.
        /// </summary>
        public virtual void Deselect()
        {
            if (worldImage != null)
                worldImage.transform.SetParent(transform, false);

            group.blocksRaycasts = true;

            var layout = GetComponent<LayoutElement>();
            if (layout != null)
                layout.ignoreLayout = false;

            var slot = GetComponentInParent<GUISlot>();
            if (slot != null)
                transform.SetParent(slot.transform, false);

            StartCoroutine(FixCameraPositionAfterModelLoad());

            if (itemRotation != null)
                itemRotation.isDragging = false;
        }


        /// <summary>
        /// Returns true if its possible to stack a given item on this one.
        /// </summary>
        /// <param name="other">The item you want to stack.</param>
        public virtual bool CanStack(GUIItem other) => item.CanStack(other.item);

        /// <summary>
        /// Tries to stack a given item on this one.
        /// </summary>
        /// <param name="other">The item you want to stack.</param>
        /// <returns>Returns true if the item was stacked.</returns>
        public virtual bool TryStack(GUIItem other) => item.TryStack(other.item);

        protected virtual void HandleLeftClick()
        {
            if (onMerchant)
                HandleBuy();
            else if (!GUI.instance.selected)
                GUI.instance.Select(this);
            else if (TryStack(GUI.instance.selected))
                GUI.instance.ClearSelection();
            else
                GameAudio.instance.PlayDeniedSound();
        }


        protected virtual void HandleRightClick()
        {
            if (itemRotation != null)
                itemRotation.isHovered = false;
            if (m_hovering)
            {
                m_hovering = false;
                GUIItemInspector.instance.Hide();
            }

            if (onMerchant)
            {
#if UNITY_ANDROID || UNITY_IOS
                HandleBuy();
#endif
                return;
            }

            if (item.IsConsumable())
            {
                TryMovePotionToHotbar();
            }
            else if (m_blacksmith.isOpen)
            {
                HandleBlacksmithEquip();
            }
            else if (m_stash.isOpen)
            {
                HandleMoveToStash();
            }
            else if (m_merchant.isOpen)
            {
                HandleSell();
            }
            else if (m_craftman.isOpen)
            {
                HandleMoveToCraftman();
            }
            else
            {
                HandleEquip();
            }
        }

        protected virtual void HandleBuy()
        {
            if (merchant.TrySell(this))
                merchant = null;
        }

        protected virtual void HandleSell()
        {
            var merchant = m_merchant.GetComponent<GUIMerchant>();

            if (merchant.TryBuy(this))
                this.merchant = merchant;
        }

        protected virtual void HandleMoveToCraftman()
        {
            var craftInventory = m_craftman.GetComponentInChildren<GUICraftmanInventory>();
            var playerInventory = GUIWindowsManager.instance.GetInventory();

            if (craftInventory == null || playerInventory == null)
                return;

            bool changed = false;

            if (transform.IsChildOf(playerInventory.itemsContainer))
            {
                if (craftInventory.TryAutoInsert(this))
                {
                    playerInventory.TryRemove(this);
                    changed = true;
                }
            }
            else if (transform.IsChildOf(craftInventory.itemsContainer))
            {
                if (playerInventory.TryAutoInsert(this))
                {
                    craftInventory.TryRemove(this);
                    changed = true;
                }
            }
            if (changed)
            {
                var items = craftInventory.inventory.items.Keys.ToList();

                m_craftman.GetComponent<GUICraftman>()?.UpdateCraftingPreview(items);
            }
        }



        protected virtual void HandleBlacksmithEquip()
        {
            if (!m_blacksmith.slot.CanEquip(this)) return;

            if (m_inventory.TryRemove(this))
                m_blacksmith.slot.Equip(this);
        }

        protected virtual void HandleEquip()
        {
            if (item.IsEquippable())
            {
                if (m_inventory && m_inventory.equipments.TryAutoEquip(this) &&
                    m_inventory.TryRemove(this))
                    return;
            }
            else if (item.IsConsumable())
            {
                var hud = GUIEntity.instance;

                if (hud && hud.TryEquipConsumable(this))
                    m_inventory?.TryRemove(this);
            }
            else if (item.IsSkill())
            {
                if (player.skills.TryLearnSkill(item.GetSkill()) &&
                    m_inventory.TryRemove(this))
                    Destroy(gameObject);
            }
        }

        protected virtual void HandleMoveToStash()
        {
            var source = GetComponentInParent<GUIInventory>();

            if (source is GUIStash)
            {
                if (m_inventory.TryAutoInsert(this))
                    source.TryRemove(this);
            }
            else if (m_stash.GetComponentInChildren<GUIInventory>().TryAutoInsert(this))
                source.TryRemove(this);
        }

        /// <summary>
        /// Updates the stack size text.
        /// </summary>
        public virtual void UpdateStackText()
        {
            if (!stackText || item == null) return;

            stackText.enabled = item.IsStackable() && item.stack > 1;

            if (stackText.enabled)
                stackText.text = item.stack.ToString();
        }

        /// <summary>
        /// Sets the last position of this GUI Item from a given GUI Inventory.
        /// </summary>
        /// <param name="inventory">The inventory you want to set as last one.</param>
        /// <param name="position">The row and column you want to set as last one.</param>
        public virtual void SetLastPosition(GUIInventory inventory, InventoryCell position)
        {
            m_lastInventory = inventory;
            m_lastInventoryPosition = position;
            m_lastSlot = null;
        }

        /// <summary>
        /// Sets the last position of this GUI Item from a given GUI Slot.
        /// </summary>
        /// <param name="slot">The GUI Slot you want to set as last one.</param>
        public virtual void SetLastPosition(GUIItemSlot slot)
        {
            m_lastSlot = slot;
            m_lastInventory = null;
        }

        /// <summary>
        /// Tries to move this GUI Item to its last position.
        /// </summary>
        /// <returns>Returns true if successfully moved.</returns>
        public virtual bool TryMoveToLastPosition()
        {
            if (GUI.instance.selected == this)
                GUI.instance.Deselect();

            if (m_lastInventory)
            {
                return m_lastInventory.TryInsert(this, m_lastInventoryPosition) ||
                    m_lastInventory.TryAutoInsert(this);
            }

            if (m_lastSlot && m_lastSlot.CanEquip(this))
            {
                m_lastSlot.Equip(this);
                return true;
            }
            else if (Level.instance.player.inventory.instance.TryAddItem(item))
            {
                Destroy(gameObject);
                return true;
            }

            return false;
        }

        public void OnPointerEnter(PointerEventData _)
        {
#if UNITY_STANDALONE || UNITY_WEBGL
            m_hovering = true;
            if (itemRotation != null)
                itemRotation.isHovered = true;

            if (!GUI.instance.selected)
                GUIItemInspector.instance.Show(this);
#endif
        }

        public void OnPointerExit(PointerEventData _)
        {
#if UNITY_STANDALONE || UNITY_WEBGL
            m_hovering = false;
            if (itemRotation != null)
                itemRotation.isHovered = false;

            GUIItemInspector.instance.Hide();
#endif
        }

        public void OnPointerDown(PointerEventData eventData)
        {
#if UNITY_STANDALONE || UNITY_WEBGL
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    HandleLeftClick();
                    break;
                case PointerEventData.InputButton.Right:
                    HandleRightClick();
                    break;
            }
#else
            if (Time.time - m_lastClickTime < k_doubleClickThreshold)
            {
                HandleRightClick();
            }
            else
            {
                GUIItemInspector.instance.Hide();
                GUIItemInspector.instance.Show(this);
                GUIItemInspector.instance.SetPositionRelativeTo((RectTransform)transform);
                EventSystem.current.SetSelectedGameObject(gameObject);
            }

            m_hovering = true;
            m_lastClickTime = Time.time;

#endif
        }

#if UNITY_ANDROID || UNITY_IOS
        public virtual void OnDrag(PointerEventData _)
        {
            if (onMerchant) return;

            GUI.instance.Select(this);
        }

        public virtual void OnEndDrag(PointerEventData _) => GUI.instance.DropItem();

        public virtual void OnDrop(PointerEventData _)
        {
            if (TryStack(GUI.instance.selected))
                GUI.instance.ClearSelection();
        }

        public virtual void OnDeselect(BaseEventData _)
        {
            m_hovering = false;
            GUIItemInspector.instance.Hide();
        }
#endif

        private IEnumerator WaitUntilActiveAndFixCamera()
        {
            Debug.Log($"[GUIItem] Czekam na aktywację {item?.data?.name}");
            yield return new WaitUntil(() => gameObject.activeInHierarchy);
            yield return null;

            Debug.Log($"[GUIItem] GUIItem aktywny – uruchamiam FixCameraPosition dla {item?.data?.name}");
            StartCoroutine(FixCameraPositionAfterModelLoad());
        }


        /// <summary>
        /// Initializes the GUI Item with a given Item Instance.
        /// </summary>
        /// <param name="item">The Item Instance this GUI Item represents.</param>
        public virtual void Initialize(ItemInstance item)
        {
            if (item == null)
            {
                Debug.LogWarning("[GUIItem] Przekazany item == null");
                return;
            }

            this.item = item;
            this.item.onStackChanged += UpdateStackText;

            if (worldImage == null)
            {
                worldImage = GetComponentInChildren<WorldImage>();
                if (worldImage == null)
                {
                    Debug.LogError($"[GUIItem] Brak przypisanego WorldImage dla {item.data.name}! Upewnij się, że prefab GUIItem go zawiera.");
                    return;
                }
            }

            if (item.data.prefab != null)
            {
                var originalModel = Instantiate(item.data.prefab);
                originalModel.transform.SetParent(worldImage.transform, false);
                originalModel.transform.localPosition = Vector3.zero;
                originalModel.transform.localRotation = Quaternion.identity;
                originalModel.transform.localScale = Vector3.one;
                originalModel.name = "Item_Original";

                float scaleOverride = 0f;
                Vector3 cameraOffset = Vector3.zero;
                Vector3 cameraLookAt = Vector3.zero;

                scaleOverride = item.data.previewScaleOverride;
                cameraOffset = item.data.previewCameraOffset;
                cameraLookAt = item.data.previewLookAtOffset;

                worldImage.ResolutionWidth = item.data.previewResolutionWidth;
                worldImage.ResolutionHeight = item.data.previewResolutionHeight;
                worldImage.CameraFollowBoundsCenter = item.data.previewCameraFollowBoundsCenter;

                int previewLayer = LayerMask.NameToLayer("Model_Preview");

                worldImage.CameraCullingMask = 1 << previewLayer;

                worldImage.OnCameraCreated += cam =>
                {
                    var previewLight = cam.transform.Find("PreviewLight")?.gameObject;
                    if (previewLight == null)
                    {
                        previewLight = new GameObject("PreviewLight");
                        previewLight.transform.SetParent(cam.transform, false);
                    }
                    previewLight.layer = previewLayer;

                    var light = previewLight.GetComponent<Light>();
                    if (light == null)
                        light = previewLight.AddComponent<Light>();

                    light.type = LightType.Point;
                    light.range = 2.5f;
                    light.intensity = 1.2f;
                    light.color = Color.white;
                    light.cullingMask = 1 << previewLayer;
                    light.renderingLayerMask = 1 << previewLayer;
                    light.shadows = LightShadows.None;

                    cam.Camera.cullingMask = 1 << previewLayer;
                    cam.Camera.clearFlags = CameraClearFlags.SolidColor;
                    cam.Camera.backgroundColor = Color.clear;
                };

                var renderAnchor = new GameObject("Render_" + item.GetHashCode());
                renderAnchorObject = renderAnchor;
                renderAnchor.transform.position = GetPreviewPositionFor(item);
                renderAnchor.layer = previewLayer;

                var renderModel = Instantiate(item.data.prefab, renderAnchor.transform);
                renderModel.transform.localPosition = Vector3.zero;
                renderModel.transform.localRotation = Quaternion.Euler(item.data.previewRotationEuler);
                renderModel.transform.localScale = (scaleOverride > 0)
                    ? Vector3.one * scaleOverride
                    : FitModelToSlot(renderModel, new Vector2Int(item.columns, item.rows));

                renderModel.gameObject.layer = previewLayer;
                foreach (var t in renderModel.GetComponentsInChildren<Transform>(true))
                    t.gameObject.layer = previewLayer;

                var rotator = renderModel.AddComponent<GUIItemRotation>();
                rotator.target = renderModel.transform;
                itemRotation = rotator;

                foreach (var renderer in renderModel.GetComponentsInChildren<Renderer>(true))
                {
                    renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                    renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    renderer.renderingLayerMask = (uint)(1 << previewLayer);
                }

                foreach (var oldObject in worldImage.WorldObjects.ToList())
                {
                    if (oldObject != null)
                    {
                        worldImage.RemoveWorldObject(oldObject);
                        Destroy(oldObject.gameObject);
                    }
                }

                worldImage.AddWorldObject(renderModel.transform);
                worldImage.CameraLookAtPosition = cameraLookAt;
                worldImage.CameraOffset = cameraOffset;

                var cam = worldImage.ObjectCamera;
                if (cam != null)
                {
                    int previewLayerIndex = LayerMask.NameToLayer("Model_Preview");
                    var previewLight = cam.transform.Find("PreviewLight")?.gameObject;
                    if (previewLight == null)
                    {
                        previewLight = new GameObject("PreviewLight");
                        previewLight.transform.SetParent(cam.transform, false);
                    }
                    previewLight.layer = previewLayerIndex;

                    var light = previewLight.GetComponent<Light>();
                    if (light == null)
                        light = previewLight.AddComponent<Light>();

                    light.type = LightType.Point;
                    light.range = 2.5f;
                    light.cullingMask = 1 << previewLayerIndex;
                    light.renderingLayerMask = (1 << previewLayerIndex);

                    if (cam.Camera != null)
                    {
                        cam.Camera.cullingMask = 1 << previewLayerIndex;
                        var camData = cam.Camera.GetUniversalAdditionalCameraData();
                        int previewVolumeLayer = LayerMask.NameToLayer("PreviewVolume");
                        if (previewVolumeLayer >= 0)
                        {
                            camData.volumeLayerMask = 1 << previewVolumeLayer;
                            camData.volumeTrigger = worldImage.transform;
                        }

                        cam.Image.CameraUseBoundsToClip = true;
                        cam.Image.CameraAutoUpdateBounds = false;
                        cam.Image.CameraFollowBoundsCenter = true;
                        cam.UpdateCameraClippingFromBounds();

                        camData.SetRenderer(1);
                    }
                }

                image.enabled = false;
            }
            else
            {
                image.sprite = item.data.image;
                image.enabled = true;
                Debug.LogWarning($"[GUIItem] Brak prefab w item.data: {item.data.name}");
            }

            stackText.enabled = item.IsStackable();
            ((RectTransform)transform).sizeDelta = new Vector2(item.columns, item.rows) * Inventory.CellSize;

            merchant = GetComponentInParent<GUIMerchant>();
            UpdateStackText();

            SetPreviewActive(false);
        }

        private static int previewCounter = 0;

        private Vector3 GetPreviewPositionFor(ItemInstance item)
        {
            previewSlotIndex = PreviewPositionPool.GetNext();
            if (previewSlotIndex < 0) previewSlotIndex = 0;

            int row = previewSlotIndex / 32;
            int col = previewSlotIndex % 32;
            float spacing = 3f;
            Vector3 basePosition = new Vector3(1000f, 1000f, 0f);
            return basePosition + new Vector3(col * spacing, row * spacing, 0f);
        }

        private IEnumerator FixCameraPositionAfterModelLoad()
        {
            const int maxTries = 30;
            int tries = 0;
            WorldObjectCamera cam = null;

            while (tries < maxTries)
            {
                cam = worldImage.ObjectCamera;
                if (cam != null && cam.Camera != null)
                    break;

                yield return null;
                tries++;
            }

            if (cam != null)
            {
                int previewLayerIndex = LayerMask.NameToLayer("Model_Preview");
                var previewLight = cam.transform.Find("PreviewLight")?.gameObject;
                if (previewLight == null)
                {
                    previewLight = new GameObject("PreviewLight");
                    previewLight.transform.SetParent(cam.transform, false);
                }
                previewLight.layer = previewLayerIndex;

                var light = previewLight.GetComponent<Light>();
                if (light == null)
                    light = previewLight.AddComponent<Light>();

                light.type = LightType.Point;
                light.range = 2.5f;
                light.cullingMask = 1 << previewLayerIndex;
                light.renderingLayerMask = (1 << previewLayerIndex);

                if (cam.Camera != null)
                    cam.Camera.cullingMask = 1 << previewLayerIndex;
            }

            if (cam == null || cam.Camera == null)
            {
                Debug.LogWarning($"[GUIItem] NIE znaleziono kamery dla: {item?.data?.name} po {tries} próbach");
                yield break;
            }

            const int maxBoundsWait = 20;
            int frame = 0;

            while (frame < maxBoundsWait)
            {
                yield return new WaitForEndOfFrame();

                var renderers = worldImage.GetComponentsInChildren<Renderer>();
                bool rendererReady = renderers.Length > 0 && renderers.All(r => r.bounds.size.magnitude > 0.001f);

                var bounds = worldImage.GetWorldObjectsBounds();

                if (bounds.HasValue && bounds.Value.size.magnitude > 0.001f)
                {
                    cam.Image.CameraUseBoundsToClip = true;
                    cam.Image.CameraAutoUpdateBounds = false;
                    cam.Image.CameraFollowBoundsCenter = true;
                    cam.UpdateCameraClippingFromBounds();
                }
                else
                {
                    Debug.LogWarning($"BOUNDS PUSTE dla {item?.data?.name} — kamera nie została ustawiona.");
                }

                frame++;
            }

            Debug.LogWarning($"[FixCamera] bounds NIE gotowe dla: {item?.data?.name}");
        }
        
        private Vector3 FitModelToSlot(GameObject model, Vector2Int slotSize)
        {
            var renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return Vector3.one;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            float maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (maxDim == 0f)
                return Vector3.one;

            float targetVisualSize = slotSize.magnitude * 0.5f;
            float scale = targetVisualSize / maxDim;

            return Vector3.one * scale;
        }

        protected virtual void OnDisable()
        {
            if (itemRotation != null)
                itemRotation.isHovered = false;

            if (m_hovering)
            {
                m_hovering = false;
                GUIItemInspector.instance.Hide();
            }
        }

        private void TryMovePotionToHotbar()
        {
            var hud = GUIEntity.instance;
            if (hud == null)
            {
                // Debug.LogWarning("GUIEntity.instance is null. Cannot move potion to hotbar.");
                return;
            }

            List<GUIConsumableSlot> availableSlots = new List<GUIConsumableSlot>();
            GUIConsumableSlot freeSlot = null;

            foreach (var slot in hud.ConsumableSlots)
            {
                if (slot.item != null && slot.item.item.data == item.data)
                {
                    availableSlots.Add(slot);
                }
                if (slot.item == null && freeSlot == null)
                {
                    freeSlot = slot;
                }
            }

            foreach (var slot in availableSlots)
            {
                if (item.stack <= 0)
                    break;

                int maxStack = item.data.stackCapacity;
                int spaceLeft = maxStack - slot.item.item.stack;

                if (spaceLeft > 0)
                {
                    int amountToAdd = Mathf.Min(spaceLeft, item.stack);
                    slot.item.item.stack += amountToAdd;
                    item.stack -= amountToAdd;

                    slot.item.UpdateStackText();
                    // Debug.Log($"[HOTBAR] Added {amountToAdd} potions to existing stack. Remaining in inventory: {item.stack}");
                }
            }

            if (item.stack > 0 && freeSlot != null)
            {
                // Debug.Log($"[HOTBAR] Moving {item.stack} potions to new free slot.");

                freeSlot.Equip(this);

                var inventory = Level.instance.player.inventory.instance;
                inventory.TryRemoveItem(item);

                var guiInventory = GUI.instance.GetComponentInChildren<GUIInventory>();
                if (guiInventory != null)
                {
                    // Debug.Log("[HOTBAR] Updating inventory slots to remove 'ghost' slot.");
                    guiInventory.UpdateSlots();
                }

                GUI.instance.Deselect();
            }

            if (item.stack == 0)
            {
                // Debug.Log("[HOTBAR] Potion stack is 0, removing from inventory and GUI.");

                var inventory = Level.instance.player.inventory.instance;
                bool removed = inventory.TryRemoveItem(item);
                if (!removed)
                {
                    Debug.LogError($"[HOTBAR] Failed to remove {item.GetName()} from inventory!");
                }
                else
                {
                    // Debug.Log($"[HOTBAR] {item.GetName()} successfully removed from inventory.");
                }

                var guiInventory = GUI.instance.GetComponentInChildren<GUIInventory>();
                if (guiInventory != null)
                {
                    guiInventory.UpdateSlots();
                }

                // Debug.Log($"[HOTBAR] Destroying game object {gameObject.name}");
                Destroy(gameObject);
            }
        }

        public void SetPreviewActive(bool state)
        {
            if (worldImage != null)
                worldImage.enabled = state;
            if (worldImage == null) return;

            worldImage.enabled = state;

            var cam = worldImage.ObjectCamera?.Camera;
            if (cam != null)
                cam.enabled = state;
        }

        private void Update()
        {
            if (worldImage != null)
            {
                var cam = worldImage.ObjectCamera?.Camera;
                if (cam != null && cam.targetTexture == null && worldImage.RenderTexture != null)
                {
                    cam.targetTexture = worldImage.RenderTexture;
            #if UNITY_EDITOR
                    Debug.LogWarning($"[GUIItem] 🛠️ Naprawiam brakujący RenderTexture dla {item?.data?.name}");
            #endif
                }
            }

            var currentResolution = new Vector2Int(Screen.width, Screen.height);

            if (currentResolution != lastResolution)
            {
                lastResolution = currentResolution;
                if (item?.data?.prefab != null && worldImage != null)
                {
                    Debug.Log($"[GUIItem] Zmiana rozdzielczości — wymuszam update dla {item.data.name}");
                    StartCoroutine(FixCameraPositionAfterModelLoad());
                }
            }

            if (GUI.instance.selected == this && worldImage != null)
            {
                worldImage.transform.localPosition = Vector3.zero;
                worldImage.transform.localRotation = Quaternion.identity;

                var model = worldImage.WorldObjects?.FirstOrDefault();
                if (model != null)
                {
                    model.localPosition = frozenModelLocalPos;
                    model.localRotation = Quaternion.Euler(item.data.previewRotationEuler);
                }
            }

        }
    
        private int debugFrames = 3;

        private void LateUpdate()
        {
            if (worldImage == null || item == null)
                return;

            var cam = worldImage.ObjectCamera?.Camera;

            if (cam != null && cam.targetTexture == null && worldImage.RenderTexture != null)
            {
                cam.targetTexture = worldImage.RenderTexture;

        #if UNITY_EDITOR
                Debug.LogWarning($"[FixCam] LateUpdate naprawił brak RenderTexture dla: {item.data.name}");
        #endif
            }

            if (debugFrames > 0)
            {
                debugFrames--;
            }
        }

        protected virtual void OnDestroy()
        {
            if (previewSlotIndex >= 0)
                PreviewPositionPool.Release(previewSlotIndex);

            if (worldImage != null)
            {
                var cam = worldImage.ObjectCamera;
                var previewLight = cam?.transform.Find("PreviewLight");
                if (previewLight != null)
                    Destroy(previewLight.gameObject);
            }

            if (renderAnchorObject != null)
                Destroy(renderAnchorObject);
        }
    }
}