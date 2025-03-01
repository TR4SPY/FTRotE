using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Windows Manager")]
    public class GUIWindowsManager : Singleton<GUIWindowsManager>
    {
        [SerializeField] private GUI gui;

        [Tooltip("A reference to the GUI Skills Manager.")]
        public GUISkillsManager skills;

        [Tooltip("A reference to the GUI Stats Manager.")]
        public GUIStatsManager stats;

        [Tooltip("A reference to the GUI Player Inventory.")]
        public GUIWindow inventoryWindow;

        [Tooltip("A reference to the GUI Quest Window.")]
        public GUIQuestWindow quest;

        [Tooltip("A reference to the GUI Quest Log.")]
        public GUIQuestLog questLog;

        [Tooltip("A reference to the GUI Blacksmith.")]
        public GUIBlacksmith blacksmith;

        [Tooltip("A reference to all GUI Windows in the game.")]
        private List<GUIWindow> windows;

        [Tooltip("Reference to the Game Menu.")]
        public GameObject gameMenu;

        [Tooltip("A reference to the Settings Window.")]
        public GUISettingsWindow settingsWindow;

        [Tooltip("A reference to the Stash Window.")]
        public GUIWindow stashWindow;

        [Tooltip("A reference to the GUI Merchant.")]
        public GUIWindow merchantWindow;

        [Tooltip("A reference to the GUI Waypoints Window.")]
        public GUIWindow waypointsWindow;

        [Tooltip("A reference to the GUI Information.")]
        public GUIWindow informationWindow;

        [Tooltip("A reference to the GUI Questionnaire.")]
        public GUIWindow questionnaireWindow;

        [Tooltip("A reference to the GUI Dialog Window.")]
        public GUIDialogWindow dialogWindow;


        [Header("Audio Settings")]
        [Tooltip("The Audio Clip that plays when opening windows.")]
        public AudioClip openClip;

        [Tooltip("The Audio Clip that plays when closing windows.")]
        public AudioClip closeClip;

        protected GameAudio m_audio => GameAudio.instance;
        public static GUIWindowsManager Instance { get; private set; }

        protected override void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            base.Awake();
        }

        /// <summary>
        /// Returns the reference to the GUI Player Inventory.
        /// </summary>
        public GUIPlayerInventory GetInventory()
        {
            if (!inventoryWindow) return null;

            return inventoryWindow.GetComponent<GUIPlayerInventory>();
        }

        /// <summary>
        /// Returns the reference to the GUI Merchant.
        /// </summary>
        public GUIMerchant GetMerchant()
        {
            if (!merchantWindow) return null;

            return merchantWindow.GetComponent<GUIMerchant>();
        }

        /// <summary>
        /// Returns the reference to the GUI Information.
        /// </summary>
        public GUIInformation GetInformation()
        {
            if (!informationWindow) return null;

            return informationWindow.GetComponent<GUIInformation>();
        }

        protected virtual void Start()
        {
            gui = FindFirstObjectByType<GUI>();

            if (settingsWindow == null)
            {
                Debug.LogError("[GUIWindowsManager] settingsWindow is NULL! Assign it in the Inspector.");
            }
            
            windows = new List<GUIWindow>(GetComponentsInChildren<GUIWindow>(true));

            foreach (var window in windows)
            {
                window.onOpen.AddListener(() => m_audio?.PlayUiEffect(openClip));
                window.onClose.AddListener(() => m_audio?.PlayUiEffect(closeClip));
            }

            Debug.Log($"GUIWindowsManager initialized. Found {windows.Count} windows.");
        }

        public void ResetWindowsState()
        {
            foreach (var window in windows)
            {
                window.Hide(); // Zamknij wszystkie okna
            }
        }

        public bool HasOpenWindows()
        {
            return windows.Exists(w => w.isOpen);
        }

        public void CloseLastOpenedWindow()
        {
            GUIWindow lastWindow = GetLastOpenedWindow();
            if (lastWindow != null)
            {
                lastWindow.Hide();
            }
        }

        private GUIWindow GetLastOpenedWindow()
        {
            for (int i = windows.Count - 1; i >= 0; i--)
            {
                if (windows[i] != null && windows[i].isOpen)
                {
                    return windows[i];
                }
            }
            return null;
        }
    }
}
