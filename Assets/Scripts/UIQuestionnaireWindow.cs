using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using PLAYERTWO.ARPGProject;
using System.Collections;
using NUnit.Framework.Constraints;

namespace AI_DDA.Assets.Scripts
{
    [AddComponentMenu("AI DDA Project/UI/UI Questionnaire Window")]
    public class UIQuestionnaireWindow : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("A reference to the Game Object that contains the questionnaire's list.")]
        public GameObject questionnaireList;

        [Tooltip("A reference to the Game Object that contains the language selection list.")]
        public GameObject languageList;

        [Tooltip("A reference to the Button component that opens the language list.")]
        public Button languageButton;

        [Tooltip("A reference to the Button component that goes back to the information list.")]
        public Button backButton;

        [Tooltip("A reference to the Button component that exits questionnaire.")]
        public Button allDoneButton;

        [Tooltip("A reference to the Button component for English language.")]
        public Button englishButton;

        [Tooltip("A reference to the Button component for Polish language.")]
        public Button polishButton;

        [Header("Audio References")]
        [Tooltip("The Audio Clip that plays when starting the game.")]
        public AudioClip startGameAudio;

        private GamePause gamePause; // Local reference to GamePause

        protected GameAudio m_audio => GameAudio.instance;

        protected virtual void Start()
        {
            languageList.SetActive(false);
             // Find GamePause in the scene using the modern method
            if (gamePause == null)
            {
                gamePause = Object.FindFirstObjectByType<GamePause>();
                if (gamePause == null)
                {
                    Debug.LogError("GamePause object not found in the scene!");
                }
            }
            
            var currentCharacter = Game.instance.currentCharacter;

            if (currentCharacter == null)
            {
                Debug.LogWarning("No current character found. Questionnaire will not be shown.");
                questionnaireList.SetActive(false);
                languageList.SetActive(false);
                return;
            }

            if (currentCharacter.questionnaireCompleted)
            {
                Debug.Log("Questionnaire already completed for this character. Skipping.");
                questionnaireList.SetActive(false);
                languageList.SetActive(false);
                PauseGame(false); // Ensure the game is unpaused
            }
            else
            {
                // Delay the game pause to ensure everything is loaded
                StartCoroutine(ShowQuestionnaireWithDelay()); // Delay for the game to load
            }
        }

        // Mark the questionnaire as completed when the "All Done" button is clicked
        protected virtual void InitializeCallbacks()
        {
            allDoneButton.onClick.AddListener(() =>
            {
                var currentCharacter = Game.instance.currentCharacter;

                if (currentCharacter != null)
                {
                    currentCharacter.questionnaireCompleted = true; // Mark as completed
                    GameSave.instance.Save(); // Save the updated state
                    Debug.Log($"Questionnaire completed for character: {currentCharacter.name}");
                }
                
                questionnaireList.SetActive(false);
                PauseGame(false); // Unpause the game
            });

            languageButton.onClick.AddListener(ShowLanguageList);
            backButton.onClick.AddListener(HideLanguageList);

            englishButton.onClick.AddListener(() => SetLanguage("en"));
            polishButton.onClick.AddListener(() => SetLanguage("pl"));
        }

        private IEnumerator ShowQuestionnaireWithDelay()
        {
            // Minimalny czas oczekiwania
            float minWaitTime = 1f;
            float startTime = Time.realtimeSinceStartup;

            // Czekaj, aż gra będzie gotowa
            while (!IsGameReady())
            {
                yield return null;
            }

            // Jeśli minimalny czas nie minął, poczekaj dodatkowe sekundy
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            if (elapsedTime < minWaitTime)
            {
                yield return new WaitForSeconds(minWaitTime - elapsedTime);
            }

            Debug.Log("Showing questionnaire for the first time.");
            questionnaireList.SetActive(true);
            InitializeCallbacks();
            PauseGame(true); // Pause the game for the questionnaire
        }

        private bool IsGameReady()
        {
            return Game.instance != null && Game.instance.currentCharacter != null;
        }

        public virtual void ShowLanguageList()
        {
            languageList.SetActive(true);
        }

        public virtual void HideLanguageList()
        {
            languageList.SetActive(false);
        }

        public virtual void SetLanguage(string languageCode)
        {
            Debug.Log($"Setting language to: {languageCode}");
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(languageCode);
        }

        private void PauseGame(bool pause)
        {
            if (gamePause != null)
            {
                gamePause.Pause(pause);
            }
            else
            {
                Debug.LogWarning("GamePause object is not assigned. Cannot pause/unpause the game.");
            }
        }
    }
}