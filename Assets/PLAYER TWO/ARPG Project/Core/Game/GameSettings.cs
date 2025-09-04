using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using AI_DDA.Assets.Scripts;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Game/Game Settings")]
    public class GameSettings : Singleton<GameSettings>
    {
        public enum PrivacyStatus
        {
            Online,
            Busy,
            Offline
        }

        public enum PartyInviteMode
        {
            Everyone,
            FriendsOnly,
            NoOne
        }

        public enum ResolutionOption
        {
            Lowest,
            Low,
            Medium,
            High,
            Maximum
        }

        public enum DamageNumberMode
        {
            Normal,
            Abbreviated
        }

        public enum ItemCompareMode
        {
            HoldShift,
            Always,
            Never
        }

        protected int m_currentResolution;
        protected int m_currentMovementSetting;
        protected int m_currentMonitor;
        protected int m_currentMaxFps;
        protected int m_currentQualityPreset;
        protected int m_currentAntiAliasing;
        protected int m_currentShadows;
        protected int m_currentTextureQuality;
        protected int m_currentAnisotropicFiltering;
        protected int m_currentOutputDevice;
        protected int m_currentFlashReductionLevel;

        protected bool m_currentFullScreen;
        protected bool m_currentPostProcessing;
        protected bool m_currentCameraShake;
        protected bool m_currentAutoLoot;
        protected bool m_currentHdr;
        protected bool m_currentAmbientOcclusion;
        protected bool m_currentBloom;
        protected bool m_currentDepthOfField;
        protected bool m_currentMotionBlur;
        protected bool m_currentMuteOnFocusLoss;
        protected bool m_currentHighContrastUI;

        protected float m_currentMusicVolume;
        protected float m_currentEffectsVolume;
        protected float m_currentUiEffectsVolume;
        protected float m_currentControllerVibration;
        protected float m_currentGamma;

        protected string m_currentLanguage;

        public InputActionAsset inputActions;

        protected const string k_resolutionKey = "settings/resolution";
        protected const string k_fullScreenKey = "settings/fullScreen";
        protected const string k_postProcessingKey = "settings/postProcessing";
        protected const string k_musicVolumeKey = "settings/musicVolume";
        protected const string k_effectsVolumeKey = "settings/effectsVolume";
        protected const string k_uiEffectsVolumeKey = "settings/uiEffectsVolume";
        protected const string k_movementSettingKey = "settings/movementSetting";
        protected const string k_overlayChatOnTopKey = "settings/overlayChatOnTop";
        protected const string k_enemyHealthBarOptionKey = "settings/enemyHealthBarOption";
        protected const string k_bindingsKey = "settings/bindings";
        protected const string k_cameraShakeKey = "settings/cameraShake";
        protected const string k_autoLootKey = "settings/autoLoot";
        protected const string k_controllerVibrationKey = "settings/controllerVibration";
        protected const string k_languageKey = "Language";
        protected const string k_colorblindKey = "settings/colorblind";
        protected const string k_largeTextKey = "settings/largeText";
        protected const string k_subtitlesKey = "settings/subtitles";
        protected const string k_showLatencyKey = "settings/showLatency";
        protected const string k_privacyStatusKey = "settings/privacyStatus";
        protected const string k_partyInviteModeKey = "settings/partyInviteMode";
        protected const string k_tradeConfirmationsKey = "settings/tradeConfirmations";
        protected const string k_blockProfanityKey = "settings/blockProfanity";
        protected const string k_minimapSizeKey = "settings/minimapSize";
        protected const string k_minimapRotationKey = "settings/minimapRotation";
        protected const string k_damageNumberModeKey = "settings/damageNumberMode";
        protected const string k_damageTextSizeKey = "settings/damageTextSize";
        protected const string k_buffTextSizeKey = "settings/buffTextSize";
        protected const string k_itemCompareModeKey = "settings/itemCompareMode";
        protected const string k_cursorSizeKey = "settings/cursorSize";
        protected const string k_monitorKey = "settings/monitor";
        protected const string k_maxFpsKey = "settings/maxFps";
        protected const string k_hdrKey = "settings/hdr";
        protected const string k_qualityPresetKey = "settings/qualityPreset";
        protected const string k_aaKey = "settings/aa";
        protected const string k_shadowsKey = "settings/shadows";
        protected const string k_textureQualityKey = "settings/textureQuality";
        protected const string k_anisotropicKey = "settings/anisotropic";
        protected const string k_aoKey = "settings/ao";
        protected const string k_bloomKey = "settings/bloom";
        protected const string k_dofKey = "settings/dof";
        protected const string k_motionBlurKey = "settings/motionBlur";
        protected const string k_gammaKey = "settings/gamma";
        protected const string k_outputDeviceKey = "settings/outputDevice";
        protected const string k_muteOnFocusLossKey = "settings/muteOnFocusLoss";
        protected const string k_highContrastKey = "settings/highContrastUI";
        protected const string k_flashReductionKey = "settings/flashReduction";

        public bool GetDisplayDifficulty() => PlayerPrefs.GetInt("DisplayDifficulty", 1) == 1;
        public bool GetDisplayDamageText() => PlayerPrefs.GetInt("DisplayDamageText", 1) == 1;
        public bool GetOverlayChatAlwaysOnTop() => PlayerPrefs.GetInt(k_overlayChatOnTopKey, 1) == 1;
        public bool GetColorblindMode() => PlayerPrefs.GetInt(k_colorblindKey, 0) == 1;
        public bool GetLargeText() => PlayerPrefs.GetInt(k_largeTextKey, 0) == 1;
        public bool GetSubtitlesEnabled() => PlayerPrefs.GetInt(k_subtitlesKey, 1) == 1;
        public bool GetMinimapRotation() => PlayerPrefs.GetInt(k_minimapRotationKey, 1) == 1;

        public int GetDifficultyTextOption() => PlayerPrefs.GetInt("DifficultyTextOption", 0);
        public int GetDamageNumberMode() => PlayerPrefs.GetInt(k_damageNumberModeKey, 0);
        public int GetDamageTextSize() => PlayerPrefs.GetInt(k_damageTextSizeKey, 14);
        public int GetBuffTextSize() => PlayerPrefs.GetInt(k_buffTextSizeKey, 14);
        public int GetItemCompareMode() => PlayerPrefs.GetInt(k_itemCompareModeKey, 0);

        public float GetMinimapSize() => PlayerPrefs.GetFloat(k_minimapSizeKey, 1f);
        public float GetCursorSize() => PlayerPrefs.GetFloat(k_cursorSizeKey, 1f);

        protected GameAudio m_audio => GameAudio.instance;

        [Tooltip("The Volume component controlling post-processing effects.")]
        public Volume postProcessingVolume;

        [Header("Accessibility Audio Cues")]
        [Tooltip("Audio played when a rare item drops.")]
        public AudioClip rareDropAudioCue;

        [Tooltip("Audio played when a jewel item drops.")]
        public AudioClip jewelDropAudioCue;

        private const string SaveLogsKey = "SaveLogs";

        public bool GetSaveLogs()
        {
            return PlayerPrefs.GetInt(SaveLogsKey, 1) == 1; // Turned ON in default (1)
        }

        public void SetSaveLogs(bool isEnabled)
        {
            PlayerPrefs.SetInt(SaveLogsKey, isEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetOverlayChatAlwaysOnTop(bool value)
        {
            PlayerPrefs.SetInt(k_overlayChatOnTopKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public bool GetShowLatency() => PlayerPrefs.GetInt(k_showLatencyKey, 0) == 1;

        public void SetShowLatency(bool value)
        {
            PlayerPrefs.SetInt(k_showLatencyKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public PrivacyStatus GetPrivacyStatus() => (PrivacyStatus)PlayerPrefs.GetInt(k_privacyStatusKey, 0);

        public void SetPrivacyStatus(PrivacyStatus status)
        {
            PlayerPrefs.SetInt(k_privacyStatusKey, (int)status);
            PlayerPrefs.Save();
        }

        public PartyInviteMode GetPartyInviteMode() => (PartyInviteMode)PlayerPrefs.GetInt(k_partyInviteModeKey, 0);

        public void SetPartyInviteMode(PartyInviteMode mode)
        {
            PlayerPrefs.SetInt(k_partyInviteModeKey, (int)mode);
            PlayerPrefs.Save();
        }

        public bool GetTradeConfirmations() => PlayerPrefs.GetInt(k_tradeConfirmationsKey, 1) == 1;

        public void SetTradeConfirmations(bool value)
        {
            PlayerPrefs.SetInt(k_tradeConfirmationsKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public bool GetBlockProfanity() => PlayerPrefs.GetInt(k_blockProfanityKey, 0) == 1;

        public void SetBlockProfanity(bool value)
        {
            PlayerPrefs.SetInt(k_blockProfanityKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public bool CanReceiveInvite(bool isFriend)
        {
            var mode = GetPartyInviteMode();
            return mode switch
            {
                PartyInviteMode.Everyone => true,
                PartyInviteMode.FriendsOnly => isFriend,
                _ => false,
            };
        }

        protected PostProcessToggler m_postProcess
        {
            get
            {
                var postProcess = PostProcessToggler.instance;
                if (postProcess == null)
                {
                    Debug.LogWarning("PostProcessToggler instance not found. Post-processing will not be applied.");
                }
                return postProcess;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            Load();
        }

        protected virtual void OnDisable() => Save();

        protected virtual void Update()
        {
            if (m_currentMuteOnFocusLoss)
                AudioListener.pause = !Application.isFocused;
        }

        public virtual int GetResolution() => m_currentResolution;

        public virtual int GetLanguage() =>
            LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);

        public virtual List<string> GetResolutions()
        {
#if UNITY_STANDALONE
            return GetScreenResolutions();
#else
            return GetRenderingResolutions();
#endif
        }

        protected virtual List<string> GetScreenResolutions() => Screen.resolutions.Select(resolution => resolution.ToString()).ToList();
        protected virtual List<string> GetRenderingResolutions() => System.Enum.GetNames(typeof(ResolutionOption)).ToList();

        public virtual List<string> GetLanguages() => LocalizationSettings.AvailableLocales.Locales.Select(locale => locale.LocaleName).ToList();
        public virtual List<string> GetMonitors() => Display.displays.Select((d, i) => $"Monitor {i + 1}").ToList();
        public virtual List<string> GetQualityPresets() => QualitySettings.names.ToList();
        
        public virtual int GetMonitor() => m_currentMonitor;
        public virtual int GetQualityPreset() => m_currentQualityPreset;
        public virtual int GetAntiAliasing() => m_currentAntiAliasing;
        public virtual int GetShadows() => m_currentShadows;
        public virtual int GetTextureQuality() => m_currentTextureQuality;
        public virtual int GetAnisotropicFiltering() => m_currentAnisotropicFiltering;
        public virtual int GetMaxFPS() => m_currentMaxFps;

        public virtual bool GetHDR() => m_currentHdr; 
        public virtual bool GetAmbientOcclusion() => m_currentAmbientOcclusion;
        public virtual bool GetBloom() => m_currentBloom;
        public virtual bool GetDepthOfField() => m_currentDepthOfField;
        public virtual bool GetMotionBlur() => m_currentMotionBlur;
        public virtual bool GetFullScreen() => Screen.fullScreen;
        public virtual bool GetPostProcessing() => m_postProcess?.value ?? false;
        public virtual bool GetCameraShake() => m_currentCameraShake;
        public virtual bool GetAutoLoot() => m_currentAutoLoot;

        public virtual float GetGamma() => m_currentGamma;
        public virtual float GetMusicVolume() => m_audio.GetMusicVolume();
        public virtual float GetEffectsVolume() => m_audio.GetEffectsVolume();
        public virtual float GetUIEffectsVolume() => m_audio.GetUIEffectsVolume();
        public virtual float GetControllerVibration() => m_currentControllerVibration;
        public virtual int GetOutputDevice() => m_currentOutputDevice;

        public virtual List<string> GetOutputDevices()
        {
            return new List<string> { "Default" };
        }

        public virtual bool GetMuteOnFocusLoss() => m_currentMuteOnFocusLoss;

        public virtual void SetOutputDevice(int index)
        {
            m_currentOutputDevice = index;
        }

        public virtual void SetMuteOnFocusLoss(bool value)
        {
            m_currentMuteOnFocusLoss = value;
            if (value)
                AudioListener.pause = !Application.isFocused;
            else
                AudioListener.pause = false;
        }

        public void SetResolution(int option)
        {
#if UNITY_STANDALONE
            SetScreenResolution(option);
#else
            SetRenderingResolution(option);
#endif
        }
        
        public void SetDifficultyTextOption(int option)
        {
            PlayerPrefs.SetInt("DifficultyTextOption", option);
            PlayerPrefs.Save();
        }

        public void SetDisplayDifficulty(bool value)
        {
            PlayerPrefs.SetInt("DisplayDifficulty", value ? 1 : 0);
            PlayerPrefs.Save();

            DifficultyText.ToggleAll(value);

            Debug.Log($"[GameSettings] Display Difficulty set to: {value}");
        }

        public void SetLanguage(int option)
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            if (option >= 0 && option < locales.Count)
            {
                SetLanguage(locales[option].Identifier.Code);
            }
        }

        public void SetLanguage(string code)
        {
            var locale = LocalizationSettings.AvailableLocales.GetLocale(code);
            if (locale != null)
            {
                LocalizationSettings.SelectedLocale = locale;
                m_currentLanguage = code;
                PlayerPrefs.SetString(k_languageKey, code);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogWarning($"[GameSettings] Locale '{code}' not found.");
            }
        }

        public void ToggleDifficultyText()
        {
            bool isEnabled = GetDisplayDifficulty();
            DifficultyText.ToggleAll(isEnabled);
        }

        public void UpdateDifficultyTextTarget()
        {
            int option = GetDifficultyTextOption();

            List<DifficultyText> textsToRemove = new List<DifficultyText>(DifficultyText.activeTexts);
            foreach (var text in textsToRemove)
            {
                if (text == null) continue;

                bool shouldBeRemoved = (text.gameObject.CompareTag("Entity/Enemy") && option == 1) || 
                                    (text.gameObject.CompareTag("Entity/Player") && option == 0);

                if (shouldBeRemoved)
                {
                    Destroy(text.gameObject);
                }
            }

            Debug.Log($"[AI-DDA] DifficultyText updated to option: {option}");
        }
        public void SetDisplayDamageText(bool value)
        {
            PlayerPrefs.SetInt("DisplayDamageText", value ? 1 : 0);
            PlayerPrefs.Save();
            
            DamageText.ToggleAll(value);
        }

        public void SetMinimapSize(float value)
        {
            PlayerPrefs.SetFloat(k_minimapSizeKey, value);
            PlayerPrefs.Save();
            MinimapHUD.instance?.Rescale(value);
        }

        public void SetMinimapRotation(bool value)
        {
            PlayerPrefs.SetInt(k_minimapRotationKey, value ? 1 : 0);
            PlayerPrefs.Save();
            if (MinimapHUD.instance != null)
            {
                MinimapHUD.instance.rotateWithTarget = value;
            }
        }

        public void SetDamageNumberMode(int mode)
        {
            PlayerPrefs.SetInt(k_damageNumberModeKey, mode);
            PlayerPrefs.Save();
        }

        public void SetDamageTextSize(int size)
        {
            PlayerPrefs.SetInt(k_damageTextSizeKey, size);
            PlayerPrefs.Save();
            DamageText.UpdateAllFontSizes(size);
        }

        public void SetBuffTextSize(int size)
        {
            PlayerPrefs.SetInt(k_buffTextSizeKey, size);
            PlayerPrefs.Save();

            foreach (var slot in Object.FindObjectsByType<GUIBuffSlot>(FindObjectsSortMode.None))
            {
                if (slot.keyText)
                    slot.keyText.fontSize = size;
            }
        }

        public void SetItemCompareMode(int mode)
        {
            PlayerPrefs.SetInt(k_itemCompareModeKey, mode);
            PlayerPrefs.Save();
        }

        public void SetCursorSize(float value)
        {
            PlayerPrefs.SetFloat(k_cursorSizeKey, value);
            PlayerPrefs.Save();
        }

        public void SetColorblindMode(bool value)
        {
            PlayerPrefs.SetInt(k_colorblindKey, value ? 1 : 0);
            PlayerPrefs.Save();

            ApplyColorblindMode(value);
        }

        public void SetLargeText(bool value)
        {
            PlayerPrefs.SetInt(k_largeTextKey, value ? 1 : 0);
            PlayerPrefs.Save();

            ApplyTextScale(value ? 1.25f : 1f);
        }

        public void SetSubtitlesEnabled(bool value)
        {
            PlayerPrefs.SetInt(k_subtitlesKey, value ? 1 : 0);
            PlayerPrefs.Save();

            ApplySubtitles(value);
        }

        public bool GetHighContrastUI() => m_currentHighContrastUI;

        public void SetHighContrastUI(bool value)
        {
            m_currentHighContrastUI = value;
            PlayerPrefs.SetInt(k_highContrastKey, value ? 1 : 0);
            PlayerPrefs.Save();
            ApplyHighContrastUI(value);
        }

        public int GetFlashReductionLevel() => m_currentFlashReductionLevel;

        public float GetFlashReductionMultiplier()
        {
            return m_currentFlashReductionLevel switch
            {
                1 => 0.5f,
                2 => 0.25f,
                _ => 1f
            };
        }

        public void SetFlashReductionLevel(int value)
        {
            m_currentFlashReductionLevel = Mathf.Clamp(value, 0, 2);
            PlayerPrefs.SetInt(k_flashReductionKey, m_currentFlashReductionLevel);
            PlayerPrefs.Save();
            ApplyFlashReductionLevel(m_currentFlashReductionLevel);
        }

        public void ApplyHighContrastUI(bool enabled)
        {
            if (enabled)
                Shader.EnableKeyword("ACCESSIBILITY_HIGH_CONTRAST");
            else
                Shader.DisableKeyword("ACCESSIBILITY_HIGH_CONTRAST");
        }

        public void ApplyFlashReductionLevel(int level)
        {
            Shader.SetGlobalFloat("_FlashReduction", GetFlashReductionMultiplier());
        }

        public void ApplyColorblindMode(bool enabled)
        {
            if (enabled)
                Shader.EnableKeyword("ACCESSIBILITY_COLORBLIND");
            else
                Shader.DisableKeyword("ACCESSIBILITY_COLORBLIND");
        }

        public void ApplyTextScale(float scale)
        {
            foreach (var scaler in Resources.FindObjectsOfTypeAll<CanvasScaler>())
                scaler.scaleFactor = scale;
        }

        public void ApplySubtitles(bool enabled)
        {
            Debug.Log($"[GameSettings] Subtitles {(enabled ? "enabled" : "disabled")}");
        }

        public int GetEnemyHealthBarOption() => PlayerPrefs.GetInt(k_enemyHealthBarOptionKey, 2);

        public void SetEnemyHealthBarOption(float value) => SetEnemyHealthBarOption((int)value);

        public void SetEnemyHealthBarOption(int value)
        {
            PlayerPrefs.SetInt(k_enemyHealthBarOptionKey, value);
            PlayerPrefs.Save();
            UpdateEnemyHealthBars();
        }

        public void UpdateEnemyHealthBars()
        {
            var bars = Object.FindObjectsByType<EntityHealthBar>(FindObjectsSortMode.None);

            foreach (var bar in bars)
            {
                bar?.ApplySettings();
            }
        }

        protected virtual void SetScreenResolution(int option)
        {
            var resolution = Screen.resolutions[option];
            Screen.SetResolution(resolution.width, resolution.height,
                Screen.fullScreenMode, resolution.refreshRateRatio);
            m_currentResolution = option;
        }

        protected virtual void SetRenderingResolution(int option)
        {
            var resolution = GetResolutionScale((ResolutionOption)option);
            Screen.SetResolution(resolution.x, resolution.y, Screen.fullScreen);
            m_currentResolution = option;
        }

        public virtual void SetFullScreen(bool value)
        {
            Screen.fullScreen = value;
            m_currentFullScreen = value;
        }

/*
        public virtual void SetPostProcessing(bool value)
        {
            if (m_postProcess == null)
            {
                Debug.LogWarning("PostProcessToggler instance is null. Cannot set post-processing.");
                return;
            }
            m_postProcess.SetValue(value);
            m_currentPostProcessing = value;
        }
*/

        public virtual void SetPostProcessing(bool value)
        {
            if (m_postProcess != null)
            {
                m_postProcess.SetValue(value);
            }
            else
            {
                Debug.LogWarning("PostProcessToggler instance is null. Cannot set post-processing.");
            }

            if (postProcessingVolume != null)
            {
                postProcessingVolume.enabled = value;
            }
            else
            {
                Debug.LogWarning("Post-processing Volume is null. Cannot set its state.");
            }

            m_currentPostProcessing = value;
        }
        
        public virtual void SetMusicVolume(float value)
        {
            m_audio.SetMusicVolume(value);
            m_currentMusicVolume = value;
        }

        public virtual void SetEffectsVolume(float value)
        {
            m_audio.SetEffectsVolume(value);
            m_currentEffectsVolume = value;
        }

        public virtual void SetUIEffectsVolume(float value)
        {
            m_audio.SetUIEffectsVolume(value);
            m_currentUiEffectsVolume = value;
        }

        public virtual void SetCameraShake(bool value)
        {
            m_currentCameraShake = value;
        }

        public virtual void SetAutoLoot(bool value)
        {
            m_currentAutoLoot = value;
        }

        public virtual void SetControllerVibration(float value)
        {
            m_currentControllerVibration = value;
        }

        public virtual void SetMonitor(int value)
        {
            if (value >= 0 && value < Display.displays.Length)
            {
                Display.displays[value].Activate();
            }
            m_currentMonitor = value;
        }

        public virtual void SetMaxFPS(int value)
        {
            Application.targetFrameRate = value;
            m_currentMaxFps = value;
        }

        public virtual void SetHDR(bool value)
        {
            foreach (var cam in Camera.allCameras)
            {
                if (cam != null) cam.allowHDR = value;
            }
            m_currentHdr = value;
        }

        public virtual void SetQualityPreset(int value)
        {
            QualitySettings.SetQualityLevel(value);
            m_currentQualityPreset = value;
        }

        public virtual void SetAntiAliasing(int value)
        {
            QualitySettings.antiAliasing = value;
            m_currentAntiAliasing = value;
        }

        public virtual void SetShadows(int value)
        {
            QualitySettings.shadows = (UnityEngine.ShadowQuality)value;
            m_currentShadows = value;
        }

        public virtual void SetTextureQuality(int value)
        {
            QualitySettings.globalTextureMipmapLimit = value;
            m_currentTextureQuality = value;
        }

        public virtual void SetAnisotropicFiltering(int value)
        {
            QualitySettings.anisotropicFiltering = (AnisotropicFiltering)value;
            m_currentAnisotropicFiltering = value;
        }

        public virtual void SetAmbientOcclusion(bool value)
        {
            if (postProcessingVolume != null)
            {
                var urpAssembly = typeof(Bloom).Assembly;
                var aoType = urpAssembly.GetType("UnityEngine.Rendering.Universal.AmbientOcclusion") ??
                             urpAssembly.GetType("UnityEngine.Rendering.Universal.ScreenSpaceAmbientOcclusion");

                if (aoType != null && postProcessingVolume.profile.TryGet(aoType, out var component))
                {
                    component.active = value;
                }
            }

            m_currentAmbientOcclusion = value;
        }

        public virtual void SetBloom(bool value)
        {
            if (postProcessingVolume != null &&
                postProcessingVolume.profile.TryGet<Bloom>(out var bloom))
                bloom.active = value;
            m_currentBloom = value;
        }

        public virtual void SetDepthOfField(bool value)
        {
            if (postProcessingVolume != null &&
                postProcessingVolume.profile.TryGet<DepthOfField>(out var dof))
                dof.active = value;
            m_currentDepthOfField = value;
        }

        public virtual void SetMotionBlur(bool value)
        {
            if (postProcessingVolume != null &&
                postProcessingVolume.profile.TryGet<MotionBlur>(out var mb))
                mb.active = value;
            m_currentMotionBlur = value;
        }

        public virtual void SetGamma(float value)
        {
            if (postProcessingVolume != null &&
                postProcessingVolume.profile.TryGet<ColorAdjustments>(out var color))
                color.postExposure.value = value;
            m_currentGamma = value;
        }

        public virtual int GetMovementSetting() => m_currentMovementSetting;

        public virtual void SetMovementSetting(float value)
        {
            SetMovementSetting((int)value);
        }

        public virtual void SetMovementSetting(int value)
        {
            m_currentMovementSetting = value;
            PlayerPrefs.SetInt(k_movementSettingKey, m_currentMovementSetting);
            PlayerPrefs.Save();

            if (Level.instance && Level.instance.player && Level.instance.player.inputs)
            {
                Level.instance.player.inputs.SetMovementSetting(value);
            }
        }

        protected virtual Vector2Int GetResolutionScale(ResolutionOption option)
        {
            var nativeWidth = Display.main.systemWidth;
            var nativeHeight = Display.main.systemHeight;

            switch (option)
            {
                default:
                case ResolutionOption.Maximum:
                    return new Vector2Int(nativeWidth, nativeHeight);
                case ResolutionOption.High:
                    return new Vector2Int((int)(nativeWidth * 0.8f), (int)(nativeHeight * 0.8f));
                case ResolutionOption.Medium:
                    return new Vector2Int((int)(nativeWidth * 0.6f), (int)(nativeHeight * 0.6f));
                case ResolutionOption.Low:
                    return new Vector2Int((int)(nativeWidth * 0.4f), (int)(nativeHeight * 0.4f));
                case ResolutionOption.Lowest:
                    return new Vector2Int((int)(nativeWidth * 0.3f), (int)(nativeHeight * 0.3f));
            }
        }

        public virtual void Save()
        {
            PlayerPrefs.SetInt(k_resolutionKey, m_currentResolution);
            PlayerPrefs.SetInt(k_fullScreenKey, m_currentFullScreen ? 1 : 0);
            PlayerPrefs.SetInt(k_postProcessingKey, m_currentPostProcessing ? 1 : 0);
            PlayerPrefs.SetInt(k_movementSettingKey, m_currentMovementSetting);

            PlayerPrefs.SetInt("DisplayDifficulty", GetDisplayDifficulty() ? 1 : 0);
            PlayerPrefs.SetInt("DifficultyTextOption", GetDifficultyTextOption());
            PlayerPrefs.SetInt("DisplayDamageText", GetDisplayDamageText() ? 1 : 0);
            PlayerPrefs.SetInt(k_overlayChatOnTopKey, GetOverlayChatAlwaysOnTop() ? 1 : 0);

            PlayerPrefs.SetInt(k_cameraShakeKey, m_currentCameraShake ? 1 : 0);
            PlayerPrefs.SetInt(k_autoLootKey, m_currentAutoLoot ? 1 : 0);
            PlayerPrefs.SetInt(k_enemyHealthBarOptionKey, GetEnemyHealthBarOption());
            PlayerPrefs.SetInt(k_colorblindKey, GetColorblindMode() ? 1 : 0);

            PlayerPrefs.SetInt(k_largeTextKey, GetLargeText() ? 1 : 0);
            PlayerPrefs.SetInt(k_subtitlesKey, GetSubtitlesEnabled() ? 1 : 0);
            PlayerPrefs.SetInt(k_showLatencyKey, GetShowLatency() ? 1 : 0);
            PlayerPrefs.SetInt(k_privacyStatusKey, (int)GetPrivacyStatus());

            PlayerPrefs.SetInt(k_partyInviteModeKey, (int)GetPartyInviteMode());
            PlayerPrefs.SetInt(k_tradeConfirmationsKey, GetTradeConfirmations() ? 1 : 0);
            PlayerPrefs.SetInt(k_blockProfanityKey, GetBlockProfanity() ? 1 : 0);
            PlayerPrefs.SetInt(k_minimapRotationKey, GetMinimapRotation() ? 1 : 0);

            PlayerPrefs.SetInt(k_damageNumberModeKey, GetDamageNumberMode());
            PlayerPrefs.SetInt(k_damageTextSizeKey, GetDamageTextSize());
            PlayerPrefs.SetInt(k_buffTextSizeKey, GetBuffTextSize());
            PlayerPrefs.SetInt(k_itemCompareModeKey, GetItemCompareMode());

            PlayerPrefs.SetInt(k_monitorKey, m_currentMonitor);
            PlayerPrefs.SetInt(k_maxFpsKey, m_currentMaxFps);
            PlayerPrefs.SetInt(k_hdrKey, m_currentHdr ? 1 : 0);
            PlayerPrefs.SetInt(k_qualityPresetKey, m_currentQualityPreset);

            PlayerPrefs.SetInt(k_aaKey, m_currentAntiAliasing);
            PlayerPrefs.SetInt(k_shadowsKey, m_currentShadows);
            PlayerPrefs.SetInt(k_textureQualityKey, m_currentTextureQuality);
            PlayerPrefs.SetInt(k_anisotropicKey, m_currentAnisotropicFiltering);

            PlayerPrefs.SetInt(k_aoKey, m_currentAmbientOcclusion ? 1 : 0);
            PlayerPrefs.SetInt(k_bloomKey, m_currentBloom ? 1 : 0);
            PlayerPrefs.SetInt(k_dofKey, m_currentDepthOfField ? 1 : 0);
            PlayerPrefs.SetInt(k_motionBlurKey, m_currentMotionBlur ? 1 : 0);

            PlayerPrefs.SetInt(k_outputDeviceKey, m_currentOutputDevice);
            PlayerPrefs.SetInt(k_muteOnFocusLossKey, m_currentMuteOnFocusLoss ? 1 : 0);
            PlayerPrefs.SetInt(k_highContrastKey, GetHighContrastUI() ? 1 : 0);
            PlayerPrefs.SetInt(k_flashReductionKey, GetFlashReductionLevel());

            PlayerPrefs.SetFloat(k_musicVolumeKey, m_currentMusicVolume);
            PlayerPrefs.SetFloat(k_effectsVolumeKey, m_currentEffectsVolume);
            PlayerPrefs.SetFloat(k_uiEffectsVolumeKey, m_currentUiEffectsVolume);
            PlayerPrefs.SetFloat(k_controllerVibrationKey, m_currentControllerVibration);

            PlayerPrefs.SetFloat(k_minimapSizeKey, GetMinimapSize());
            PlayerPrefs.SetFloat(k_cursorSizeKey, GetCursorSize());
            PlayerPrefs.SetFloat(k_gammaKey, m_currentGamma);

            PlayerPrefs.SetString(k_languageKey, m_currentLanguage);
        
            if (inputActions != null)
            {
                var json = inputActions.SaveBindingOverridesAsJson();
                PlayerPrefs.SetString(k_bindingsKey, json);
            }

            PlayerPrefs.Save();
        }

        public virtual void SaveBindings()
        {
            if (inputActions == null)
                return;

            var json = inputActions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(k_bindingsKey, json);
            PlayerPrefs.Save();
        }

        public virtual void Load()
        {
#if UNITY_STANDALONE
            var initialResolution = Screen.resolutions.Length - 1;
#else
            var initialResolution = (int)ResolutionOption.Maximum;
#endif
            var resolution = PlayerPrefs.GetInt(k_resolutionKey, initialResolution);
            var fullScreen = PlayerPrefs.GetInt(k_fullScreenKey, Screen.fullScreen ? 1 : 0);
            var postProcessing = PlayerPrefs.GetInt(k_postProcessingKey, 1);
            var musicVolume = PlayerPrefs.GetFloat(k_musicVolumeKey, m_audio.initialMusicVolume);
            var effectsVolume = PlayerPrefs.GetFloat(k_effectsVolumeKey, m_audio.initialEffectsVolume);
            var uiEffectsVolume = PlayerPrefs.GetFloat(k_uiEffectsVolumeKey, m_audio.initialUiEffectsVolume);
            var movementSetting = PlayerPrefs.GetInt(k_movementSettingKey, 0);
            var enemyHealthBarOption = PlayerPrefs.GetInt(k_enemyHealthBarOptionKey, 2);
            var bindingsJson = PlayerPrefs.GetString(k_bindingsKey, string.Empty);
            var cameraShake = PlayerPrefs.GetInt(k_cameraShakeKey, 1);
            var autoLoot = PlayerPrefs.GetInt(k_autoLootKey, 1);
            var controllerVibration = PlayerPrefs.GetFloat(k_controllerVibrationKey, 1f);
            var language = PlayerPrefs.GetString(k_languageKey, LocalizationSettings.SelectedLocale.Identifier.Code);
            var colorblind = PlayerPrefs.GetInt(k_colorblindKey, 0);
            var largeText = PlayerPrefs.GetInt(k_largeTextKey, 0);
            var subtitles = PlayerPrefs.GetInt(k_subtitlesKey, 1);
            var showLatency = PlayerPrefs.GetInt(k_showLatencyKey, 0) == 1;
            var privacyStatus = PlayerPrefs.GetInt(k_privacyStatusKey, 0);
            var partyInviteMode = PlayerPrefs.GetInt(k_partyInviteModeKey, 0);
            var tradeConfirmations = PlayerPrefs.GetInt(k_tradeConfirmationsKey, 1) == 1;
            var blockProfanity = PlayerPrefs.GetInt(k_blockProfanityKey, 0) == 1;
            var minimapSize = PlayerPrefs.GetFloat(k_minimapSizeKey, 1f);
            var minimapRotation = PlayerPrefs.GetInt(k_minimapRotationKey, 1) == 1;
            var damageNumberMode = PlayerPrefs.GetInt(k_damageNumberModeKey, 0);
            var damageTextSize = PlayerPrefs.GetInt(k_damageTextSizeKey, 14);
            var buffTextSize = PlayerPrefs.GetInt(k_buffTextSizeKey, 14);
            var itemCompareMode = PlayerPrefs.GetInt(k_itemCompareModeKey, 0);
            var cursorSize = PlayerPrefs.GetFloat(k_cursorSizeKey, 1f);
            var monitor = PlayerPrefs.GetInt(k_monitorKey, 0);
            var maxFps = PlayerPrefs.GetInt(k_maxFpsKey, Application.targetFrameRate);
            var hdr = PlayerPrefs.GetInt(k_hdrKey, 0);
            var qualityPreset = PlayerPrefs.GetInt(k_qualityPresetKey, QualitySettings.GetQualityLevel());
            var aa = PlayerPrefs.GetInt(k_aaKey, QualitySettings.antiAliasing);
            var shadows = PlayerPrefs.GetInt(k_shadowsKey, (int)QualitySettings.shadows);
            var textureQuality = PlayerPrefs.GetInt(k_textureQualityKey, QualitySettings.globalTextureMipmapLimit);
            var anisotropic = PlayerPrefs.GetInt(k_anisotropicKey, (int)QualitySettings.anisotropicFiltering);
            var ao = PlayerPrefs.GetInt(k_aoKey, 1);
            var bloom = PlayerPrefs.GetInt(k_bloomKey, 1);
            var dof = PlayerPrefs.GetInt(k_dofKey, 1);
            var motionBlur = PlayerPrefs.GetInt(k_motionBlurKey, 1);
            var gamma = PlayerPrefs.GetFloat(k_gammaKey, 0f);
            var outputDevice = PlayerPrefs.GetInt(k_outputDeviceKey, 0);
            var muteOnFocusLoss = PlayerPrefs.GetInt(k_muteOnFocusLossKey, 0);
            var highContrast = PlayerPrefs.GetInt(k_highContrastKey, 0);
            var flashReduction = PlayerPrefs.GetInt(k_flashReductionKey, 0);

            SetResolution(resolution);
            SetFullScreen(fullScreen == 1);
            SetPostProcessing(postProcessing == 1);
            SetMusicVolume(musicVolume);
            SetEffectsVolume(effectsVolume);
            SetUIEffectsVolume(uiEffectsVolume);
            SetMovementSetting(movementSetting);
            SetEnemyHealthBarOption(enemyHealthBarOption);
            SetCameraShake(cameraShake == 1);
            SetAutoLoot(autoLoot == 1);
            SetControllerVibration(controllerVibration);
            SetLanguage(language);
            SetColorblindMode(colorblind == 1);
            SetLargeText(largeText == 1);
            SetSubtitlesEnabled(subtitles == 1);
            SetShowLatency(showLatency);
            SetPrivacyStatus((PrivacyStatus)privacyStatus);
            SetPartyInviteMode((PartyInviteMode)partyInviteMode);
            SetTradeConfirmations(tradeConfirmations);
            SetBlockProfanity(blockProfanity);
            SetMinimapSize(minimapSize);
            SetMinimapRotation(minimapRotation);
            SetDamageNumberMode(damageNumberMode);
            SetDamageTextSize(damageTextSize);
            SetBuffTextSize(buffTextSize);
            SetItemCompareMode(itemCompareMode);
            SetCursorSize(cursorSize);
            SetMonitor(monitor);
            SetMaxFPS(maxFps);
            SetHDR(hdr == 1);
            SetQualityPreset(qualityPreset);
            SetAntiAliasing(aa);
            SetShadows(shadows);
            SetTextureQuality(textureQuality);
            SetAnisotropicFiltering(anisotropic);
            SetAmbientOcclusion(ao == 1);
            SetBloom(bloom == 1);
            SetDepthOfField(dof == 1);
            SetMotionBlur(motionBlur == 1);
            SetGamma(gamma);
            SetOutputDevice(outputDevice);
            SetMuteOnFocusLoss(muteOnFocusLoss == 1);
            SetHighContrastUI(highContrast == 1);
            SetFlashReductionLevel(flashReduction);

            if (!string.IsNullOrEmpty(bindingsJson) && inputActions != null)
                inputActions.LoadBindingOverridesFromJson(bindingsJson);

            bool displayDifficulty = PlayerPrefs.GetInt("DisplayDifficulty", 1) == 1;
            bool displayDamageText = PlayerPrefs.GetInt("DisplayDamageText", 1) == 1;
            int difficultyTextOption = PlayerPrefs.GetInt("DifficultyTextOption", 0);
            bool overlayChatOnTop = PlayerPrefs.GetInt(k_overlayChatOnTopKey, 1) == 1;

            bool saveLogs = GetSaveLogs();

            SetDisplayDifficulty(displayDifficulty);
            SetDisplayDamageText(displayDamageText);
            SetDifficultyTextOption(difficultyTextOption);
            SetOverlayChatAlwaysOnTop(overlayChatOnTop);
            UpdateDifficultyTextTarget();
            SetSaveLogs(saveLogs);
            PlayerBehaviorLogger.Instance?.ApplySettingsFromGameSettings();
        }
    }
}