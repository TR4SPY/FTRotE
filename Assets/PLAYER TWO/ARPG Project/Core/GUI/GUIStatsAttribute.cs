using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Stats Attribute")]
    public class GUIStatsAttribute : MonoBehaviour
    {
        [Tooltip("A reference to the Text component used to display the points.")]
        public Text pointsText;

        [Tooltip("A reference to the Button component to increment the points.")]
        public Button addButton;

        [Tooltip("A reference to the Button component to decrease the points.")]
        public Button removeButton;

        [Header("Audio Settings")]
        [Tooltip("The Audio Clip that plays when points are added.")]
        public AudioClip addPointClip;

        [Tooltip("The Audio Clip that plays when points are removed.")]
        public AudioClip removePointClip;

        /// Default color of the points text used when the value matches the base value.
        /// </summary>
        private Color m_defaultColor;

        /// <summary>
        /// Indicates whether the default colour has already been captured.
        /// </summary>
        private bool m_hasDefaultColor;


        /// <summary>
        /// Base value of the attribute without any modifications.
        /// </summary>
        private int m_basePoints;

        /// <summary>
        /// Indicates whether the base points have been initialised to avoid early colour changes.
        /// </summary>
        private bool m_hasBasePoints;

        private Coroutine holdRoutine;
        private bool isHolding = false;
        private bool isAddButtonHeld = false;

        [Tooltip("Maximum duration in seconds for a continuous hold action.")]
        public float maxHoldDuration = 5f;

        protected GUIStatsManager m_stats;

        /// <summary>
        /// Returns the GUI Stats Manager associated to this attribute.
        /// </summary>
        public GUIStatsManager stats
        {
            get
            {
                if (!m_stats)
                    m_stats = GetComponentInParent<GUIStatsManager>();

                return m_stats;
            }
        }

        /// <summary>
        /// Returns the amount of distributed points.
        /// </summary>
        public int distributedPoints { get; set; }

        /// <summary>
        /// Returns the amount of points on this attributes.
        /// </summary>
        public int currentPoints { get; set; }

        protected GameAudio m_audio => GameAudio.instance;

        protected virtual void Awake()
        {
            m_defaultColor = pointsText.color;
            m_hasDefaultColor = true;
        }

        /// <summary>
        /// Resets the distributed points to zero and sets the current and base points.
        /// </summary>
        /// <param name="currentPoints">The amount of points on this attribute.</param>
        /// <param name="basePoints">The base value of this attribute.</param>
        public virtual void Reset(int currentPoints, int basePoints)
        {
            distributedPoints = 0;
            this.currentPoints = currentPoints;
            m_basePoints = basePoints;
            m_hasBasePoints = true;

            if (stats.availablePoints > 0)
                addButton.transform.localScale = Vector3.one;

            removeButton.transform.localScale = Vector3.zero;
            pointsText.color = m_defaultColor;
            UpdateText();
        }

        /// <summary>
        /// Increased the points of this attribute.
        /// </summary>
        public virtual void AddPoint()
        {
            distributedPoints += 1;
            stats.availablePoints -= 1;
            removeButton.transform.localScale = Vector3.one;
            m_audio.PlayUiEffect(addPointClip);

            if (stats.availablePoints == 0)
                addButton.transform.localScale = Vector3.zero;

            UpdateText();
            stats.UpdateApplyCancelButtons();
        }


        /// <summary>
        /// Decreases the points of this attribute.
        /// </summary>
        public virtual void SubPoint()
        {
            distributedPoints -= 1;
            stats.availablePoints += 1;
            addButton.transform.localScale = Vector3.one;
            m_audio.PlayUiEffect(removePointClip);

            if (distributedPoints == 0)
                removeButton.transform.localScale = Vector3.zero;

            UpdateText();
            stats.UpdateApplyCancelButtons();
        }

        /// <summary>
        /// Sets the current points of this attribute.
        /// </summary>
        /// <param name="points">The amount of points you want to set.</param>
        public virtual void SetCurrentPoints(int points)
        {
            currentPoints = points;
        }

        /// <summary>
        /// Updates the text showing the total points and adjusts the colour based on the
        /// comparison with the base value.
        /// </summary>
        public virtual void UpdateText()
        {
            pointsText.text = (currentPoints + distributedPoints).ToString();

            if (m_hasBasePoints)
            {
                int total = currentPoints + distributedPoints;
                if (total > m_basePoints)
                {
                    pointsText.color = GameColors.Green;
                }
                else if (total < m_basePoints)
                {
                    pointsText.color = GameColors.LightRed;
                }
                else
                {
                    pointsText.color = m_defaultColor;
                }
            }
        }

        private void AddHoldEvents(EventTrigger trigger, bool isAdd)
        {
            var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDown.callback.AddListener((_) => StartHold(isAdd));
            trigger.triggers.Add(pointerDown);

            var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUp.callback.AddListener((_) => StopHold());
            trigger.triggers.Add(pointerUp);
        }

        private void StartHold(bool isAdd)
        {
            isHolding = true;
            isAddButtonHeld = isAdd;
            ClickOnce();
            holdRoutine = StartCoroutine(HoldLoop());
        }

        private void StopHold()
        {
            isHolding = false;
            if (holdRoutine != null)
            {
                StopCoroutine(holdRoutine);
                holdRoutine = null;
            }
        }

        private void ClickOnce()
        {
            if (isAddButtonHeld)
            {
                if (stats.availablePoints > 0)
                {
                    AddPoint();
                }
                else
                {
                    StopHold();
                }
            }
            else
            {
                if (distributedPoints > 0)
                {
                    SubPoint();
                }
                else
                {
                    StopHold();
                }
            }
        }

        private IEnumerator HoldLoop()
        {
            yield return new WaitForSeconds(0.5f);

            float startTime = Time.time;
            while (isHolding && Time.time - startTime < maxHoldDuration)
            {
                ClickOnce();
                yield return new WaitForSeconds(0.05f);
            }

            isHolding = false;
            holdRoutine = null;
        }

        protected virtual void OnDisable()
        {
            StopHold();
        }

        protected virtual void Start()
        {
            if (!m_hasDefaultColor)
                m_defaultColor = pointsText.color;

            stats.onPointsChanged += (availablePoints) =>
            {
                addButton.transform.localScale = availablePoints > 0 ? Vector3.one : Vector3.zero;
            };

            // addButton.onClick.AddListener(AddPoint);
            // removeButton.onClick.AddListener(SubPoint);

            var addTrigger = addButton.gameObject.AddComponent<EventTrigger>();
            var removeTrigger = removeButton.gameObject.AddComponent<EventTrigger>();

            AddHoldEvents(addTrigger, isAdd: true);
            AddHoldEvents(removeTrigger, isAdd: false);


            addButton.transform.localScale = stats.availablePoints > 0 ? Vector3.one : Vector3.zero;
            removeButton.transform.localScale = distributedPoints > 0 ? Vector3.one : Vector3.zero;

            UpdateText();
        }
    }
}