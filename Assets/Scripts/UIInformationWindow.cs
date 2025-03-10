using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    [AddComponentMenu("AI DDA Project/UI/UI Information Window")]
    public class UIInformationWindow : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("A reference to the Game Object that contains the information's list.")]
        public GameObject informationList;

        [Tooltip("A reference to the Game Object that contains the language selection list.")]
        public GameObject languageList;

        [Tooltip("A reference to the Button component that opens the language list.")]
        public Button languageButton;

        [Tooltip("A reference to the Button component that goes back to the information list.")]
        public Button backButton;

        [Tooltip("A reference to the Button component that starts the Game.")]
        public Button understandButton;

        [Tooltip("A reference to the Button component that exits the game.")]
        public Button declineButton;

        [Tooltip("A reference to the Button component for English language.")]
        public Button englishButton;

        [Tooltip("A reference to the Button component for Polish language.")]
        public Button polishButton;

        [Header("Audio References")]
        [Tooltip("The Audio Clip that plays when starting the game.")]
        public AudioClip startGameAudio;

        protected GameAudio m_audio => GameAudio.instance;

        protected virtual void Start()
        {
            InitializeCallbacks();
            ShowInformationList();
        }

        protected virtual void InitializeCallbacks()
        {
            understandButton.onClick.AddListener(StartGame);
            declineButton.onClick.AddListener(ExitGame);
            languageButton.onClick.AddListener(ShowLanguageList);
            backButton.onClick.AddListener(ShowInformationList);

            englishButton.onClick.AddListener(() => SetLanguage("en"));
            polishButton.onClick.AddListener(() => SetLanguage("pl"));
        }
        public virtual void StartGame()
        {
            if (startGameAudio != null)
            {
                m_audio.PlayUiEffect(startGameAudio);
            }

            var gameScenes = GameScenes.instance;
            if (gameScenes != null)
            {
                gameScenes.LoadScene("Title");
            }
            else
            {
                Debug.LogError("GameScenes instance not found!");
            }
        }
        public virtual void ExitGame()
        {
            Debug.Log("Exiting the game...");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        public virtual void ShowLanguageList()
        {
            informationList.SetActive(false);
            languageList.SetActive(true);
        }
        public virtual void ShowInformationList()
        {
            languageList.SetActive(false);
            informationList.SetActive(true);
        }
        public virtual void SetLanguage(string languageCode)
        {
            Debug.Log($"Setting language to: {languageCode}");
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(languageCode);
        }
    }
}
