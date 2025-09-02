using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using AI_DDA.Assets.Scripts;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Game/Game Settings")]
    public class GameSettings : Singleton<GameSettings>
    {
        public enum ResolutionOption
        {
            Lowest,
            Low,
            Medium,
            High,
            Maximum
        }

        protected int m_currentResolution;
        protected int m_currentMovementSetting;

        protected bool m_currentFullScreen;
        protected bool m_currentPostProcessing;
        protected bool m_currentCameraShake;
        protected bool m_currentAutoLoot;

        protected float m_currentMusicVolume;
        protected float m_currentEffectsVolume;
        protected float m_currentUiEffectsVolume;
        protected float m_currentControllerVibration;

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

        public bool GetDisplayDifficulty() => PlayerPrefs.GetInt("DisplayDifficulty", 1) == 1;
        public int GetDifficultyTextOption() => PlayerPrefs.GetInt("DifficultyTextOption", 0);
        public bool GetDisplayDamageText() => PlayerPrefs.GetInt("DisplayDamageText", 1) == 1;
        public bool GetOverlayChatAlwaysOnTop() => PlayerPrefs.GetInt(k_overlayChatOnTopKey, 1) == 1;
        public bool GetColorblindMode() => PlayerPrefs.GetInt(k_colorblindKey, 0) == 1;
        public bool GetLargeText() => PlayerPrefs.GetInt(k_largeTextKey, 0) == 1;
        public bool GetSubtitlesEnabled() => PlayerPrefs.GetInt(k_subtitlesKey, 1) == 1;


        protected GameAudio m_audio => GameAudio.instance;

        [Tooltip("The Volume component controlling post-processing effects.")]
        public Volume postProcessingVolume;

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

        protected virtual List<string> GetScreenResolutions() =>
            Screen.resolutions.Select(resolution => resolution.ToString()).ToList();

        protected virtual List<string> GetRenderingResolutions() =>
            System.Enum.GetNames(typeof(ResolutionOption)).ToList();

        public virtual List<string> GetLanguages() =>
            LocalizationSettings.AvailableLocales.Locales.Select(locale => locale.LocaleName).ToList();

        public virtual bool GetFullScreen() => Screen.fullScreen;

        public virtual bool GetPostProcessing() => m_postProcess?.value ?? false;

        public virtual bool GetCameraShake() => m_currentCameraShake;

        public virtual bool GetAutoLoot() => m_currentAutoLoot;

        public virtual float GetMusicVolume() => m_audio.GetMusicVolume();

        public virtual float GetEffectsVolume() => m_audio.GetEffectsVolume();

        public virtual float GetUIEffectsVolume() => m_audio.GetUIEffectsVolume();
    
        public virtual float GetControllerVibration() => m_currentControllerVibration;

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

            PlayerPrefs.SetFloat(k_musicVolumeKey, m_currentMusicVolume);
            PlayerPrefs.SetFloat(k_effectsVolumeKey, m_currentEffectsVolume);
            PlayerPrefs.SetFloat(k_uiEffectsVolumeKey, m_currentUiEffectsVolume);
            PlayerPrefs.SetFloat(k_controllerVibrationKey, m_currentControllerVibration);

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