using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

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

        [Serializable]
        public class RebindButton
        {
            public string actionName;
            public int bindingIndex;
            public Button button;
        }

        [Header("Rebindable Actions")]
        public RebindButton[] rebindButtons;

        protected GameSettings m_settings => GameSettings.instance;

        protected override void Start()
        {
            base.Start();
            InitializeMovementSetting();
            InitializeRebindButtons();

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
        
        protected virtual void InitializeRebindButtons()
        {
            if (rebindButtons == null || m_settings == null || m_settings.inputActions == null)
                return;

            foreach (var rb in rebindButtons)
            {
                if (rb.button == null)
                    continue;

                UpdateBindingDisplay(rb);
                rb.button.onClick.AddListener(() => StartRebind(rb));
            }
        }

        protected virtual void UpdateBindingDisplay(RebindButton rb)
        {
            if (rb.button == null)
                return;

            var action = m_settings.inputActions?.FindAction(rb.actionName, true);
            var text = rb.button.GetComponentInChildren<Text>();
            if (action == null || text == null)
            {
                if (text != null)
                    text.text = "N/A";
                return;
            }

            var display = action.GetBindingDisplayString(rb.bindingIndex);
            text.text = display;
        }

        protected virtual void StartRebind(RebindButton rb)
        {
            var action = m_settings.inputActions?.FindAction(rb.actionName, true);
            if (action == null)
                return;

            rb.button.interactable = false;
            rb.button.GetComponentInChildren<Text>().text = "...";

            action.PerformInteractiveRebinding(rb.bindingIndex)
                .OnComplete(operation =>
                {
                    operation.Dispose();
                    rb.button.interactable = true;
                    UpdateBindingDisplay(rb);
                    m_settings.SaveBindings();
                })
                .OnCancel(operation =>
                {
                    operation.Dispose();
                    rb.button.interactable = true;
                    UpdateBindingDisplay(rb);
                })
                .Start();
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