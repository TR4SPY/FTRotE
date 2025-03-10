using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Misc/Difficulty Text")]
    public class DifficultyText : MonoBehaviour
    {
        [Header("Text Settings")]
        public Text difficultyText;

        [Header("Color Settings")]
        public Color increaseColor = Color.yellow;
        public Color decreaseColor = Color.blue;
        public Color buffColor     = Color.green;
        public Color weakenColor   = Color.red;

        [Tooltip("Time (seconds) before destroying this object.")]
        public float lifetime = 2f;

        // =========================================
        // [1] Statyczna lista, żeby GameSettings.cs
        //     mogło odwoływać się do activeTexts.
        // =========================================
        public static List<DifficultyText> activeTexts = new List<DifficultyText>();

        // =========================================
        // [2] Metoda statyczna ToggleAll(bool isEnabled),
        //     wywoływana z GameSettings.cs (SetDisplayDifficulty).
        // =========================================
        public static void ToggleAll(bool isEnabled)
        {
            foreach (var text in activeTexts)
            {
                if (text != null)
                    text.gameObject.SetActive(isEnabled);
            }
            Debug.Log($"[AI-DDA] DifficultyText global visibility set to: {isEnabled}");
        }

        public Transform target { get; set; }

        protected float m_lifeTime;

        public enum MessageType
        {
            DifficultyIncreased,
            DifficultyDecreased,
            BuffUp,
            WeakenUp
        }

        private void Awake()
        {
            activeTexts.Add(this);

            if (!GameSettings.instance.GetDisplayDifficulty())
            {
                gameObject.SetActive(false);
                return;
            }

            if (!TryGetComponent<Billboard>(out _))
            {
                gameObject.AddComponent<Billboard>();
            }
        }

        private void OnDestroy()
        {
            activeTexts.Remove(this);
        }

        public void SetMessageType(MessageType type)
        {
            if (difficultyText == null)
            {
                Debug.LogError("[DifficultyText] UI Text is missing!");
                return;
            }

            switch (type)
            {
                case MessageType.DifficultyIncreased:
                    difficultyText.text = "Difficulty Increased";
                    difficultyText.color = increaseColor;
                    break;

                case MessageType.DifficultyDecreased:
                    difficultyText.text = "Difficulty Decreased";
                    difficultyText.color = decreaseColor;
                    break;

                case MessageType.BuffUp:
                    difficultyText.text = "Buff Up";
                    difficultyText.color = buffColor;
                    break;

                case MessageType.WeakenUp:
                    difficultyText.text = "Weaken Up";
                    difficultyText.color = weakenColor;
                    break;
            }
        }

        protected virtual void LateUpdate()
        {
            m_lifeTime += Time.deltaTime;
            if (m_lifeTime > lifetime)
            {
                Destroy(gameObject);
            }

            transform.position += Vector3.up * (Time.deltaTime * 0.3f);
        }
    }
}
