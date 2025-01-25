//  ZMODYFIKOWANO 31 GRUDNIA 2024

using UnityEngine;
using UnityEngine.Events;
using AI_DDA.Assets.Scripts; // Dodano, aby użyć PlayerBehaviorLogger

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("PLAYER TWO/ARPG Project/Misc/Waypoint")]
    public class Waypoint : MonoBehaviour
    {
        public UnityEvent onActive;

        [Header("Waypoint Settings")]
        public int waypointID = -1; // Unikalne ID Waypoint
        public bool autoActive = true;
        public float activationRadius = 10f;
        public string title = "New Waypoint";

        protected float m_startTime;
        protected bool m_active = false;
        protected Collider m_collider;

        protected const float k_triggerActivationDelay = 0.1f;
        protected static int globalWaypointID = 0;

        protected LevelRespawner m_respawner => LevelRespawner.instance;
        protected LevelWaypoints m_waypoints => LevelWaypoints.instance;

        public bool active
        {
            get { return m_active; }
            set
            {
                if (!m_active && value)
                {
                    onActive.Invoke();
                    LogWaypointDiscovery();
                }
                m_active = value;
            }
        }

        protected Entity m_player => Level.instance.player;

        public virtual SpacePoint GetSpacePoint()
        {
            var position = transform.position;
            position += Vector3.up * m_player.controller.height * 0.5f;
            return new(position, transform.rotation);
        }

        protected virtual void Start()
        {
            m_collider = GetComponent<Collider>();
            m_collider.isTrigger = true;
            m_startTime = Time.time;

            if (waypointID == -1)
            {
                waypointID = globalWaypointID++;
                Debug.Log($"Assigned unique ID {waypointID} to Waypoint '{name}'");
            }
        }

        protected virtual void Update()
        {
            var distance = Vector3.Distance(m_player.position, transform.position);

            if (distance <= activationRadius)
            {
                m_waypoints.currentWaypoint = this;

                if (!active && autoActive)
                    active = true;
            }
        }

        protected virtual bool CanTrigger() => active && !m_waypoints.traveling &&
            !m_respawner.isRespawning && Time.time > k_triggerActivationDelay;

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!CanTrigger() || !other.CompareTag(GameTags.Player))
                return;

            m_player.StandStill();
            m_player.inputs.LockMoveDirection();
            m_waypoints.currentWaypoint = this;
            GUIWindowsManager.instance.waypointsWindow.Show();
        }

        protected void LogWaypointDiscovery(Collider other = null)
        {
            // Domyślnie ustaw interaktor na gracza
            Entity interactor = m_player;

            // Jeśli przekazano Collider, sprawdź, czy to gracz lub AI Agent
            if (other != null)
            {
                bool isPlayer = other.CompareTag(GameTags.Player);
                bool isAI = other.GetComponent<AgentController>()?.isAI == true;

                if (!isPlayer && !isAI)
                {
                    Debug.LogWarning($"Waypoint discovery ignored. {other.name} is neither Player nor AI Agent.");
                    return;
                }

                // Pobierz Entity na podstawie Collidera
                interactor = other.GetComponent<Entity>();
                if (interactor == null)
                {
                    Debug.LogWarning($"Entity is null for {other.name}. Cannot log waypoint discovery.");
                    return;
                }
            }

            // Logowanie odkrycia waypointu
            if (PlayerBehaviorLogger.Instance != null && interactor != null)
            {
                PlayerBehaviorLogger.Instance.LogWaypointDiscovery(interactor, waypointID);
                Debug.Log($"Waypoint '{title}' (ID: {waypointID}) discovered and logged by {interactor.name}.");
            }
            else if (interactor == null)
            {
                Debug.LogWarning("Interactor entity is null. Cannot log waypoint discovery.");
            }
            else
            {
                Debug.LogWarning("PlayerBehaviorLogger.Instance is null. Cannot log waypoint discovery.");
            }
        }
    }
}
