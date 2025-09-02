using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Accessibility Settings Window")]
    public class GUIAccessibilitySettingsWindow : GUIWindow
    {
        [Header("Navigation Buttons")]
        public Button backButton;

        [Header("Accessibility Options")]
        public Toggle colorblindFilterToggle;
        public Toggle largeTextToggle;
        public Toggle subtitlesToggle;

        protected GameSettings m_settings => GameSettings.instance;

        protected override void Start()
        {
            base.Start();

            if (backButton != null)
                backButton.onClick.AddListener(BackButton);
            else
                Debug.LogError("[Settings] Back Button is NULL! Assign it in the Inspector.");

            if (colorblindFilterToggle != null)
            {
                colorblindFilterToggle.isOn = m_settings.GetColorblindMode();
                colorblindFilterToggle.onValueChanged.AddListener(OnColorblindToggleChanged);
            }
            else
            {
                Debug.LogError("[Settings] Colorblind Filter Toggle is NULL! Assign it in the Inspector.");
            }

            if (largeTextToggle != null)
            {
                largeTextToggle.isOn = m_settings.GetLargeText();
                largeTextToggle.onValueChanged.AddListener(OnLargeTextToggleChanged);
            }
            else
            {
                Debug.LogError("[Settings] Large Text Toggle is NULL! Assign it in the Inspector.");
            }

            if (subtitlesToggle != null)
            {
                subtitlesToggle.isOn = m_settings.GetSubtitlesEnabled();
                subtitlesToggle.onValueChanged.AddListener(OnSubtitlesToggleChanged);
            }
            else
            {
                Debug.LogError("[Settings] Subtitles Toggle is NULL! Assign it in the Inspector.");
            }
        }

        protected virtual void OnDisable() => m_settings?.Save();

        protected virtual void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                m_settings.Save();
        }

        private void OnColorblindToggleChanged(bool isOn)
        {
            m_settings.SetColorblindMode(isOn);
        }

        private void OnLargeTextToggleChanged(bool isOn)
        {
            m_settings.SetLargeText(isOn);
        }

        private void OnSubtitlesToggleChanged(bool isOn)
        {
            m_settings.SetSubtitlesEnabled(isOn);
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
}
