using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Quest HUD")]
    public class GUIQuestHUD : MonoBehaviour, HUDManager.IHUD
    {
        [Header("Texts References")]
        public Text status;
        public Text title;
        public Text objective;

        [Header("Display Settings")]
        public float showDuration = 0.25f;
        public float hideDuration = 1f;
        public float hideDelay = 3f;

        [Header("Status Messages")]
        public string newQuestStatus = "New Quest";
        public string questCompletedStatus = "Quest Completed";

        [Header("Audio Settings")]
        public AudioClip questAccepted;
        public AudioClip questCompleted;

        protected CanvasGroup m_group;
        protected WaitForSeconds m_waitForHideDelay;
        protected GameAudio m_audio => GameAudio.instance;

        protected virtual void InitializeWaits()
        {
            m_waitForHideDelay = new WaitForSeconds(hideDelay);
        }

        protected virtual void InitializeCanvasGroup()
        {
            if (!TryGetComponent(out m_group))
                m_group = gameObject.AddComponent<CanvasGroup>();

            m_group.alpha = 0;
        }

        protected virtual void InitializeCallbacks()
        {
            LevelQuests.instance.onQuestAdded.AddListener(OnQuestAdded);
            LevelQuests.instance.onQuestCompleted.AddListener(OnQuestCompleted);
            LevelQuests.instance.onQuestRemoved.AddListener(OnQuestRemoved);
        }

        protected virtual void OnQuestAdded(QuestInstance quest)
        {
            m_audio.PlayUiEffect(questAccepted);
            UpdateTexts(quest, newQuestStatus);
            HUDManager.Instance.RequestDisplay(this); // Dodanie do kolejki
        }

        protected virtual void OnQuestCompleted(QuestInstance quest)
        {
            m_audio.PlayUiEffect(questCompleted);
            UpdateTexts(quest, questCompletedStatus);

            // Kolejkowanie w HUDManager
            HUDManager.Instance.RequestDisplay(this);

            // Oryginalne zachowanie
            StopAllCoroutines();
            StartCoroutine(ShowRoutine());
        }

        protected virtual void OnQuestRemoved(QuestInstance quest) { }

        protected virtual void UpdateTexts(QuestInstance quest, string status)
        {
            this.status.text = status;
            title.text = quest.data.title;
            objective.text = quest.data.objective;
        }

        public void Show()
        {
            StopAllCoroutines();
            StartCoroutine(ShowRoutine());
        }

        protected IEnumerator ShowRoutine()
        {
            for (float timer = 0; timer < showDuration;)
            {
                timer += Time.deltaTime;
                m_group.alpha = Mathf.Lerp(0, 1, timer / showDuration);
                yield return null;
            }

            yield return m_waitForHideDelay;

            Hide(); // Automatyczne ukrywanie po wyświetleniu
        }

        public void Hide()
        {
            StopAllCoroutines();
            StartCoroutine(HideRoutine());
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

    yield return new WaitForSeconds(0.1f); // Krótki bufor czasowy

    HUDManager.Instance.OnHUDHidden(this); // Informacja o zakończeniu
}


        protected virtual void Start()
        {
            InitializeWaits();
            InitializeCanvasGroup();
            InitializeCallbacks();
        }
    }
}
