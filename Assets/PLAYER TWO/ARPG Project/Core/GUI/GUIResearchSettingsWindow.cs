using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using AI_DDA.Assets.Scripts;
using PLAYERTWO.ARPGProject;

    public class GUIResearchSettingsWindow : GUIWindow
    {
        [Header("Research Settings")]
        [Tooltip("References the toggle that controls saving of the logs.")]
        public Toggle saveLogsToggle;

        [Header("Navigation Buttons")]
        public Button backButton;

        protected GameSettings m_settings => GameSettings.instance;

        protected override void Start()
        {
            base.Start();

            InitializeSaveLogs();
            saveLogsToggle.onValueChanged.AddListener(OnSaveLogsToggleChanged);

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

        protected virtual void InitializeSaveLogs()
        {
            saveLogsToggle.isOn = m_settings.GetSaveLogs();
            saveLogsToggle.onValueChanged.AddListener(OnSaveLogsToggleChanged);
        }

        private void OnSaveLogsToggleChanged(bool isOn)
        {
            m_settings.SetSaveLogs(isOn);

            if (PlayerBehaviorLogger.Instance != null)
            {
                PlayerBehaviorLogger.Instance.isLoggingEnabled = isOn;
            }

            Debug.Log($"Save Logs is now: {(isOn ? "Enabled" : "Disabled")}");
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