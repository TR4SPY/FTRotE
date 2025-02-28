using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    public class QuestionnaireManager : MonoBehaviour
    {
        private static QuestionnaireManager _instance;
        public static QuestionnaireManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    #if UNITY_2023_1_OR_NEWER
                    _instance = FindFirstObjectByType<QuestionnaireManager>();
                    #else
                    _instance = Object.FindObjectOfType<QuestionnaireManager>();
                    #endif

                   /* if (_instance == null)
                    {
                        Debug.LogError("QuestionnaireManager.Instance was accessed, but no instance exists in the scene.");
                    } */
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                // DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        [Header("Dropdowns")]
        public Dropdown question1Dropdown;
        public Dropdown question2Dropdown;
        public Dropdown question3Dropdown;
        public Dropdown question4Dropdown;
        public Dropdown question5Dropdown;

        [Header("Button")]
        public Button allDoneButton;

        private Dictionary<string, int> playerTypeScores = new Dictionary<string, int>
        {
            { "Achiever", 0 },
            { "Explorer", 0 },
            { "Socializer", 0 },
            { "Killer", 0 }
        };

        public Text playerTypeAttributeText;

        public string playerType = "Undefined";

        private void Start()
        {
            allDoneButton.onClick.AddListener(ProcessAnswers);
        }

        public void ProcessAnswers()
        {
            ResetScores();

            AddScore(question1Dropdown.value);
            AddScore(question2Dropdown.value);
            AddScore(question3Dropdown.value);
            AddScore(question4Dropdown.value);
            AddScore(question5Dropdown.value);

            string dominantType = GetDominantPlayerType();

            if (playerTypeAttributeText != null)
            {
                playerTypeAttributeText.text = dominantType;
            }

            if (Game.instance?.currentCharacter != null)
            {
                Game.instance.currentCharacter.playerType = playerType;
            }

            gameObject.SetActive(false);
        }

        private void ResetScores()
        {
            playerTypeScores["Achiever"] = 0;
            playerTypeScores["Explorer"] = 0;
            playerTypeScores["Socializer"] = 0;
            playerTypeScores["Killer"] = 0;
        }

        private void AddScore(int answerIndex)
        {
            switch (answerIndex)
            {
                case 0:
                    playerTypeScores["Achiever"]++;
                    break;
                case 1:
                    playerTypeScores["Explorer"]++;
                    break;
                case 2:
                    playerTypeScores["Socializer"]++;
                    break;
                case 3:
                    playerTypeScores["Killer"]++;
                    break;
                default:
                    Debug.LogWarning("Invalid answer index.");
                    break;
            }
        }

        private string GetDominantPlayerType()
        {
            string dominantType = "Undefined";
            int maxScore = -1;

            foreach (var type in playerTypeScores)
            {
                if (type.Value > maxScore)
                {
                    maxScore = type.Value;
                    dominantType = type.Key;
                }
            }
            playerType = dominantType;
            return dominantType;
        }
    }
}
