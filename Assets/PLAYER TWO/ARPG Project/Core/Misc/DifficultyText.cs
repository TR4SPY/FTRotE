using UnityEngine;
using UnityEngine.UI;

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

        private float previousDifficulty;
        private int previousStrength, previousDexterity, previousVitality, previousEnergy;

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
    }
}
