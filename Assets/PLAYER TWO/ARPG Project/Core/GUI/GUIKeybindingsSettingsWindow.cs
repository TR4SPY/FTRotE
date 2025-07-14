using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Keybindings Settings Window")]
    public class GUIKeybindingsSettingsWindow : GUIWindow
    {
        [Header("Keybindings Settings")]
        [Tooltip("Slider that switches the movement control scheme.")]
        public Slider movementSettingSlider;

        [Header("Navigation Buttons")]
        public Button backButton;

        protected GameSettings m_settings => GameSettings.instance;

        protected override void Start()
        {
            base.Start();
            InitializeMovementSetting();

            if (backButton != null)
                backButton.onClick.AddListener(BackButton);
            else
                Debug.LogError("[Settings] Back Button is NULL! Assign it in the Inspector.");
        }

        protected virtual void InitializeMovementSetting()
        {
            if (movementSettingSlider != null && m_settings != null)
            {
                movementSettingSlider.minValue = 0;
                movementSettingSlider.maxValue = 1;
                movementSettingSlider.wholeNumbers = true;
                movementSettingSlider.value = m_settings.GetMovementSetting();
                movementSettingSlider.onValueChanged.AddListener(m_settings.SetMovementSetting);
            }
        }

        protected virtual void OnDisable() => m_settings?.Save();

        protected virtual void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                m_settings.Save();
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