using UnityEngine;
using UnityEngine.UI;

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

        protected Transform m_target;
        protected Camera m_camera;

        protected virtual void InitializeCamera() => m_camera = Camera.main;

        /// <summary>
        /// Sets the character info to display.
        /// </summary>
        /// <param name="target">The Transform of the character.</param>
        /// <param name="name">The character's name.</param>
        /// <param name="characterClass">The character's class.</param>
        /// <param name="level">The character's level.</param>
        public virtual void SetCharacterInfo(Transform target, string name, string characterClass, int level)
        {
            if (!target) return;

            m_target = target;
            infoText.text = $"{name} - {characterClass}";
            levelText.text = $"Level {level}";
        }

        protected virtual void Start()
        {
            InitializeCamera();
        }

        protected virtual void LateUpdate()
        {
            if (!m_target)
            {
                Destroy(this.gameObject);
                return;
            }

            var position = m_target.position + offset;
            var screenPos = m_camera.WorldToScreenPoint(position);
            transform.position = screenPos;

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
    }
}