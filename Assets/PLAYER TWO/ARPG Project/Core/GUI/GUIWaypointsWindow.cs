using UnityEngine;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Waypoints Window")]
    public class GUIWaypointsWindow : MonoBehaviour
    {
        [Tooltip("A reference to the transform used as the waypoints container.")]
        public RectTransform waypointsContainer;

        [Tooltip("The prefab you want to use as Waypoints.")]
        public GUIWaypoint waypointPrefab;

        protected List<GUIWaypoint> m_buttons = new();

        protected LevelWaypoints m_levelWaypoints => LevelWaypoints.instance;

        protected virtual void Awake()
        {
            foreach (var waypoint in m_levelWaypoints.waypoints)
            {
                var button = Instantiate(waypointPrefab, waypointsContainer);
                button.SetWaypoint(waypoint);
                m_buttons.Add(button);
            }
        }

        protected virtual void OnEnable()
        {
            for (int i = 0; i < m_buttons.Count; i++)
            {
                var state = m_buttons[i].waypoint.active;
                var interactable = state && !IsCurrentWaypoint(m_buttons[i]);

                m_buttons[i].gameObject.SetActive(state);
                m_buttons[i].button.interactable = interactable;
            }
        }

        protected bool IsCurrentWaypoint(GUIWaypoint button) =>
            button && button.waypoint == m_levelWaypoints.currentWaypoint;
    }
}
