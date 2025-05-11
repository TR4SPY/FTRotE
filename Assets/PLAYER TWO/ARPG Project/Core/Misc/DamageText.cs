using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Misc/Damage Text")]
    public class DamageText : MonoBehaviour
    {
        [Header("Text Settings")]
        [Tooltip("A reference to the Text component.")]
        public Text damageText;

        [Tooltip("The text used when the damage is zero.")]
        public string missText = "Miss";

        [Header("Color Settings")]
        [Tooltip("The color of the text when the damage is zero.")]
        public Color missColor = Color.white;

        [Tooltip("The color of the text when receiving damage.")]
        public Color regularColor = Color.yellow;

        [Tooltip("The color of the text when the damage is critical.")]
        public Color criticalColor = Color.red;

        protected float m_lifeTime;

        /// <summary>
        /// The transform of the object receiving damage.
        /// </summary>
        public Transform target { get; set; }

        public static List<DamageText> activeTexts = new List<DamageText>();

        private void Awake()
        {
            if (!GameSettings.instance.GetDisplayDamageText())
            {
                gameObject.SetActive(false);
                return;
            }

            activeTexts.Add(this);
        }

        private void OnDestroy()
        {
            activeTexts.Remove(this);
        }

        public static void ToggleAll(bool isEnabled)
        {
            foreach (var text in activeTexts)
            {
                if (text != null)
                    text.gameObject.SetActive(isEnabled);
            }

            Debug.Log($"[AI-DDA] DamageText global visibility set to: {isEnabled}");
        }

        /// <summary>
        /// Sets the text to the damage text.
        /// </summary>
        /// <param name="damage">The amount of damage points.</param>
        /// <param name="critical">If true, the Damage Text uses the critical damage settings.</param>
        public virtual void SetText(int damage, bool critical)
        {
            damageText.text = damage > 0 ? damage.ToString() : missText;
            damageText.color = GetColor(damage, critical);
        }

        protected virtual Color GetColor(int damage, bool critical)
        {
            if (damage <= 0) return missColor;

            return critical ? criticalColor : regularColor;
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