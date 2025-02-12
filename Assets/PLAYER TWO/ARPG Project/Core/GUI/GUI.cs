using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI")]
    public class GUI : Singleton<GUI>
    {
        [SerializeField] private GUIWindowsManager windowsManager; // referencja do GUI Windows Manager
        [SerializeField] private GameObject gameMenu; // Referencja do menu gry
        
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

        public float dropRange = 2f; // Editable range around the player to drop items

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

        protected Entity m_entity;

        protected float m_dropTime;

        public GUIItem selected { get; protected set; }

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
            //m_toggleMenu.performed += _ => onToggleMenu.Invoke();
            //m_toggleMenu.performed += _ => HandleEscape(); // Zmiana tutaj!
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

            Vector3 dropPosition;

            if (m_entity.inputs.MouseRaycast(out var hit, dropGroundLayer))
            {
                // If the raycast hits the ground, calculate the drop position based on the hit point
                Vector3 direction = (hit.point - m_entity.transform.position).normalized;
                float distance = Mathf.Min(Vector3.Distance(m_entity.transform.position, hit.point), dropRange);
                dropPosition = m_entity.transform.position + direction * distance;

                // Create the item at the player's feet
                var collectible = Instantiate(droppedItemPrefab, m_entity.transform.position, Quaternion.identity);
                collectible.SetItem(selected.item);
                Destroy(selected.gameObject);
                selected = null;
                m_dropTime = Time.time;

                // Start the drop animation in an arc path
                StartCoroutine(AnimateDropItem(collectible.transform, dropPosition));
            }
            else
            {
                // If the raycast does not hit the ground, move the item back to the inventory
#if UNITY_ANDROID || UNITY_IOS
        selected.TryMoveToLastPosition();
        Deselect();
#endif
            }
        }

        private void HandleEscape()
        {
            Debug.Log($"HandleEscape() called at {Time.time}");

            if (windowsManager == null)
            {
                Debug.LogError("GUIWindowsManager is not assigned in GUI.cs!");
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
                Debug.LogError("Game Menu is not assigned in GUI.cs!");
                return;
            }

            bool isActive = gameMenu.activeSelf;
            gameMenu.SetActive(!isActive);

            // Odtwarzanie dźwięku otwierania/zamykania menu
            GameAudio.instance.PlayUiEffect(isActive ? windowsManager.closeClip : windowsManager.openClip);

            // Pauza gry bez użycia GamePause.Instance
            Time.timeScale = isActive ? 1 : 0; // 1 = normalna gra, 0 = pauza

            Debug.Log($"Game Menu {(isActive ? "closed" : "opened")}, Game Paused: {!isActive}");
        }

        private Vector3 GetRandomDropPosition()
        {
            float angle = Random.Range(0f, 360f);
            float radius = Random.Range(0f, dropRange);
            Vector3 randomOffset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            return randomOffset;
        }

        private IEnumerator AnimateDropItem(Transform itemTransform, Vector3 targetPosition)
        {
            float animationDuration = 1f; // Duration of the animation
            float elapsedTime = 0f;

            Vector3 startPosition = itemTransform.position;
            Vector3 controlPoint = startPosition + (targetPosition - startPosition) / 2 + Vector3.up * 2f; // Control point to create the arc

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / animationDuration;

                // Calculate the position on the quadratic Bezier curve
                Vector3 currentPos = Mathf.Pow(1 - t, 2) * startPosition +
                                     2 * (1 - t) * t * controlPoint +
                                     Mathf.Pow(t, 2) * targetPosition;

                itemTransform.position = currentPos;

                yield return null;
            }

            // Ensure the item lands exactly at the target position
            itemTransform.position = targetPosition;
        }

        protected virtual void HandleItemPosition()
        {
            if (!selected) return;

            selected.transform.position = EntityInputs.GetPointerPosition();
        }

        protected virtual void HandleDropEntityRestoration()
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

        protected virtual void Start()
        {
            InitializeEntity();
            InitializeActions();
            InitializeCallbacks();
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
