using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using AI_DDA.Assets.Scripts;
using PLAYERTWO.ARPGProject;

    public class GUIGraphicsSettingsWindow : GUIWindow
    {
        [Header("Graphics Settings")]
        [Tooltip("References the dropdown used to display the game's resolutions.")]
        public TMP_Dropdown resolution;

        [Tooltip("References the toggle that controls the full screen state.")]
        public Toggle fullScreen;

        [Tooltip("References the toggle that controls the post-processing state.")]
        public Toggle postProcessing;

/*
        [Header("References")]
        [Tooltip("The Volume component controlling post-processing effects.")]
        public Volume postProcessingVolume;
*/

        [Header("Navigation Buttons")]
        public Button backButton;

        protected GameSettings m_settings => GameSettings.instance;

        protected override void Start()
        {
            base.Start();

            InitializeResolution();
            InitializeFullScreen();
            InitializePostProcessing();

            if (backButton != null)
            {
                backButton.onClick.AddListener(BackButton);
            }
            else
            {
                Debug.LogError("[Settings] Back Button is NULL! Assign it in the Inspector.");
            }
        }

        protected virtual void OnDisable() => m_settings?.Save();

        protected virtual void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                m_settings.Save();
        }

        protected virtual void InitializeResolution()
        {
            resolution.ClearOptions();
            resolution.AddOptions(m_settings.GetResolutions());
            resolution.value = m_settings.GetResolution();
            resolution.onValueChanged.AddListener(m_settings.SetResolution);
#if UNITY_WEBGL
            resolution.interactable = false;
#endif
        }

        protected virtual void InitializeFullScreen()
        {
            fullScreen.isOn = m_settings.GetFullScreen();
            fullScreen.onValueChanged.AddListener(m_settings.SetFullScreen);
        }

/*
        protected virtual void InitializePostProcessing()
        {
            postProcessing.isOn = m_settings.GetPostProcessing();
            postProcessing.onValueChanged.AddListener(m_settings.SetPostProcessing);
        }
*/
/*
        protected virtual void InitializePostProcessing()
        {
            // Ustaw początkowy stan Toggle zgodnie z aktualnym ustawieniem
            bool isPostProcessingEnabled = m_settings.GetPostProcessing();
            postProcessing.isOn = isPostProcessingEnabled;

            // Ustaw aktywność Volume w oparciu o początkowe ustawienie
            if (postProcessingVolume != null)
                postProcessingVolume.enabled = isPostProcessingEnabled;

            // Dodaj listener do Toggle
            postProcessing.onValueChanged.AddListener(SetPostProcessingState);
        }

        protected virtual void SetPostProcessingState(bool isEnabled)
        {
            // Zapisz ustawienie w Settings
            m_settings.SetPostProcessing(isEnabled);

            // Włącz/wyłącz Volume
            if (postProcessingVolume != null)
                postProcessingVolume.enabled = isEnabled;
        }
*/

        protected virtual void InitializePostProcessing()
        {
            postProcessing.isOn = m_settings.GetPostProcessing();
            postProcessing.onValueChanged.AddListener(m_settings.SetPostProcessing);
        }

        public void BackButton()
        {
            Hide();
            if (GUIWindowsManager.Instance != null && GUIWindowsManager.Instance.settingsWindow != null)
            {
                GUIWindowsManager.Instance.settingsWindow.ShowMainSettings();
            }
            else
            {
                Debug.LogError("[Settings] settingsWindow is NULL! Make sure it's assigned in GUIWindowsManager.");
            }
        }
    }
