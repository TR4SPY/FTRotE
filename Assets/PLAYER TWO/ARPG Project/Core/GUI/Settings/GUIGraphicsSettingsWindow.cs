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

        [Tooltip("References the dropdown that controls the screen mode.")]
        public TMP_Dropdown screenModeDropdown;

        [Tooltip("References the toggle that controls VSync.")]
        public Toggle vSyncToggle;

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
        protected FullScreenMode m_pendingScreenMode;
        protected bool m_pendingVSync;
        protected int m_pendingMonitor;
        protected int m_pendingMaxFPS;
        protected bool m_pendingHDR;
        protected bool m_pendingPostProcessing;
        protected int m_pendingQualityPreset;
        protected int m_pendingAntiAliasing;
        protected int m_pendingShadows;
        protected int m_pendingTextureQuality;
        protected int m_pendingAnisotropic;
        protected bool m_pendingAmbientOcclusion;
        protected bool m_pendingBloom;
        protected bool m_pendingDepthOfField;
        protected bool m_pendingMotionBlur;
        protected float m_pendingGamma;

        protected int m_previousResolution;
        protected FullScreenMode m_previousScreenMode;
        protected bool m_previousVSync;
        protected int m_previousMonitor;
        protected int m_previousMaxFPS;
        protected bool m_previousHDR;
        protected bool m_previousPostProcessing;
        protected int m_previousQualityPreset;
        protected int m_previousAntiAliasing;
        protected int m_previousShadows;
        protected int m_previousTextureQuality;
        protected int m_previousAnisotropic;
        protected bool m_previousAmbientOcclusion;
        protected bool m_previousBloom;
        protected bool m_previousDepthOfField;
        protected bool m_previousMotionBlur;
        protected float m_previousGamma;

        protected Coroutine m_revertCoroutine;

        protected override void Start()
        {
            base.Start();
            InitializeResolution();
            InitializeVSync();
            InitializeScreenMode();
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

        protected virtual void InitializeScreenMode()
        {
            if (screenModeDropdown == null) return;

            var options = new List<string> { "Full Screen", "Windowed", "Borderless" };
            screenModeDropdown.ClearOptions();
            screenModeDropdown.AddOptions(options);

            m_pendingScreenMode = m_settings.GetScreenMode();
            screenModeDropdown.value = ModeToIndex(m_pendingScreenMode);
            screenModeDropdown.onValueChanged.AddListener(v => m_pendingScreenMode = IndexToMode(v));
        }

        protected virtual void InitializeVSync()
        {
            if (vSyncToggle == null) return;
            m_pendingVSync = m_settings.GetVSync();
            vSyncToggle.isOn = m_pendingVSync;
            vSyncToggle.onValueChanged.AddListener(v => m_pendingVSync = v);
        }

        protected int ModeToIndex(FullScreenMode mode)
        {
            switch (mode)
            {
                case FullScreenMode.ExclusiveFullScreen:
                    return 0;
                case FullScreenMode.FullScreenWindow:
                    return 2;
                case FullScreenMode.Windowed:
                case FullScreenMode.MaximizedWindow:
                    return 1;
                default:
                    return 0;
            }
        }

        protected FullScreenMode IndexToMode(int index)
        {
            switch (index)
            {
                case 0:
                    return FullScreenMode.ExclusiveFullScreen;
                case 1:
                    return FullScreenMode.Windowed;
                case 2:
                    return FullScreenMode.FullScreenWindow;
                default:
                    return FullScreenMode.Windowed;
            }
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
            m_previousScreenMode = m_settings.GetScreenMode();
            m_previousVSync = m_settings.GetVSync();
            m_previousMonitor = m_settings.GetMonitor();
            m_previousMaxFPS = m_settings.GetMaxFPS();
            m_previousHDR = m_settings.GetHDR();
            m_previousPostProcessing = m_settings.GetPostProcessing();
            m_previousQualityPreset = m_settings.GetQualityPreset();
            m_previousAntiAliasing = m_settings.GetAntiAliasing();
            m_previousShadows = m_settings.GetShadows();
            m_previousTextureQuality = m_settings.GetTextureQuality();
            m_previousAnisotropic = m_settings.GetAnisotropicFiltering();
            m_previousAmbientOcclusion = m_settings.GetAmbientOcclusion();
            m_previousBloom = m_settings.GetBloom();
            m_previousDepthOfField = m_settings.GetDepthOfField();
            m_previousMotionBlur = m_settings.GetMotionBlur();
            m_previousGamma = m_settings.GetGamma();

            m_settings.SetResolution(m_pendingResolution);
            m_settings.SetScreenMode(m_pendingScreenMode);
            m_settings.SetVSync(m_pendingVSync);
            m_settings.SetMonitor(m_pendingMonitor);
            m_settings.SetMaxFPS(m_pendingMaxFPS);
            m_settings.SetHDR(m_pendingHDR);
            m_settings.SetPostProcessing(m_pendingPostProcessing);
            m_settings.SetQualityPreset(m_pendingQualityPreset);
            int[] aaValues = { 0, 2, 4, 8 };
            m_settings.SetAntiAliasing(aaValues[m_pendingAntiAliasing]);
            m_settings.SetShadows(m_pendingShadows);
            m_settings.SetTextureQuality(m_pendingTextureQuality);
            m_settings.SetAnisotropicFiltering(m_pendingAnisotropic);
            m_settings.SetAmbientOcclusion(m_pendingAmbientOcclusion);
            m_settings.SetBloom(m_pendingBloom);
            m_settings.SetDepthOfField(m_pendingDepthOfField);
            m_settings.SetMotionBlur(m_pendingMotionBlur);
            m_settings.SetGamma(m_pendingGamma);

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
            m_settings.SetScreenMode(m_previousScreenMode);
            m_settings.SetVSync(m_previousVSync);
            m_settings.SetMonitor(m_previousMonitor);
            m_settings.SetMaxFPS(m_previousMaxFPS);
            m_settings.SetHDR(m_previousHDR);
            m_settings.SetPostProcessing(m_previousPostProcessing);
            m_settings.SetQualityPreset(m_previousQualityPreset);
            m_settings.SetAntiAliasing(m_previousAntiAliasing);
            m_settings.SetShadows(m_previousShadows);
            m_settings.SetTextureQuality(m_previousTextureQuality);
            m_settings.SetAnisotropicFiltering(m_previousAnisotropic);
            m_settings.SetAmbientOcclusion(m_previousAmbientOcclusion);
            m_settings.SetBloom(m_previousBloom);
            m_settings.SetDepthOfField(m_previousDepthOfField);
            m_settings.SetMotionBlur(m_previousMotionBlur);
            m_settings.SetGamma(m_previousGamma);

            resolution.SetValueWithoutNotify(m_previousResolution);
            m_pendingResolution = m_previousResolution;
            if (screenModeDropdown != null)
            {
                screenModeDropdown.SetValueWithoutNotify(ModeToIndex(m_previousScreenMode));
                m_pendingScreenMode = m_previousScreenMode;
            }
            if (vSyncToggle != null)
            {
                vSyncToggle.SetIsOnWithoutNotify(m_previousVSync);
                m_pendingVSync = m_previousVSync;
            }
            if (monitor != null)
            {
                monitor.SetValueWithoutNotify(m_previousMonitor);
                m_pendingMonitor = m_previousMonitor;
            }
            if (maxFps != null)
            {
                var options = new List<string> { "30", "60", "120", "144", "240", "Unlimited" };
                int index = options.Count - 1;
                if (m_previousMaxFPS > 0)
                {
                    var str = m_previousMaxFPS.ToString();
                    index = options.IndexOf(str);
                    if (index < 0) index = options.Count - 1;
                }
                maxFps.SetValueWithoutNotify(index);
                m_pendingMaxFPS = m_previousMaxFPS;
            }
            if (hdr != null)
            {
                hdr.SetIsOnWithoutNotify(m_previousHDR);
                m_pendingHDR = m_previousHDR;
            }
            if (postProcessing != null)
            {
                postProcessing.SetIsOnWithoutNotify(m_previousPostProcessing);
                m_pendingPostProcessing = m_previousPostProcessing;
            }
            if (qualityPreset != null)
            {
                qualityPreset.SetValueWithoutNotify(m_previousQualityPreset);
                m_pendingQualityPreset = m_previousQualityPreset;
            }
            if (antiAliasing != null)
            {
                int index = 0;
                switch (m_previousAntiAliasing)
                {
                    case 2: index = 1; break;
                    case 4: index = 2; break;
                    case 8: index = 3; break;
                }
                antiAliasing.SetValueWithoutNotify(index);
                m_pendingAntiAliasing = index;
            }
            if (shadows != null)
            {
                shadows.SetValueWithoutNotify(m_previousShadows);
                m_pendingShadows = m_previousShadows;
            }
            if (textureQuality != null)
            {
                textureQuality.SetValueWithoutNotify(m_previousTextureQuality);
                m_pendingTextureQuality = m_previousTextureQuality;
            }
            if (anisotropic != null)
            {
                anisotropic.SetValueWithoutNotify(m_previousAnisotropic);
                m_pendingAnisotropic = m_previousAnisotropic;
            }
            if (ambientOcclusion != null)
            {
                ambientOcclusion.SetIsOnWithoutNotify(m_previousAmbientOcclusion);
                m_pendingAmbientOcclusion = m_previousAmbientOcclusion;
            }
            if (bloom != null)
            {
                bloom.SetIsOnWithoutNotify(m_previousBloom);
                m_pendingBloom = m_previousBloom;
            }
            if (depthOfField != null)
            {
                depthOfField.SetIsOnWithoutNotify(m_previousDepthOfField);
                m_pendingDepthOfField = m_previousDepthOfField;
            }
            if (motionBlur != null)
            {
                motionBlur.SetIsOnWithoutNotify(m_previousMotionBlur);
                m_pendingMotionBlur = m_previousMotionBlur;
            }
            if (gamma != null)
            {
                gamma.SetValueWithoutNotify(m_previousGamma);
                m_pendingGamma = m_previousGamma;
            }

            if (confirmDialog != null) confirmDialog.SetActive(false);
        }


        protected virtual void InitializePostProcessing()
        {
            m_pendingPostProcessing = m_settings.GetPostProcessing();
            postProcessing.isOn = m_pendingPostProcessing;
            postProcessing.onValueChanged.AddListener(v => m_pendingPostProcessing = v);
        }

        protected virtual void OnResolutionChanged(int option)
        {
            if (m_reverting) return;

            int previousResolution = m_settings.GetResolution();
            var previousScreenMode = m_settings.GetScreenMode();

            m_settings.SetResolution(option);

            ShowApplyRevert(() => m_settings.Save(), () =>
            {
                m_reverting = true;
                m_settings.SetResolution(previousResolution);
                resolution.SetValueWithoutNotify(previousResolution);
                m_settings.SetScreenMode(previousScreenMode);
                screenModeDropdown.SetValueWithoutNotify(ModeToIndex(previousScreenMode));
                m_reverting = false;
            });
        }

        protected virtual void OnScreenModeChanged(int value)
        {
            if (m_reverting) return;

            var previousScreenMode = m_settings.GetScreenMode();
            int previousResolution = m_settings.GetResolution();

            m_settings.SetScreenMode(IndexToMode(value));

            ShowApplyRevert(() => m_settings.Save(), () =>
            {
                m_reverting = true;
                m_settings.SetScreenMode(previousScreenMode);
                screenModeDropdown.SetValueWithoutNotify(ModeToIndex(previousScreenMode));
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
