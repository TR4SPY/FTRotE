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
            DetachSettingsWindows();
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
        /// Shows the main settings menu and hides any sub windows.
        /// </summary>
        public void ShowMainSettings()
        {
            mainSettings.SetActive(true);

            soundSettingsWindow?.Hide();
            graphicsSettingsWindow?.Hide();
            uiSettingsWindow?.Hide();
            researchSettingsWindow?.Hide();
            keybindingsSettingsWindow?.Hide();
            gameplaySettingsWindow?.Hide();
            languageSettingsWindow?.Hide();
            accessibilitySettingsWindow?.Hide();
        }

        /// <summary>
        /// Hides only the main settings menu, keeping sub-window parents active.
        /// </summary>
        private void HideMainSettings()
        {
            mainSettings.SetActive(false);
        }

        private void DetachSettingsWindows()
        {
            if (!mainSettings) return;
            var parent = mainSettings.transform.parent;

            soundSettingsWindow?.transform.SetParent(parent, false);
            graphicsSettingsWindow?.transform.SetParent(parent, false);
            uiSettingsWindow?.transform.SetParent(parent, false);
            researchSettingsWindow?.transform.SetParent(parent, false);
            keybindingsSettingsWindow?.transform.SetParent(parent, false);
            gameplaySettingsWindow?.transform.SetParent(parent, false);
            languageSettingsWindow?.transform.SetParent(parent, false);
            accessibilitySettingsWindow?.transform.SetParent(parent, false);
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
