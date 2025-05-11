using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Waypoint")]
    public class GUIWaypoint : MonoBehaviour
    {
        [Tooltip("A reference to the Text component used as the Waypoint title.")]
        public Text title;

        /// <summary>
        /// The Waypoint this GUI Waypoint represents.
        /// </summary>
        public Waypoint waypoint { get; protected set; }

        /// <summary>
        /// The Button component of this GUI Waypoint.
        /// </summary>
        public Button button { get; protected set; }

        protected virtual void InitializeButton()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        /// <summary>
        /// Sets the Waypoint this GUI Waypoint represents.
        /// </summary>
        /// <param name="waypoint">The Waypoint you want to set.</param>
        public virtual void SetWaypoint(Waypoint waypoint)
        {
            if (this.waypoint == waypoint) return;

            this.waypoint = waypoint;
            title.text = this.waypoint.title;
        }

        protected virtual void OnClick()
        {
            if (!waypoint) return;

            LevelWaypoints.instance.TravelTo(waypoint);
        }

        protected virtual void Awake() => InitializeButton();
    }
}
