using UnityEngine;
using UnityEngine.UI;

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

        /// <summary>
        /// Resets the distributed points to zero and sets the current points.
        /// </summary>
        /// <param name="currentPoints">The amount of points on this attribute.</param>
        public virtual void Reset(int currentPoints)
        {
            distributedPoints = 0;
            this.currentPoints = currentPoints;

            if (stats.availablePoints > 0)
                addButton.transform.localScale = Vector3.one;

            removeButton.transform.localScale = Vector3.zero;
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

            if (this.stats.availablePoints == 0)
                addButton.transform.localScale = Vector3.zero;

            UpdateText();
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
        /// Updates the text showing the total points.
        /// </summary>
        public virtual void UpdateText()
        {
            pointsText.text = (currentPoints + distributedPoints).ToString();
        }

        protected virtual void Start()
        {
            stats.onPointsChanged += (availablePoints) =>
            {
                addButton.transform.localScale = availablePoints > 0 ? Vector3.one : Vector3.zero;
            };

            addButton.onClick.AddListener(AddPoint);
            removeButton.onClick.AddListener(SubPoint);

            addButton.transform.localScale = stats.availablePoints > 0 ? Vector3.one : Vector3.zero;
            removeButton.transform.localScale = distributedPoints > 0 ? Vector3.one : Vector3.zero;

            UpdateText();
        }
    }
}