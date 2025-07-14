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

        [Header("Main Settings Menu")]
        public GameObject mainSettings;

        [Header("Navigation Buttons")]
        public Button soundSettingsButton;
        public Button graphicsSettingsButton;
        public Button uiSettingsButton;
        public Button researchSettingsButton;
        public Button keybindingsSettingsButton;

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
        }

        /// <summary>
        /// Pokazuje menu główne i ukrywa wszystkie okna ustawień.
        /// </summary>
        public void ShowMainSettings()
        {
            mainSettings.SetActive(true);
        }

        /// <summary>
        /// Otwiera ustawienia dźwięku.
        /// </summary>
        public void ShowSoundSettings()
        {
            soundSettingsWindow.Show();
        }

        /// <summary>
        /// Otwiera ustawienia graficzne.
        /// </summary>
        public void ShowGraphicsSettings()
        {
            graphicsSettingsWindow.Show();
        }

        /// <summary>
        /// Otwiera ustawienia interfejsu.
        /// </summary>
        public void ShowUISettings()
        {
            uiSettingsWindow.Show();
        }

        /// <summary>
        /// Otwiera ustawienia badań (research).
        /// </summary>
        public void ShowResearchSettings()
        {
            researchSettingsWindow.Show();
        }

        /// <summary>
        /// Opens the keybindings settings window.
        /// </summary>
        public void ShowKeybindingsSettings()
        {
            keybindingsSettingsWindow.Show();
        }
    }
}
