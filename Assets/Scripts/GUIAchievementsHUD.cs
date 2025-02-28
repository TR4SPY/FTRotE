using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Achievements HUD")]
    public class GUIAchievementsHUD : MonoBehaviour, HUDManager.IHUD
    {
        [Header("Text References")]
        public Text achievementStatus;
        public Text achievementName;
        public Text achievementDescription;

        [Header("Display Settings")]
        public float showDuration = 0.25f;
        public float hideDuration = 1f;
        public float hideDelay = 3f;

        [Header("Audio Settings")]
        public AudioClip achievementUnlockedClip;

        protected CanvasGroup m_group;
        protected WaitForSeconds m_waitForHideDelay;
        protected GameAudio m_audio => GameAudio.instance;

        protected virtual void InitializeCanvasGroup()
        {
            if (!TryGetComponent(out m_group))
                m_group = gameObject.AddComponent<CanvasGroup>();

            m_group.alpha = 0;
            m_group.interactable = false;
            m_group.blocksRaycasts = false;
        }

        protected virtual void InitializeWaits()
        {
            m_waitForHideDelay = new WaitForSeconds(hideDelay);
        }

        public virtual void ShowAchievement(string status, string name, string description)
        {
            achievementStatus.text = status;
            achievementName.text = name;
            achievementDescription.text = description;

            HUDManager.Instance.RequestDisplay(this);
        }

        public void Show()
        {
            m_audio.PlayUiEffect(achievementUnlockedClip);
            StopAllCoroutines();
            StartCoroutine(ShowRoutine());
        }

        public void Hide()
        {
            StopAllCoroutines();
            StartCoroutine(HideRoutine());
        }

        protected IEnumerator ShowRoutine()
        {
            m_group.interactable = true;
            m_group.blocksRaycasts = true;

            for (float timer = 0; timer < showDuration;)
            {
                timer += Time.deltaTime;
                m_group.alpha = Mathf.Lerp(0, 1, timer / showDuration);
                yield return null;
            }

            m_group.alpha = 1;
            yield return m_waitForHideDelay;

            Hide();
        }

        protected IEnumerator HideRoutine()
{
    m_group.interactable = false;
    m_group.blocksRaycasts = false;

    for (float timer = 0; timer < hideDuration;)
    {
        timer += Time.deltaTime;
        m_group.alpha = Mathf.Lerp(1, 0, timer / hideDuration);
        yield return null;
    }

    m_group.alpha = 0;

    yield return new WaitForSeconds(0.1f);

    HUDManager.Instance.OnHUDHidden(this);
}


        protected virtual void Start()
        {
            InitializeCanvasGroup();
            InitializeWaits();
        }
    }
}
