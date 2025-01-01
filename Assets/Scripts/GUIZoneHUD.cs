using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Zone HUD")]
    public class GUIZoneHUD : MonoBehaviour
    {
        [Header("Text References")]
        [Tooltip("A reference to the Text component that represents the zone status.")]
        public Text zoneStatus;

        [Tooltip("A reference to the Text component that represents the zone name.")]
        public Text zoneName;

        [Tooltip("A reference to the Text component that represents the zone description.")]
        public Text zoneDescription;

        [Header("Display Settings")]
        [Tooltip("The duration in seconds the HUD takes to fade in.")]
        public float showDuration = 0.25f;

        [Tooltip("The duration in seconds the HUD takes to fade out.")]
        public float hideDuration = 1f;

        [Tooltip("The duration in seconds before the HUD starts to fade out.")]
        public float hideDelay = 3f;

        [Header("Audio Settings")]
        [Tooltip("The Audio Clip that plays when a new zone is discovered.")]
        public AudioClip zoneDiscoveredClip;

        protected CanvasGroup m_group;

        protected WaitForSeconds m_waitForHideDelay;

        protected GameAudio m_audio => GameAudio.instance;

        protected virtual void InitializeCanvasGroup()
        {
            if (!TryGetComponent(out m_group))
                m_group = gameObject.AddComponent<CanvasGroup>();

            m_group.alpha = 0;
        }

        protected virtual void InitializeWaits()
        {
            m_waitForHideDelay = new WaitForSeconds(hideDelay);
        }

        public virtual void ShowZone(string status, string name, string description)
        {
            zoneStatus.text = status;
            zoneName.text = name;
            zoneDescription.text = description;
            m_audio.PlayUiEffect(zoneDiscoveredClip);
            StopAllCoroutines();
            StartCoroutine(ShowRoutine());
        }

        protected IEnumerator ShowRoutine()
        {
            for (float timer = 0; timer < showDuration; )
            {
                timer += Time.deltaTime;
                m_group.alpha = Mathf.Lerp(0, 1, timer / showDuration);
                yield return null;
            }

            yield return m_waitForHideDelay;

            for (float timer = 0; timer < hideDuration; )
            {
                timer += Time.deltaTime;
                m_group.alpha = Mathf.Lerp(1, 0, timer / hideDuration);
                yield return null;
            }

            m_group.alpha = 0;
        }

        protected virtual void Start()
        {
            InitializeCanvasGroup();
            InitializeWaits();
        }
    }
}
