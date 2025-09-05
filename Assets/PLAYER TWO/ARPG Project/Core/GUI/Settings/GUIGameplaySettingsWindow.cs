using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using AI_DDA.Assets.Scripts;
using PLAYERTWO.ARPGProject;
using Unity.Mathematics;

public class GUIGameplaySettingsWindow : GUIWindow
    {
        [Header("Gameplay Settings")]
        [Tooltip("References the camera shake toggle.")]
        public Toggle cameraShakeToggle;

        [Tooltip("References the auto-loot toggle.")]
        public Toggle autoLootToggle;

        [Tooltip("References the controller vibration slider.")]
        public Slider vibrationSlider;

        [Header("Navigation Buttons")]
        public Button backButton;

        protected GameSettings m_settings => GameSettings.instance;

        protected override void Start()
        {
            base.Start();

            InitializeCameraShake();
            InitializeAutoLoot();
            InitializeVibration();

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

        protected virtual void InitializeCameraShake()
        {
            if (cameraShakeToggle != null)
            {
                cameraShakeToggle.isOn = m_settings.GetCameraShake();
                cameraShakeToggle.onValueChanged.AddListener(m_settings.SetCameraShake);
            }
            else
            {
                Debug.LogError("[Settings] Camera Shake Toggle is NULL! Assign it in the Inspector.");
            }
        }

        protected virtual void InitializeAutoLoot()
        {
            if (autoLootToggle != null)
            {
                autoLootToggle.isOn = m_settings.GetAutoLoot();
                autoLootToggle.onValueChanged.AddListener(m_settings.SetAutoLoot);
            }
            else
            {
                Debug.LogError("[Settings] Auto-Loot Toggle is NULL! Assign it in the Inspector.");
            }
        }

        protected virtual void InitializeVibration()
        {
            if (vibrationSlider != null)
            {
                vibrationSlider.value = m_settings.GetControllerVibration();
                vibrationSlider.onValueChanged.AddListener(m_settings.SetControllerVibration);
            }
            else
            {
                Debug.LogError("[Settings] Controller Vibration Slider is NULL! Assign it in the Inspector.");
            }
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
