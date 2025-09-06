using System;
using System.Collections;
using System.Collections.Generic;
using PLAYERTWO.ARPGProject;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

    public class GUIGraphicsSettingsWindow : GUIWindow
    {
        [Header("Graphics Settings")]
        [Tooltip("References the dropdown used to display the game's resolutions.")]
        public TMP_Dropdown resolution;

        [Tooltip("References the toggle that controls the full screen state.")]
        public Toggle fullScreen;

        [Tooltip("References the monitor selection dropdown.")]
        public TMP_Dropdown monitor;

        [Tooltip("References the max FPS dropdown.")]
        public TMP_Dropdown maxFps;

        [Tooltip("References the HDR toggle.")]
        public Toggle hdr;

        [Tooltip("References the toggle that controls the post-processing state.")]
        public Toggle postProcessing;

        [Header("Confirmation Dialog")]
        [Tooltip("Dialog used to confirm or rollback graphics changes.")]
        public GUISettingsRevertDialog settingsRevertDialog;

        [Header("Quality Settings")]
        public TMP_Dropdown qualityPreset;
        public TMP_Dropdown antiAliasing;
        public TMP_Dropdown shadows;
        public TMP_Dropdown textureQuality;
        public TMP_Dropdown anisotropic;
        public Toggle ambientOcclusion;
        public Toggle bloom;
        public Toggle depthOfField;
        public Toggle motionBlur;
        public Slider gamma;

        [Header("Navigation Buttons")]
        public Button backButton;
        public Button applyButton;
        public GameObject confirmDialog;
        public TMP_Text confirmCountdownText;
        public Button confirmButton;
        public Button revertButton;

        protected GameSettings m_settings => GameSettings.instance;

        private bool m_reverting;

        protected int m_pendingResolution;
        protected bool m_pendingFullScreen;
        protected int m_previousResolution;
        protected bool m_previousFullScreen;
        protected Coroutine m_revertCoroutine;

        protected override void Start()
        {
            base.Start();
            InitializeResolution();
            InitializeFullScreen();
            InitializeMonitor();
            InitializeMaxFps();
            InitializeHDR();
            InitializePostProcessing();
            InitializeQualityPreset();
            InitializeAntiAliasing();
            InitializeShadows();
            InitializeTextureQuality();
            InitializeAnisotropic();
            InitializeAmbientOcclusion();
            InitializeBloom();
            InitializeDepthOfField();
            InitializeMotionBlur();
            InitializeGamma();

            if (applyButton != null)
                applyButton.onClick.AddListener(ApplyDisplaySettings);

            if (confirmButton != null)
                confirmButton.onClick.AddListener(ConfirmResolution);

            if (revertButton != null)
                revertButton.onClick.AddListener(RevertResolution);

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
            m_pendingResolution = m_settings.GetResolution();
            resolution.value = m_pendingResolution;
            resolution.onValueChanged.AddListener(v => m_pendingResolution = v);
#if UNITY_WEBGL
            resolution.interactable = false;
#endif
        }

        protected virtual void InitializeFullScreen()
        {
            m_pendingFullScreen = m_settings.GetFullScreen();
            fullScreen.isOn = m_pendingFullScreen;
            fullScreen.onValueChanged.AddListener(v => m_pendingFullScreen = v);
        }

        protected virtual void InitializeMonitor()
        {
            if (monitor == null) return;
            monitor.ClearOptions();
            monitor.AddOptions(m_settings.GetMonitors());
            monitor.value = m_settings.GetMonitor();
            monitor.onValueChanged.AddListener(m_settings.SetMonitor);
        }

        protected virtual void InitializeMaxFps()
        {
            if (maxFps == null) return;
            var options = new List<string> { "30", "60", "120", "144", "240", "Unlimited" };
            maxFps.ClearOptions();
            maxFps.AddOptions(options);
            int current = m_settings.GetMaxFPS();
            int index = options.Count - 1;
            if (current > 0)
            {
                var str = current.ToString();
                index = options.IndexOf(str);
                if (index < 0) index = options.Count - 1;
            }
            maxFps.value = index;
            maxFps.onValueChanged.AddListener(i =>
            {
                if (i >= options.Count - 1)
                    m_settings.SetMaxFPS(-1);
                else
                    m_settings.SetMaxFPS(int.Parse(options[i]));
            });
        }

        protected virtual void InitializeHDR()
        {
            if (hdr == null) return;
            hdr.isOn = m_settings.GetHDR();
            hdr.onValueChanged.AddListener(m_settings.SetHDR);
        }

        protected virtual void InitializeQualityPreset()
        {
            if (qualityPreset == null) return;
            qualityPreset.ClearOptions();
            qualityPreset.AddOptions(m_settings.GetQualityPresets());
            qualityPreset.value = m_settings.GetQualityPreset();
            qualityPreset.onValueChanged.AddListener(m_settings.SetQualityPreset);
        }

        protected virtual void InitializeAntiAliasing()
        {
            if (antiAliasing == null) return;
            var options = new List<string> { "Disabled", "2x", "4x", "8x" };
            antiAliasing.ClearOptions();
            antiAliasing.AddOptions(options);
            int current = m_settings.GetAntiAliasing();
            int index = 0;
            switch (current)
            {
                case 2: index = 1; break;
                case 4: index = 2; break;
                case 8: index = 3; break;
            }
            antiAliasing.value = index;
            antiAliasing.onValueChanged.AddListener(i =>
            {
                int[] values = { 0, 2, 4, 8 };
                m_settings.SetAntiAliasing(values[i]);
            });
        }

        protected virtual void InitializeShadows()
        {
            if (shadows == null) return;
            shadows.ClearOptions();
            shadows.AddOptions(System.Enum.GetNames(typeof(ShadowQuality)).ToList());
            shadows.value = m_settings.GetShadows();
            shadows.onValueChanged.AddListener(m_settings.SetShadows);
        }

        protected virtual void InitializeTextureQuality()
        {
            if (textureQuality == null) return;
            var options = new List<string> { "Full Res", "Half Res", "Quarter Res", "Eighth Res" };
            textureQuality.ClearOptions();
            textureQuality.AddOptions(options);
            textureQuality.value = m_settings.GetTextureQuality();
            textureQuality.onValueChanged.AddListener(m_settings.SetTextureQuality);
        }

        protected virtual void InitializeAnisotropic()
        {
            if (anisotropic == null) return;
            anisotropic.ClearOptions();
            anisotropic.AddOptions(System.Enum.GetNames(typeof(AnisotropicFiltering)).ToList());
            anisotropic.value = m_settings.GetAnisotropicFiltering();
            anisotropic.onValueChanged.AddListener(m_settings.SetAnisotropicFiltering);
        }

       protected virtual void InitializeAmbientOcclusion()
        {
            if (ambientOcclusion == null) return;
            ambientOcclusion.isOn = m_settings.GetAmbientOcclusion();
            ambientOcclusion.onValueChanged.AddListener(m_settings.SetAmbientOcclusion);
        }

        protected virtual void InitializeBloom()
        {
            if (bloom == null) return;
            bloom.isOn = m_settings.GetBloom();
            bloom.onValueChanged.AddListener(m_settings.SetBloom);
        }

        protected virtual void InitializeDepthOfField()
        {
            if (depthOfField == null) return;
            depthOfField.isOn = m_settings.GetDepthOfField();
            depthOfField.onValueChanged.AddListener(m_settings.SetDepthOfField);
        }

        protected virtual void InitializeMotionBlur()
        {
            if (motionBlur == null) return;
            motionBlur.isOn = m_settings.GetMotionBlur();
            motionBlur.onValueChanged.AddListener(m_settings.SetMotionBlur);
        }

        protected virtual void InitializeGamma()
        {
            if (gamma == null) return;
            gamma.value = m_settings.GetGamma();
            gamma.onValueChanged.AddListener(m_settings.SetGamma);
        }

        protected virtual void ApplyDisplaySettings()
        {
            m_previousResolution = m_settings.GetResolution();
            m_previousFullScreen = m_settings.GetFullScreen();

            m_settings.SetResolution(m_pendingResolution);
            m_settings.SetFullScreen(m_pendingFullScreen);

            if (confirmDialog != null) confirmDialog.SetActive(true);
            if (m_revertCoroutine != null) StopCoroutine(m_revertCoroutine);
            m_revertCoroutine = StartCoroutine(RevertCountdown());
        }

        protected IEnumerator RevertCountdown()
        {
            float timer = 10f;
            while (timer > 0f)
            {
                if (confirmCountdownText != null)
                    confirmCountdownText.text = Mathf.CeilToInt(timer).ToString();
                timer -= Time.unscaledDeltaTime;
                yield return null;
            }
            RevertResolution();
        }

        public void ConfirmResolution()
        {
            if (m_revertCoroutine != null) StopCoroutine(m_revertCoroutine);
            if (confirmDialog != null) confirmDialog.SetActive(false);
            m_settings.Save();
        }

        public void RevertResolution()
        {
            if (m_revertCoroutine != null) StopCoroutine(m_revertCoroutine);
            m_settings.SetResolution(m_previousResolution);
            m_settings.SetFullScreen(m_previousFullScreen);
            if (confirmDialog != null) confirmDialog.SetActive(false);
        }

        protected virtual void InitializePostProcessing()
        {
            postProcessing.isOn = m_settings.GetPostProcessing();
            postProcessing.onValueChanged.AddListener(m_settings.SetPostProcessing);
        }

        protected virtual void OnResolutionChanged(int option)
        {
            if (m_reverting) return;

            int previousResolution = m_settings.GetResolution();
            bool previousFullScreen = m_settings.GetFullScreen();

            m_settings.SetResolution(option);

            ShowApplyRevert(() => m_settings.Save(), () =>
            {
                m_reverting = true;
                m_settings.SetResolution(previousResolution);
                resolution.SetValueWithoutNotify(previousResolution);
                m_settings.SetFullScreen(previousFullScreen);
                fullScreen.SetIsOnWithoutNotify(previousFullScreen);
                m_reverting = false;
            });
        }

        protected virtual void OnFullScreenChanged(bool value)
        {
            if (m_reverting) return;

            bool previousFullScreen = m_settings.GetFullScreen();
            int previousResolution = m_settings.GetResolution();

            m_settings.SetFullScreen(value);

            ShowApplyRevert(() => m_settings.Save(), () =>
            {
                m_reverting = true;
                m_settings.SetFullScreen(previousFullScreen);
                fullScreen.SetIsOnWithoutNotify(previousFullScreen);
                m_settings.SetResolution(previousResolution);
                resolution.SetValueWithoutNotify(previousResolution);
                m_reverting = false;
            });
        }

        protected virtual void ShowApplyRevert(Action onApply, Action onRevert)
        {
            if (settingsRevertDialog != null)
                settingsRevertDialog.Show(onApply, onRevert);
            else
                onApply?.Invoke();
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
