using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using AI_DDA.Assets.Scripts;
using UnityEngine.SceneManagement;

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

        [Tooltip("A reference to the GUI Stats Extended Manager.")]
        public GUIExtendedStatsManager extendedStats;

        [Tooltip("A reference to the GUI Player Inventory.")]
        public GUIWindow inventoryWindow;

        [Tooltip("A reference to the GUI Quest Window.")]
        public GUIQuestWindow quest;

        [Tooltip("A reference to the GUI Exclusive Quest Window.")]
        public GUIExclusiveWindow exclusiveWindow;

        [Tooltip("A reference to the GUI Quest Log.")]
        public GUIQuestLog questLog;

        [Tooltip("A reference to the GUI Blacksmith.")]
        public GUIBlacksmith blacksmith;

        [Tooltip("A reference to the GUI Craftman.")]
        public GUIWindow craftmanWindow;

        [Tooltip("A reference to the GUI Crafting Recipies.")]
        public GUIWindow craftingRecipiesWindow;

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

        [Tooltip("A reference to the GUI Help Window.")]
        public GUIHelpWindow helpWindow;

        [Tooltip("A reference to the GUI Chat Window.")]
        public GUIChatWindow chatWindow;

        [Tooltip("A reference to the Minimap HUD Window.")]
        public GUIWindow minimapWindow;
        [Tooltip("A reference to the GUI Guildmaster.")]
        public GUIGuildmaster guildmasterWindow;

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

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RebuildWindowsList();
        }

        private void RebuildWindowsList()
        {
            windows = new List<GUIWindow>(GetComponentsInChildren<GUIWindow>(true));

            foreach (var window in windows)
            {
                window.onOpen.AddListener(() => m_audio?.PlayUiEffect(openClip));
                window.onClose.AddListener(() => m_audio?.PlayUiEffect(closeClip));
            }

            Debug.Log($"[GUIWindowsManager] Windows list rebuilt. Found {windows.Count} windows.");
        }

        public GUIPlayerInventory GetInventory()
        {
            if (!inventoryWindow) return null;
            return inventoryWindow.GetComponent<GUIPlayerInventory>();
        }

        public GUIMerchant GetMerchant()
        {
            if (!merchantWindow) return null;
            return merchantWindow.GetComponent<GUIMerchant>();
        }

        public GUICraftman GetCraftman()
        {
            if (!craftmanWindow) return null;
            return craftmanWindow.GetComponent<GUICraftman>();
        }

        public GUIGuildmaster GetGuildmaster() => guildmasterWindow;

        public GUIInformation GetInformation()
        {
            if (!informationWindow) return null;
            return informationWindow.GetComponent<GUIInformation>();
        }

        public GUIChatWindow GetChatWindow()
        {
            return chatWindow;
        }

        protected virtual void Start()
        {
            gui = FindFirstObjectByType<GUI>();

            if (settingsWindow == null)
            {
                Debug.LogError("[GUIWindowsManager] settingsWindow is NULL! Assign it in the Inspector.");
            }

            RebuildWindowsList();

           // Debug.Log($"GUIWindowsManager initialized. Found {windows.Count} windows.");

            if (stats != null)
            {
                var statsWindow = stats.GetComponent<GUIWindow>();
                if (statsWindow != null)
                {
                    statsWindow.onOpen.AddListener(() =>
                    {
                        PlayerBehaviorLogger.Instance?.UpdatePlayerType();
                        stats.Refresh();
                    });
                }
                else
                {
                    Debug.LogWarning("[GUIWindowsManager] Cannot find GUIStatsManager.");

                }
            }

            if (extendedStats != null)
            {
                var extendedWindow = extendedStats.GetComponent<GUIWindow>();
                if (extendedWindow != null)
                {
                    extendedWindow.onOpen.AddListener(() =>
                    {
                        PlayerBehaviorLogger.Instance?.UpdatePlayerType();
                        extendedStats.Refresh();
                    });
                }
                else
                {
                    Debug.LogWarning("[GUIWindowsManager] Cannot find GUIExtendedStatsManager.");
                }
            }

            if (dialogWindow != null)
            {
                var dialogGUIWindow = dialogWindow.GetComponent<GUIWindow>();
                if (dialogGUIWindow != null)
                {
                    dialogGUIWindow.onOpen.AddListener(() =>
                    {
                        PlayerBehaviorLogger.Instance?.UpdatePlayerType();
                        dialogWindow.Refresh();
                    });
                }
                else
                {
                    Debug.LogWarning("[GUIWindowsManager] Cannot find GUIDialogWindow.");
                }
            }
        }

        public void ResetWindowsState()
        {
            foreach (var window in windows)
            {
                if (window != null)
                {
                    window.Hide();
                }
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
