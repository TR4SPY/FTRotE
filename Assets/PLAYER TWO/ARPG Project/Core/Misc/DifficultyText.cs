using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Misc/Difficulty Text")]
    public class DifficultyText : MonoBehaviour
    {
        [Header("Text Settings")]
        [Tooltip("A reference to the Text component.")]
        public Text difficultyText;

        [Header("Color Settings")]
        [Tooltip("The color of the text when difficulty increases.")]
        public Color increaseColor = Color.yellow;

        [Tooltip("The color of the text when difficulty decreases.")]
        public Color decreaseColor = Color.blue;

        [Tooltip("The color of the text when stats change without difficulty change.")]
        public Color buffColor = Color.green;
        public Color weakenColor = Color.red;

        protected float m_lifeTime;
        public Transform target { get; set; }

        public static List<DifficultyText> activeTexts = new List<DifficultyText>(); // Lista aktywnych instancji

        private float previousDifficulty;
        private int previousStrength, previousDexterity, previousVitality, previousEnergy;
        
        private void Awake()
{
    if (!GameSettings.instance.GetDisplayDifficulty())
    {
        gameObject.SetActive(false);
        return;
    }

    int option = GameSettings.instance.GetDifficultyTextOption();
    bool isPlayerText = option == 1;
    bool isEnemyText = option == 0;

    if (gameObject.CompareTag("Entity/Player") && isPlayerText)
    {
        ShowDifficultyForPlayer();
        return;
    }

    if ((gameObject.CompareTag("Entity/Enemy") && !isEnemyText) || 
        (gameObject.CompareTag("Entity/Player") && !isPlayerText))
    {
        gameObject.SetActive(false);
        return;
    }

    // 🔹 Upewniamy się, że obiekt ma Billboard, aby zawsze był skierowany do kamery
    if (!TryGetComponent<Billboard>(out _))
    {
        gameObject.AddComponent<Billboard>();
    }

    activeTexts.Add(this);
}


        private void OnDestroy()
        {
            activeTexts.Remove(this); // Usuwamy instancję po zniszczeniu obiektu
        }

        /// <summary>
        /// Ustawia wartości początkowe do porównania.
        /// </summary>
        public void Initialize(float difficulty, int strength, int dexterity, int vitality, int energy)
        {
            previousDifficulty = difficulty;
            previousStrength = strength;
            previousDexterity = dexterity;
            previousVitality = vitality;
            previousEnergy = energy;
        }

        /// <summary>
        /// Sprawdza, czy statystyki przeciwnika zmieniły się zgodnie ze zmianą trudności.
        /// </summary>
        public void CheckDifficultyChange(float newDifficulty, int newStrength, int newDexterity, int newVitality, int newEnergy, string entityTag)
        {
            if (entityTag == "Entity/Player") return;

            bool difficultyIncreased = newDifficulty > previousDifficulty;
            bool difficultyDecreased = newDifficulty < previousDifficulty;

            bool statsIncreased = (newStrength > previousStrength || newDexterity > previousDexterity ||
                                newVitality > previousVitality || newEnergy > previousEnergy);

            bool statsDecreased = (newStrength < previousStrength || newDexterity < previousDexterity ||
                                newVitality < previousVitality || newEnergy < previousEnergy);

            if (difficultyIncreased && statsIncreased)
            {
                SetText("Difficulty Increased", increaseColor);
            }
            else if (difficultyDecreased && statsDecreased)
            {
                SetText("Difficulty Decreased", decreaseColor);
            }
            else if (!difficultyIncreased && !difficultyDecreased && statsIncreased)
            {
                SetText("Buff Up", buffColor);
            }
            else if (!difficultyIncreased && !difficultyDecreased && statsDecreased)
            {
                SetText("Weaken Up", weakenColor);
            }

            previousDifficulty = newDifficulty;
            previousStrength = newStrength;
            previousDexterity = newDexterity;
            previousVitality = newVitality;
            previousEnergy = newEnergy;
        }
        private void ShowDifficultyForPlayer()
{
    float adjustedDifficulty = DifficultyManager.Instance.GetAdjustedDifficulty(); 
    string message = adjustedDifficulty > previousDifficulty ? "Difficulty Increased" : "Difficulty Decreased";
    Color textColor = adjustedDifficulty > previousDifficulty ? increaseColor : decreaseColor;

    // 🔹 Usuwamy istniejące teksty nad graczem, aby uniknąć duplikacji
    List<DifficultyText> textsToRemove = new List<DifficultyText>(activeTexts);
    foreach (var difficultyTextInstance in textsToRemove)
    {
        if (difficultyTextInstance != null && difficultyTextInstance.gameObject.CompareTag("Entity/Player"))
        {
            Destroy(difficultyTextInstance.gameObject);
        }
    }

    // 🔹 Tworzymy dynamicznie `DifficultyText` nad graczem
    var player = GameObject.FindGameObjectWithTag("Entity/Player");
    if (player == null)
    {
        Debug.LogError("[AI-DDA] No player found! Cannot instantiate DifficultyText.");
        return;
    }

    var origin = player.transform.position + new Vector3(0, 2f, 0); // Umieszczamy tekst nad głową gracza
    var instance = Instantiate(this, origin, Quaternion.identity);
    
    if (instance.TryGetComponent(out DifficultyText difficultyText))
    {
        difficultyText.target = player.transform; // 🔹 Przypisujemy target do gracza
        difficultyText.SetText(message, textColor);
        Debug.Log($"[AI-DDA] Player DifficultyText instantiated: {message}");
    }
    else
    {
        Debug.LogError("[AI-DDA] Instantiated object does not have DifficultyText component!");
    }

    previousDifficulty = adjustedDifficulty;
}

        /// <summary>
        /// Ustawia tekst i kolor.
        /// </summary>
        public void SetText(string text, Color color)
        {
            if (difficultyText == null)
            {
                Debug.LogError("[DEBUG] difficultyText UI component is missing!");
                return;
            }

            difficultyText.text = text;
            difficultyText.color = color;
        }

        protected virtual void LateUpdate()
        {
            if (m_lifeTime > 1.0f)
            {
                Destroy(this.gameObject);
            }

            m_lifeTime += Time.deltaTime;
            transform.position += Vector3.up * Time.deltaTime;
        }

        /// <summary>
        /// 🔹 Włącza/wyłącza wszystkie `DifficultyText` w grze.
        /// </summary>
        public static void ToggleAll(bool isEnabled)
        {
            foreach (var text in activeTexts)
            {
                if (text != null)
                    text.gameObject.SetActive(isEnabled);
            }

            Debug.Log($"[AI-DDA] DifficultyText global visibility set to: {isEnabled}");
        }
    }
}
