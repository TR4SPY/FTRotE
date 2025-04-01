using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.AI;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI")]
    public class GUI : Singleton<GUI>
    {
        [SerializeField] private GUIWindowsManager windowsManager;
        [SerializeField] private GameObject gameMenu;
        
        public UnityEvent<GUIItem> onSelectItem;
        public UnityEvent<GUIItem> onDeselectItem;

        [Tooltip("The Input Action Asset with all GUI actions.")]
        public InputActionAsset actions;

        [Header("Items Settings")]
        [Tooltip("The prefab to use when instantiating GUI Items.")]
        public GUIItem itemPrefab;

        [Header("Containers Settings")]
        [Tooltip("The container with all collectible titles.")]
        public RectTransform collectiblesContainer;

        [Header("Item Drop Settings")]
        [Tooltip("If true, the Player can drop items from the GUI.")]
        public bool canDropItems = true;

        [Tooltip("The duration in seconds before being able to move the Player after dropping an Item.")]
        public float movementRestorationDelay = 0.2f;

        [Tooltip("The Layer Mask of the ground to drop items.")]
        public LayerMask dropGroundLayer;

        [Tooltip("The prefab instantiated when dropping an Item on the ground.")]
        public CollectibleItem droppedItemPrefab;

        [Tooltip("Maximum range around the player to drop items.")]
        public float dropRange = 2f;

        [Header("Input Callbacks")]
        public UnityEvent onToggleSkills;
        public UnityEvent onToggleCharacter;
        public UnityEvent onToggleInventory;
        public UnityEvent onToggleQuestLog;
        public UnityEvent onToggleMenu;
        public UnityEvent onToggleCollectiblesNames;

        protected InputAction m_toggleSkills;
        protected InputAction m_toggleCharacter;
        protected InputAction m_toggleInventory;
        protected InputAction m_toggleQuestLog;
        protected InputAction m_toggleMenu;
        protected InputAction m_toggleMenuWebGL;
        protected InputAction m_dropItem;
        protected InputAction m_toggleCollectiblesNames;
        protected GameAudio m_audio => GameAudio.instance;

        protected Entity m_entity;
        protected float m_dropTime;

        public GUIItem selected { get; protected set; }

        protected virtual void Start()
        {
            InitializeEntity();
            InitializeActions();
            InitializeCallbacks();
        }

        protected virtual void InitializeEntity() => m_entity = Level.instance.player;

        protected virtual void InitializeActions()
        {
            m_toggleSkills = actions["Toggle Skills"];
            m_toggleCharacter = actions["Toggle Character"];
            m_toggleInventory = actions["Toggle Inventory"];
            m_toggleQuestLog = actions["Toggle Quest Log"];
            m_toggleMenu = actions["Toggle Menu"];
            m_toggleMenuWebGL = actions["Toggle Menu (WebGL)"];
            m_dropItem = actions["Drop Item"];
            m_toggleCollectiblesNames = actions["Toggle Collectibles Names"];
        }

        protected virtual void InitializeCallbacks()
        {
            m_toggleSkills.performed += _ => onToggleSkills.Invoke();
            m_toggleCharacter.performed += _ => onToggleCharacter.Invoke();
            m_toggleInventory.performed += _ => onToggleInventory.Invoke();
            m_toggleQuestLog.performed += _ => onToggleQuestLog.Invoke();
#if UNITY_WEBGL
            m_toggleMenuWebGL.performed += _ => onToggleMenu.Invoke();
#else
            m_toggleMenu.performed -= _ => HandleEscape();
            m_toggleMenu.performed += _ => HandleEscape();
#endif
            m_dropItem.performed += _ => DropItem();
            m_toggleCollectiblesNames.performed += _ => onToggleCollectiblesNames.Invoke();
        }

        public virtual void Select(GUIItem item)
        {
            if (!selected)
            {
                selected = item;
                selected.transform.SetParent(transform);
                selected.Select();
                m_entity.canUpdateDestination = false;
                GUIItemInspector.instance?.Hide();
                onSelectItem?.Invoke(selected);
            }
        }

        public virtual void Deselect()
        {
            if (selected)
            {
                var item = selected;
                selected.Deselect();
                selected = null;
                m_entity.canUpdateDestination = true;
                onDeselectItem?.Invoke(item);
            }
        }

        public virtual void ClearSelection()
        {
            if (selected)
            {
                Destroy(selected.gameObject);
                selected = null;
                m_entity.canUpdateDestination = true;
            }
        }

        public virtual void DropItem()
        {
            if (!selected || !canDropItems) return;

            var item = selected.item;

            if (item == null || item.data == null)
                return;

            if (item.data.cannotBeDropped && !IsDroppingIntoInventory())
            {
                m_audio.PlayDeniedSound();
                selected.TryMoveToLastPosition();
                Deselect();
                return;
            }

            if (m_entity.inputs.MouseRaycast(out var hit, dropGroundLayer))
            {
                Vector3 direction = (hit.point - m_entity.transform.position).normalized;
                float distance = Mathf.Min(Vector3.Distance(m_entity.transform.position, hit.point), dropRange);
                Vector3 dropPosition = m_entity.transform.position + direction * distance;

                dropPosition = FindNearestNavMeshPosition(dropPosition, 2f);

                var collectible = Instantiate(droppedItemPrefab, m_entity.transform.position, Quaternion.identity);
                collectible.SetItem(item);

                Destroy(selected.gameObject);
                selected = null;
                m_dropTime = Time.time;

                StartCoroutine(AnimateDropItem(collectible.transform, dropPosition));
            }
            else
            {
        #if UNITY_ANDROID || UNITY_IOS
                selected.TryMoveToLastPosition();
                Deselect();
        #endif
            }
        }

        public void DropItem(ItemInstance item)
        {
            if (item == null || item.data == null || GUI.instance.droppedItemPrefab == null)
                return;

            Vector3 dropPosition = Level.instance.player.transform.position;

            dropPosition += Random.insideUnitSphere * dropRange;
            dropPosition.y = Level.instance.player.transform.position.y;

            var collectible = Instantiate(droppedItemPrefab, dropPosition, Quaternion.identity);
            collectible.SetItem(item);
        }

        private bool IsDroppingIntoInventory()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }


        private Vector3 FindNearestNavMeshPosition(Vector3 center, float searchRadius)
        {
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(center, out navHit, searchRadius, NavMesh.AllAreas))
            {
                return navHit.position;
            }
            return center;
        }

        private IEnumerator AnimateDropItem(Transform itemTransform, Vector3 targetPosition)
        {
            float animationDuration = 1f; // Duration of the animation
            float elapsedTime = 0f;

            Vector3 startPosition = itemTransform.position;
            Vector3 controlPoint = startPosition + (targetPosition - startPosition) / 2 + Vector3.up * 2f;

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / animationDuration;

                // Quadratic bezier
                Vector3 currentPos = Mathf.Pow(1 - t, 2) * startPosition
                                   + 2 * (1 - t) * t * controlPoint
                                   + Mathf.Pow(t, 2) * targetPosition;

                itemTransform.position = currentPos;
                yield return null;
            }

            // final
            itemTransform.position = targetPosition;
        }

        private void HandleEscape()
        {
            if (windowsManager == null)
            {
                return;
            }

            if (gameMenu != null && gameMenu.activeSelf)
            {
                ToggleGameMenu();
                return;
            }

            if (windowsManager.HasOpenWindows())
            {
                windowsManager.CloseLastOpenedWindow();
                return;
            }

            ToggleGameMenu();
        }

        public void ToggleGameMenu()
        {
            if (gameMenu == null)
            {
                return;
            }

            bool isActive = gameMenu.activeSelf;
            gameMenu.SetActive(!isActive);

            // Play sound for opening / closing
            GameAudio.instance.PlayUiEffect(isActive ? windowsManager.closeClip : windowsManager.openClip);

            // Pause game
            Time.timeScale = isActive ? 1 : 0;
        }

        private void HandleItemPosition()
        {
            if (!selected) return;
            selected.transform.position = EntityInputs.GetPointerPosition();
        }

        private void HandleDropEntityRestoration()
        {
            if (!selected && !m_entity.canUpdateDestination &&
                Time.time - m_dropTime > movementRestorationDelay)
            {
                m_entity.canUpdateDestination = true;
            }
        }

        public GUIItem CreateGUIItem(ItemInstance item, RectTransform container = null)
        {
            var parent = container ? container : transform;
            var instance = Instantiate(itemPrefab, parent);
            instance.Initialize(item);
            return instance;
        }

        protected virtual void LateUpdate()
        {
            HandleItemPosition();
            HandleDropEntityRestoration();
        }

        protected virtual void OnEnable() => actions.Enable();
        protected virtual void OnDisable() => actions.Disable();
    }
}
