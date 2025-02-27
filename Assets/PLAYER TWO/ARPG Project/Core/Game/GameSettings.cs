using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using AI_DDA.Assets.Scripts;

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
        protected bool m_currentFullScreen;
        protected bool m_currentPostProcessing;
        protected float m_currentMusicVolume;
        protected float m_currentEffectsVolume;
        protected float m_currentUiEffectsVolume;

        protected const string k_resolutionKey = "settings/resolution";
        protected const string k_fullScreenKey = "settings/fullScreen";
        protected const string k_postProcessingKey = "settings/postProcessing";
        protected const string k_musicVolumeKey = "settings/musicVolume";
        protected const string k_effectsVolumeKey = "settings/effectsVolume";
        protected const string k_uiEffectsVolumeKey = "settings/uiEffectsVolume";

        public bool GetDisplayDifficulty() => PlayerPrefs.GetInt("DisplayDifficulty", 1) == 1;
        public int GetDifficultyTextOption() => PlayerPrefs.GetInt("DifficultyTextOption", 0);
        public bool GetDisplayDamageText() => PlayerPrefs.GetInt("DisplayDamageText", 1) == 1;

        protected GameAudio m_audio => GameAudio.instance;

        private const string SaveLogsKey = "SaveLogs";

        public bool GetSaveLogs()
        {
            return PlayerPrefs.GetInt(SaveLogsKey, 1) == 1; // Domyślnie włączone (1)
        }

        public void SetSaveLogs(bool isEnabled)
        {
            PlayerPrefs.SetInt(SaveLogsKey, isEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        // Zabezpieczenie przed brakiem PostProcessToggler
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

        public virtual bool GetFullScreen() => Screen.fullScreen;

        public virtual bool GetPostProcessing() => m_postProcess?.value ?? false;

        public virtual float GetMusicVolume() => m_audio.GetMusicVolume();

        public virtual float GetEffectsVolume() => m_audio.GetEffectsVolume();

        public virtual float GetUIEffectsVolume() => m_audio.GetUIEffectsVolume();

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

        public void ToggleDifficultyText()
        {
            bool isEnabled = GetDisplayDifficulty();
            DifficultyText.ToggleAll(isEnabled);
        }

        public void UpdateDifficultyTextTarget()
{
    int option = GetDifficultyTextOption();

    // 🔹 Usuwamy stare teksty przed zmianą opcji
    List<DifficultyText> textsToRemove = new List<DifficultyText>(DifficultyText.activeTexts);
    foreach (var text in textsToRemove)
    {
        if (text == null) continue;

        bool shouldBeRemoved = (text.gameObject.CompareTag("Entity/Enemy") && option == 1) || 
                               (text.gameObject.CompareTag("Entity/Player") && option == 0);

        if (shouldBeRemoved)
        {
            Destroy(text.gameObject); // 🔹 Usuwamy stare napisy, zamiast tylko je wyłączać
        }
    }

    Debug.Log($"[AI-DDA] DifficultyText updated to option: {option}");
}

        public void SetDisplayDamageText(bool value)
        {
            PlayerPrefs.SetInt("DisplayDamageText", value ? 1 : 0);
            PlayerPrefs.Save();
            
            DamageText.ToggleAll(value); // 🔹 Włącza/wyłącza DamageText w całej grze
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

            PlayerPrefs.SetInt("DisplayDifficulty", GetDisplayDifficulty() ? 1 : 0);
            PlayerPrefs.SetInt("DifficultyTextOption", GetDifficultyTextOption());
            PlayerPrefs.SetInt("DisplayDamageText", GetDisplayDamageText() ? 1 : 0);

            PlayerPrefs.SetFloat(k_musicVolumeKey, m_currentMusicVolume);
            PlayerPrefs.SetFloat(k_effectsVolumeKey, m_currentEffectsVolume);
            PlayerPrefs.SetFloat(k_uiEffectsVolumeKey, m_currentUiEffectsVolume);
            
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

            SetResolution(resolution);
            SetFullScreen(fullScreen == 1);
            SetPostProcessing(postProcessing == 1);
            SetMusicVolume(musicVolume);
            SetEffectsVolume(effectsVolume);
            SetUIEffectsVolume(uiEffectsVolume);

            bool displayDifficulty = PlayerPrefs.GetInt("DisplayDifficulty", 1) == 1;
            bool displayDamageText = PlayerPrefs.GetInt("DisplayDamageText", 1) == 1;
            int difficultyTextOption = PlayerPrefs.GetInt("DifficultyTextOption", 0);

            SetDisplayDifficulty(displayDifficulty);
        }
    }
}
