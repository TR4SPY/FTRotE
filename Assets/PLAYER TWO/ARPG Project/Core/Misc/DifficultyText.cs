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
        public Color increaseColor = Color.red;

        [Tooltip("The color of the text when difficulty decreases.")]
        public Color decreaseColor = Color.green;

        protected float m_lifeTime;

        /// <summary>
        /// The transform of the object affected by difficulty change.
        /// </summary>
        public Transform target { get; set; }

        /// <summary>
        /// Sets the text message and color based on difficulty change.
        /// </summary>
        /// <param name="increased">True if difficulty increased, false if decreased.</param>
        public virtual void SetText(bool increased)
        {
            Debug.Log($"[DEBUG] Setting DifficultyText: {(increased ? "Difficulty Increased" : "Difficulty Decreased")}");

            if (difficultyText == null)
            {
                Debug.LogError("[DEBUG] difficultyText UI component is missing!");
                return;
            }

            difficultyText.text = increased ? "Difficulty Increased" : "Difficulty Decreased";
            difficultyText.color = increased ? increaseColor : decreaseColor;
        }

        protected virtual void LateUpdate()
        {
            if (m_lifeTime > 0.5f)
            {
                Destroy(this.gameObject);
            }

            m_lifeTime += Time.deltaTime;
            transform.position += Vector3.up * Time.deltaTime;
        }
    }
}
