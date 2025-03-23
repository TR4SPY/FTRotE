using System;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("PLAYER TWO/ARPG Project/Misc/Interactive")]
    public class Interactive : MonoBehaviour
    {
        public UnityEvent onInteract;

        [Header("Interaction Settings")]
        [Tooltip("If true, this Interactive will get disabled after interacting the first time.")]
        public bool interactOnce;

        [Tooltip("If true, the Game Object is disabled after interacting.")]
        public bool disableOnInteract;

        [Tooltip("If true, this Interactive Game Object can be interacted with by AI Agent.")]
        public bool CanAgentInteract = false;

        [Tooltip("A reference to the Animator component of the Interactive object.")]
        public Animator animator;

        [Tooltip("The Audio Clip that plays when interacting with this Interactive.")]
        public AudioClip interactClip;

        protected Collider m_collider;

        [Header("NPC Unique ID")]
        [Tooltip("Unique identifier for each NPC")]
        [SerializeField] private string uniqueNPCID;

        /// <summary>
        /// Returns true if it's possible to interact with this Interactive.
        /// </summary>
        public bool interactive { get; set; } = true;

        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(uniqueNPCID))
            {
                uniqueNPCID = Game.instance.GetNPCIDForName(gameObject.name);
            }
        }

        public string GetNPCID()
        {
            return uniqueNPCID;
        }

        protected virtual void InitializeCollider()
        {
            m_collider = GetComponent<Collider>();
            m_collider.isTrigger = true;
        }

        protected virtual void InitializeState()
        {
            if (interactive) return;

            HandleState();
            PlayAnimation();
        }

        protected virtual void InitializeTag() =>
            gameObject.tag = GameTags.Interactive;

        protected virtual void PlayAnimation()
        {
            if (!animator) return;

            animator.SetTrigger("OnInteract");
        }

        protected virtual void PlayAudioClip()
        {
            if (!interactClip) return;

            GameAudio.instance.PlayEffect(interactClip);
        }

        protected virtual void HandleState()
        {
            if (interactOnce)
                DisableInteraction();

            if (disableOnInteract)
                gameObject.SetActive(false);
        }

        protected virtual void OnInteract(object other) { }

        /// <summary>
        /// Interacts with this Interactive.
        /// </summary>
        /// <param name="other">The object that is interacting with this.</param>
        public virtual void Interact(object other = null)
        {
            if (interactive)
            {
                HandleState();
                PlayAnimation();
                PlayAudioClip();
                OnInteract(other);
                onInteract.Invoke();

                // Debug.Log($"NPC Interacted: {name}, ID: {uniqueNPCID}");
            }
        }

        /// <summary>
        /// Disables the interaction with this Interactive.
        /// </summary>
        public virtual void DisableInteraction()
        {
            interactive = false;
            m_collider.enabled = false;
        }

        protected virtual void Start()
        {
            InitializeCollider();
            InitializeState();
            InitializeTag();
        }
    }
}
