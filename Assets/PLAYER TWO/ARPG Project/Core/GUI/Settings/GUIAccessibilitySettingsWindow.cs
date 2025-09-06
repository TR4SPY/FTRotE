using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Accessibility Settings Window")]
    public class GUIAccessibilitySettingsWindow : GUIWindow
    {
        [Header("Accessibility Options")]
        public Toggle colorblindFilterToggle;
        public Toggle largeTextToggle;
        public Toggle subtitlesToggle;
        public Toggle highContrastToggle;
        public Slider flashReductionSlider;

        [Header("Navigation Buttons")]
        public Button backButton;

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

            if (highContrastToggle != null)
            {
                highContrastToggle.isOn = m_settings.GetHighContrastUI();
                highContrastToggle.onValueChanged.AddListener(OnHighContrastToggleChanged);
            }
            else
            {
                Debug.LogError("[Settings] High Contrast Toggle is NULL! Assign it in the Inspector.");
            }

            if (flashReductionSlider != null)
            {
                flashReductionSlider.minValue = 0;
                flashReductionSlider.maxValue = 2;
                flashReductionSlider.wholeNumbers = true;
                flashReductionSlider.value = m_settings.GetFlashReductionLevel();
                flashReductionSlider.onValueChanged.AddListener(OnFlashReductionChanged);
            }
            else
            {
                Debug.LogError("[Settings] Flash Reduction Slider is NULL! Assign it in the Inspector.");
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
        
        private void OnHighContrastToggleChanged(bool isOn)
        {
            m_settings.SetHighContrastUI(isOn);
        }

        private void OnFlashReductionChanged(float value)
        {
            m_settings.SetFlashReductionLevel((int)value);
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
