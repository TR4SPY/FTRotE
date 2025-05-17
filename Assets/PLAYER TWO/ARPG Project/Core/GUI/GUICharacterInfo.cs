using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Character Info")]
    public class GUICharacterInfo : MonoBehaviour
    {
        [Tooltip("Reference to the Text component for the character's info.")]
        public Text infoText;
        [Tooltip("Reference to the Text component for the character's level.")]
        public Text levelText;

        [Tooltip("Offset for positioning the UI element above the character.")]
        public Vector3 offset = new Vector3(0, 2f, 0);
        public Transform m_target; // Właściwość tylko do odczytu
        protected Camera m_camera;

        protected virtual void InitializeCamera()
        {
            m_camera = Camera.main;

            if (m_camera == null)
            {
                Debug.LogWarning("Main Camera not found! Retrying...");
                StartCoroutine(RetryInitializeCamera());
            }
        }

        /// <summary>
        /// Sets the character info to display.
        /// </summary>
        /// <param name="target">The Transform of the character.</param>
        /// <param name="name">The character's name.</param>
        /// <param name="characterClass">The character's class.</param>
        /// <param name="level">The character's level.</param>
        public virtual void SetCharacterInfo(Transform target, string name, string characterClass, int level)
        {
            if (target == null)
            {
                Debug.LogWarning("Target is null. Skipping character info setup.");
                return;
            }

            m_target = target;
            infoText.text = $"{name} - {characterClass}";
            levelText.text = $"Level {level}";
        }

        protected virtual void Start()
        {
            if (m_camera == null)
            {
                m_camera = Camera.main;
                if (m_camera == null)
                {
                    Debug.LogWarning("Main Camera not found. Retrying...");
                    StartCoroutine(RetryInitializeCamera());
                }
            }
        }

        protected virtual void OnEnable()
{
    RefreshCameraAndPosition();          // ← od razu po ponownym włączeniu
}

/// <summary>Łapie aktualną kamerę i ustawia pozycję etykiety.</summary>
public void RefreshCameraAndPosition()
{
    m_camera = Camera.main;              // zawsze aktualna MainCamera
    if (m_camera == null || m_target == null) return;

    var pos = m_camera.WorldToScreenPoint(m_target.position + offset);
    transform.position = pos;
}


        private IEnumerator RetryInitializeCamera()
        {
            yield return new WaitForSeconds(0.1f); // Poczekaj 100 ms
            m_camera = Camera.main;

            if (m_camera == null)
            {
                Debug.LogError("Failed to initialize Main Camera after retry.");
            }
        }

        protected virtual void LateUpdate()
        {
            // Sprawdź, czy m_target jest zniszczony
            if (m_target == null || m_target.Equals(null))
            {
                Debug.LogWarning("Target has been destroyed. Delaying UI removal.");
                StartCoroutine(DelayedDestroy(0.5f)); // Opóźnienie przed usunięciem UI
                return;
            }

            var position = m_target.position + offset;
            var screenPos = m_camera.WorldToScreenPoint(position);
            transform.position = screenPos;

            // Kontrola widoczności w zależności od położenia względem kamery
            if (screenPos.z < 0)
            {
                infoText.enabled = false; // Ukryj, gdy postać jest za kamerą
                levelText.enabled = false;
            }
            else
            {
                infoText.enabled = true; // Pokaż, gdy postać jest widoczna
                levelText.enabled = true;
            }
        }

        private IEnumerator DelayedDestroy(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (m_target == null || m_target.Equals(null))
            {
                Debug.LogWarning("Final check failed. Destroying UI element.");
                Destroy(this.gameObject);
            }
        }

    }
}