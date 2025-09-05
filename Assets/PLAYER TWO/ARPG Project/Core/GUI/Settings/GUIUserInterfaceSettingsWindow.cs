using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using AI_DDA.Assets.Scripts;
using PLAYERTWO.ARPGProject;
using Unity.Mathematics;
using System.Collections.Generic;

public class GUIUserInterfaceSettingsWindow : GUIWindow
    {
        [Header("UI Settings")]

        [Header("Navigation Buttons")]
        public Button backButton;

        public Toggle difficultyToggle;
        public Toggle damageTextToggle;
        public Toggle overlayChatToggle;
        public Toggle showLatencyToggle;
        public Toggle tradeConfirmationsToggle;
        public Toggle blockProfanityToggle;
        public Toggle minimapRotationToggle;

        public Slider minimapSizeSlider;
        public Slider enemyHpBarSlider;
        public Slider buffTextSizeSlider;
        public Slider damageTextSizeSlider;
        public Slider cursorSizeSlider;

        public TMP_Dropdown difficultyDropdown;
        public TMP_Dropdown privacyStatusDropdown;
        public TMP_Dropdown partyInviteDropdown;
        public TMP_Dropdown damageNumberModeDropdown;
        public TMP_Dropdown itemCompareModeDropdown;

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

            if (overlayChatToggle != null)
            {
                overlayChatToggle.isOn = m_settings.GetOverlayChatAlwaysOnTop();
                overlayChatToggle.onValueChanged.AddListener(OnOverlayChatToggleChanged);
            }
            else
            {
                Debug.LogError("[Settings] Overlay Chat Toggle is NULL! Assign it in the Inspector.");
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
            
            if (enemyHpBarSlider != null)
            {
                enemyHpBarSlider.wholeNumbers = true;
                enemyHpBarSlider.minValue = 0;
                enemyHpBarSlider.maxValue = 2;
                enemyHpBarSlider.value = m_settings.GetEnemyHealthBarOption();
                enemyHpBarSlider.onValueChanged.AddListener(OnEnemyHpBarSliderChanged);
            }
            else
            {
                Debug.LogError("[Settings] Enemy HP Bar Slider is NULL! Assign it in the Inspector.");
            }

            if (showLatencyToggle != null)
            {
                showLatencyToggle.isOn = m_settings.GetShowLatency();
                showLatencyToggle.onValueChanged.AddListener(OnShowLatencyToggleChanged);
            }
            else
            {
                Debug.LogError("[Settings] Show Latency Toggle is NULL! Assign it in the Inspector.");
            }

            if (privacyStatusDropdown != null)
            {
                privacyStatusDropdown.ClearOptions();
                privacyStatusDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "Online",
                    "Busy",
                    "Offline"
                });
                privacyStatusDropdown.value = (int)m_settings.GetPrivacyStatus();
                privacyStatusDropdown.onValueChanged.AddListener(OnPrivacyStatusChanged);
            }
            else
            {
                Debug.LogError("[Settings] Privacy Status Dropdown is NULL! Assign it in the Inspector.");
            }

            if (partyInviteDropdown != null)
            {
                partyInviteDropdown.ClearOptions();
                partyInviteDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "Everyone",
                    "Friends Only",
                    "No One"
                });
                partyInviteDropdown.value = (int)m_settings.GetPartyInviteMode();
                partyInviteDropdown.onValueChanged.AddListener(OnPartyInviteModeChanged);
            }
            else
            {
                Debug.LogError("[Settings] Party Invite Dropdown is NULL! Assign it in the Inspector.");
            }

            if (tradeConfirmationsToggle != null)
            {
                tradeConfirmationsToggle.isOn = m_settings.GetTradeConfirmations();
                tradeConfirmationsToggle.onValueChanged.AddListener(OnTradeConfirmationsToggleChanged);
            }
            else
            {
                Debug.LogError("[Settings] Trade Confirmations Toggle is NULL! Assign it in the Inspector.");
            }

            if (blockProfanityToggle != null)
            {
                blockProfanityToggle.isOn = m_settings.GetBlockProfanity();
                blockProfanityToggle.onValueChanged.AddListener(OnBlockProfanityToggleChanged);
            }
            else
            {
                Debug.LogError("[Settings] Block Profanity Toggle is NULL! Assign it in the Inspector.");
            }
            if (minimapSizeSlider != null)
            {
                minimapSizeSlider.value = m_settings.GetMinimapSize();
                minimapSizeSlider.onValueChanged.AddListener(m_settings.SetMinimapSize);
            }

            if (minimapRotationToggle != null)
            {
                minimapRotationToggle.isOn = m_settings.GetMinimapRotation();
                minimapRotationToggle.onValueChanged.AddListener(m_settings.SetMinimapRotation);
            }

            if (damageNumberModeDropdown != null)
            {
                damageNumberModeDropdown.ClearOptions();
                damageNumberModeDropdown.AddOptions(new List<string> { "Normal", "Abbreviated" });
                damageNumberModeDropdown.value = m_settings.GetDamageNumberMode();
                damageNumberModeDropdown.onValueChanged.AddListener(m_settings.SetDamageNumberMode);
            }

            if (damageTextSizeSlider != null)
            {
                damageTextSizeSlider.wholeNumbers = true;
                damageTextSizeSlider.value = m_settings.GetDamageTextSize();
                damageTextSizeSlider.onValueChanged.AddListener(v => m_settings.SetDamageTextSize((int)v));
            }

            if (buffTextSizeSlider != null)
            {
                buffTextSizeSlider.wholeNumbers = true;
                buffTextSizeSlider.value = m_settings.GetBuffTextSize();
                buffTextSizeSlider.onValueChanged.AddListener(v => m_settings.SetBuffTextSize((int)v));
            }

            if (itemCompareModeDropdown != null)
            {
                itemCompareModeDropdown.ClearOptions();
                itemCompareModeDropdown.AddOptions(new List<string> { "Hold Shift", "Always", "Never" });
                itemCompareModeDropdown.value = m_settings.GetItemCompareMode();
                itemCompareModeDropdown.onValueChanged.AddListener(m_settings.SetItemCompareMode);
            }

            if (cursorSizeSlider != null)
            {
                cursorSizeSlider.value = m_settings.GetCursorSize();
                cursorSizeSlider.onValueChanged.AddListener(m_settings.SetCursorSize);
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
            UpdateDropdownVisibility();
        }

        private void OnEnemyHpBarSliderChanged(float option)
        {
            m_settings.SetEnemyHealthBarOption(option);
        }
        
        private void OnDamageTextToggleChanged(bool isOn)
        {
            m_settings.SetDisplayDamageText(isOn);
        }
        
        private void OnOverlayChatToggleChanged(bool isOn)
        {
            m_settings.SetOverlayChatAlwaysOnTop(isOn);
        }

        private void OnShowLatencyToggleChanged(bool isOn)
        {
            m_settings.SetShowLatency(isOn);
        }

        private void OnPrivacyStatusChanged(int option)
        {
            m_settings.SetPrivacyStatus((GameSettings.PrivacyStatus)option);
        }

        private void OnPartyInviteModeChanged(int option)
        {
            m_settings.SetPartyInviteMode((GameSettings.PartyInviteMode)option);
        }

        private void OnTradeConfirmationsToggleChanged(bool isOn)
        {
            m_settings.SetTradeConfirmations(isOn);
        }

        private void OnBlockProfanityToggleChanged(bool isOn)
        {
            m_settings.SetBlockProfanity(isOn);
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