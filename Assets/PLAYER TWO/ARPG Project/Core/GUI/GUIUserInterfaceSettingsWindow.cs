using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using AI_DDA.Assets.Scripts;
using PLAYERTWO.ARPGProject;
using Unity.Mathematics;

public class GUIUserInterfaceSettingsWindow : GUIWindow
    {
        [Header("UI Settings")]

        [Header("Navigation Buttons")]
        public Button backButton;

        public Toggle difficultyToggle;
        public Toggle damageTextToggle;
        public TMP_Dropdown difficultyDropdown;

        protected GameSettings m_settings => GameSettings.instance;

        protected override void Start()
        {
            base.Start();


            if (backButton != null)
            {
                backButton.onClick.AddListener(BackButton);
            }
            else
            {
                Debug.LogError("[Settings] Back Button is NULL! Assign it in the Inspector.");
            }

            if (difficultyToggle != null)
            {
                difficultyToggle.isOn = m_settings.GetDisplayDifficulty();
                difficultyToggle.onValueChanged.AddListener(OnDifficultyToggleChanged);
            }
            else
            {
                Debug.LogError("[Settings] Difficulty Toggle is NULL! Assign it in the Inspector.");
            }

            if (damageTextToggle != null)
            {
                damageTextToggle.isOn = m_settings.GetDisplayDamageText();
                damageTextToggle.onValueChanged.AddListener(OnDamageTextToggleChanged);
            }
            else
            {
                Debug.LogError("[Settings] Damage Text Toggle is NULL! Assign it in the Inspector.");
            }

            if (difficultyDropdown != null)
            {
                difficultyDropdown.ClearOptions();
                difficultyDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "Text over enemies",
                    "Text over player"
                });

                difficultyDropdown.value = m_settings.GetDifficultyTextOption();
                difficultyDropdown.onValueChanged.AddListener(OnDifficultyDropdownChanged);
                UpdateDropdownVisibility();
            }
            else
            {
                Debug.LogError("[Settings] Difficulty Dropdown is NULL! Assign it in the Inspector.");
            }
        }

        protected virtual void OnDisable() => m_settings?.Save();

        protected virtual void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                m_settings.Save();
        }

        private void OnDifficultyToggleChanged(bool isOn)
        {
            m_settings.SetDisplayDifficulty(isOn);
            m_settings.ToggleDifficultyText();
            UpdateDropdownVisibility();
        }
        
        private void OnDamageTextToggleChanged(bool isOn)
        {
            m_settings.SetDisplayDamageText(isOn);
        }

        private void OnDifficultyDropdownChanged(int option)
        {
            m_settings.SetDifficultyTextOption(option);
            m_settings.UpdateDifficultyTextTarget();
        }

        private void UpdateDropdownVisibility()
        {
            if (difficultyDropdown != null)
                difficultyDropdown.gameObject.SetActive(difficultyToggle.isOn);
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