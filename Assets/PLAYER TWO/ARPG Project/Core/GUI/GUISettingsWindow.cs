using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Settings Window")]
    public class GUISettingsWindow : GUIWindow
    {
        [Header("Settings Windows")]
        public GUIWindow soundSettingsWindow;
        public GUIWindow graphicsSettingsWindow;
        public GUIWindow uiSettingsWindow;
        public GUIWindow researchSettingsWindow;
        public GUIWindow keybindingsSettingsWindow;
        public GUIWindow gameplaySettingsWindow;
        public GUIWindow languageSettingsWindow;
        public GUIWindow accessibilitySettingsWindow;

        [Header("Main Settings Menu")]
        public GameObject mainSettings;

        [Header("Navigation Buttons")]
        public Button soundSettingsButton;
        public Button graphicsSettingsButton;
        public Button uiSettingsButton;
        public Button researchSettingsButton;
        public Button keybindingsSettingsButton;
        public Button gameplaySettingsButton;
        public Button languageSettingsButton;
        public Button accessibilitySettingsButton;

        protected override void Start()
        {
            base.Start();
            ShowMainSettings();
            
            if (soundSettingsButton != null)
                soundSettingsButton.onClick.AddListener(ShowSoundSettings);
            else
                Debug.LogError("[Settings] Sound Settings Button is NULL! Assign it in the Inspector.");

            if (graphicsSettingsButton != null)
                graphicsSettingsButton.onClick.AddListener(ShowGraphicsSettings);
            else
                Debug.LogError("[Settings] Graphics Settings Button is NULL! Assign it in the Inspector.");

            if (uiSettingsButton != null)
                uiSettingsButton.onClick.AddListener(ShowUISettings);
            else
                Debug.LogError("[Settings] UI Settings Button is NULL! Assign it in the Inspector.");

            if (researchSettingsButton != null)
                researchSettingsButton.onClick.AddListener(ShowResearchSettings);
            else
                Debug.LogError("[Settings] Research Settings Button is NULL! Assign it in the Inspector.");

            if (keybindingsSettingsButton != null)
                keybindingsSettingsButton.onClick.AddListener(ShowKeybindingsSettings);
            else
                Debug.LogError("[Settings] Keybindings Settings Button is NULL! Assign it in the Inspector.");

            if (gameplaySettingsButton != null)
                gameplaySettingsButton.onClick.AddListener(ShowGameplaySettings);
            else
                Debug.LogError("[Settings] Gameplay Settings Button is NULL! Assign it in the Inspector.");

            if (languageSettingsButton != null)
                languageSettingsButton.onClick.AddListener(ShowLanguageSettings);
            else
                Debug.LogError("[Settings] Language Settings Button is NULL! Assign it in the Inspector.");

            if (accessibilitySettingsButton != null)
                accessibilitySettingsButton.onClick.AddListener(ShowAccessibilitySettings);
            else
                Debug.LogError("[Settings] Accessibility Settings Button is NULL! Assign it in the Inspector.");
        }

        /// <summary>
        /// Pokazuje menu główne i ukrywa wszystkie okna ustawień.
        /// </summary>
        public void ShowMainSettings()
        {
            mainSettings.SetActive(true);
        }

        /// <summary>
        /// Hides the main settings menu.
        /// </summary>
        private void HideMainSettings()
        {
            mainSettings.SetActive(false);
        }

        /// <summary>
        /// Otwiera ustawienia dźwięku.
        /// </summary>
        public void ShowSoundSettings()
        {
            HideMainSettings();
            soundSettingsWindow.Show();
        }

        /// <summary>
        /// Otwiera ustawienia graficzne.
        /// </summary>
        public void ShowGraphicsSettings()
        {
            HideMainSettings();
            graphicsSettingsWindow.Show();
        }

        /// <summary>
        /// Otwiera ustawienia interfejsu.
        /// </summary>
        public void ShowUISettings()
        {
            HideMainSettings();
            uiSettingsWindow.Show();
        }

        /// <summary>
        /// Otwiera ustawienia badań (research).
        /// </summary>
        public void ShowResearchSettings()
        {
            HideMainSettings();
            researchSettingsWindow.Show();
        }

        /// <summary>
        /// Opens the keybindings settings window.
        /// </summary>
        public void ShowKeybindingsSettings()
        {
            HideMainSettings();
            keybindingsSettingsWindow.Show();
        }

        /// <summary>
        /// Opens the gameplay settings window.
        /// </summary>
        public void ShowGameplaySettings()
        {
            HideMainSettings();
            gameplaySettingsWindow.Show();
        }

        /// <summary>
        /// Opens the language settings window.
        /// </summary>
        public void ShowLanguageSettings()
        {
            HideMainSettings();
            languageSettingsWindow.Show();
        }

        /// <summary>
        /// Opens the accessibility settings window.
        /// </summary>
        public void ShowAccessibilitySettings()
        {
            HideMainSettings();
            accessibilitySettingsWindow.Show();
        }
    }
}
