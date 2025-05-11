using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Entity/Entity Camera")]
    public class EntityCamera : MonoBehaviour
    {
        [Header("Inputs Settings")]
        [Tooltip(
            "The Input Action Asset from the New Input System containing all the possible actions."
        )]
        public InputActionAsset actions;

        [Header("General Settings")]
        [Tooltip("The angle of the Camera.")]
        public float angle = 45f;

        [Tooltip("The minimum distance the Camera can reach from the target.")]
        public float minDistance = 5f;

        [Tooltip("The maximum distance the Camera can reach from the target.")]
        public float maxDistance = 20f;

        [Header("Scroll Settings")]
        [Tooltip("A multiplier value to speed up the zoom scroll speed.")]
        public float zoomScrollMultiplier = 500f;

        [Tooltip("A multiplier value to speed up the rotation scroll speed.")]
        public float rotationScrollMultiplier = 1500f;

        [Tooltip("The time in seconds it takes for the scroll to reach its target value.")]
        public float scrollSmoothTime = 0.1f;

        protected float m_distance;
        protected float m_rotation;
        protected float m_targetDistance;
        protected float m_targetRotation;
        protected float m_distanceVelocity;
        protected float m_rotationVelocity;

        protected bool m_scrollModifier;
        protected bool m_pointerOverUi;

        protected InputAction m_scrollAction;
        protected InputAction m_scrollModifierAction;

        protected Entity m_entity;

        protected virtual void InitializeEntity()
        {
            if (Level.instance == null || Level.instance.player == null)
            {
                Debug.LogError("Level.instance or Level.instance.player is null. Cannot initialize camera entity.");
                return;
            }

            m_entity = Level.instance.player;

            if (m_entity == null)
            {
                Debug.LogError("Failed to assign player to EntityCamera.");
            }
        }

        protected virtual void InitializeActions()
        {
            m_scrollAction = actions["Scroll"];
            m_scrollModifierAction = actions["Scroll Modifier"];
        }

        protected virtual void InitializeActionsCallbacks()
        {
            m_scrollAction.performed += _ => OnScroll();
            m_scrollModifierAction.performed += _ => m_scrollModifier = true;
            m_scrollModifierAction.canceled += _ => m_scrollModifier = false;
        }

        protected virtual void HandleValueSmoothness()
        {
            if (float.IsNaN(m_targetDistance) || float.IsNaN(m_targetRotation))
            {
                Debug.LogError("Target distance or rotation is NaN. Resetting to default values.");
                m_targetDistance = maxDistance;
                m_targetRotation = 0;
            }

            m_distance = Mathf.SmoothDampAngle(
                m_distance,
                m_targetDistance,
                ref m_distanceVelocity,
                scrollSmoothTime
            );
            m_rotation = Mathf.SmoothDampAngle(
                m_rotation,
                m_targetRotation,
                ref m_rotationVelocity,
                scrollSmoothTime
            );

            if (float.IsNaN(m_distance)) m_distance = maxDistance;
            if (float.IsNaN(m_rotation)) m_rotation = 0;
        }

        protected virtual void HandleTransform()
        {
            if (m_entity == null || m_entity.transform == null)
            {
                Debug.LogError("Entity is null or its transform is null. Cannot update camera transform.");
                return;
            }

            var target = m_entity.transform.position;

            if (float.IsNaN(target.x) || float.IsNaN(target.y) || float.IsNaN(target.z))
            {
                Debug.LogError($"Target position is invalid: {target}. Resetting to Vector3.zero.");
                target = Vector3.zero;
            }

            var rotation = Quaternion.Euler(angle, m_rotation, 0);

            var newPosition = rotation * new Vector3(0, 0, -m_distance) + target;
            if (!float.IsNaN(newPosition.x) && !float.IsNaN(newPosition.y) && !float.IsNaN(newPosition.z))
            {
                transform.position = newPosition;
                transform.rotation = rotation;
            }
            else
            {
                Debug.LogError($"New camera position is invalid: {newPosition}. Resetting.");
                transform.position = target + new Vector3(0, 5, -10);
                transform.rotation = Quaternion.Euler(angle, 0, 0);
            }
        }


        protected virtual void HandleMovement()
        {
            HandleValueSmoothness();
            HandleTransform();
        }

        protected virtual void HandlePointer() =>
            m_pointerOverUi = EventSystem.current.IsPointerOverGameObject();

        protected virtual void OnScroll()
        {
            if (m_pointerOverUi)
                return;

            var scroll = m_scrollAction.ReadValue<float>();

            if (m_scrollModifier)
            {
                m_targetRotation = m_rotation - scroll * rotationScrollMultiplier;
            }
            else
            {
                m_targetDistance = m_distance - scroll * zoomScrollMultiplier;
                m_targetDistance = Mathf.Clamp(m_targetDistance, minDistance, maxDistance);
            }
        }

        /// <summary>
        /// Resets the Camera to its initial rotation and distance.
        /// </summary>
        public virtual void Reset()
        {
            if (float.IsNaN(maxDistance) || maxDistance <= 0)
            {
                Debug.LogError("Invalid maxDistance value. Setting to default (20).");
                maxDistance = 20;
            }

            m_rotation = m_targetRotation = 0;
            m_distance = m_targetDistance = maxDistance;
        }

        protected virtual void Start()
        {
            InitializeEntity();
            InitializeActions();
            InitializeActionsCallbacks();
            Reset();
        }

        protected virtual void LateUpdate()
        {
            if (Time.timeScale == 0) return;
            
            HandlePointer();
            HandleMovement();
        }

        protected virtual void OnEnable() => actions.Enable();

        protected virtual void OnDisable() => actions.Disable();
    }
}
