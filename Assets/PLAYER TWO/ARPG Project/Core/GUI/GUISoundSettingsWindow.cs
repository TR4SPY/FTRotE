using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using AI_DDA.Assets.Scripts;
using PLAYERTWO.ARPGProject;
using Unity.Mathematics;

public class GUISoundSettingsWindow : GUIWindow
    {
        [Header("Sound Settings")]
        [Tooltip("References the music volume slider.")]
        public Slider musicVolume;

        [Tooltip("References the effects volume slider.")]
        public Slider effectsVolume;

        [Tooltip("References the ui effects volume slider.")]
        public Slider uiVolume;

        [Tooltip("References the output device dropdown.")]
        public TMP_Dropdown outputDevice;

        [Tooltip("References the toggle controlling mute on focus loss.")]
        public Toggle muteOnFocusLoss;

        [Header("Navigation Buttons")]
        public Button backButton;

        protected GameSettings m_settings => GameSettings.instance;

        protected override void Start()
        {
            base.Start();

            InitializeMusicVolume();
            InitializeEffectsVolume();
            InitializeUIEffectsVolume();
            InitializeOutputDevice();
            InitializeMuteOnFocusLoss();

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

        protected virtual void InitializeMusicVolume()
        {
            musicVolume.value = m_settings.GetMusicVolume();
            musicVolume.onValueChanged.AddListener(m_settings.SetMusicVolume);
        }

        protected virtual void InitializeEffectsVolume()
        {
            effectsVolume.value = m_settings.GetEffectsVolume();
            effectsVolume.onValueChanged.AddListener(m_settings.SetEffectsVolume);
        }

        protected virtual void InitializeUIEffectsVolume()
        {
            uiVolume.value = m_settings.GetUIEffectsVolume();
            uiVolume.onValueChanged.AddListener(m_settings.SetUIEffectsVolume);
        }

        protected virtual void InitializeOutputDevice()
        {
            if (outputDevice == null)
                return;
            outputDevice.ClearOptions();
            outputDevice.AddOptions(m_settings.GetOutputDevices());
            outputDevice.value = m_settings.GetOutputDevice();
            outputDevice.onValueChanged.AddListener(m_settings.SetOutputDevice);
        }

        protected virtual void InitializeMuteOnFocusLoss()
        {
            if (muteOnFocusLoss == null)
                return;
            muteOnFocusLoss.isOn = m_settings.GetMuteOnFocusLoss();
            muteOnFocusLoss.onValueChanged.AddListener(m_settings.SetMuteOnFocusLoss);
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
