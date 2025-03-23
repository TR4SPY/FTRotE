using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(Entity))]
    [AddComponentMenu("PLAYER TWO/ARPG Project/Entity/Entity Audio")]
    public class EntityAudio : MonoBehaviour
    {
        [Header("Hit Audio Clips")]
        [Tooltip("List of audios to play when the Entity gets hit.")]
        public AudioClip[] hitClips;

        [Tooltip("List of audios to play when the Entity gets a critical hit.")]
        public AudioClip[] criticalHitClips;

        [Header("Death & Attack Clips")]
        [Tooltip("List of audios to play when the Entity dies.")]
        public AudioClip[] dieClips;

        [Tooltip("List of audios to play when the Entity performs a melee attack.")]
        public AudioClip[] meleeAttackClips;

        [Header("Other Clips")]
        [Tooltip("List of audios to play when the Entity have a target assigned.")]
        public AudioClip[] targetSetClips;

        [Tooltip("List of audios to play when the Entity blocks an attack.")]
        public AudioClip[] blockClips;

        [Tooltip("List of audios to play when the Entity gets stunned by an attack.")]
        public AudioClip[] stunClips;

        [Header("Audio Settings")]
        [Tooltip("Cooldown (in seconds) between consecutive audio plays, to prevent spam.")]
        public float m_audioCooldown = 0.3f;

        [Tooltip("Distance limit at which the entity's sound can be heard. Beyond this, no sound will be played.")]
        public float distanceLimit = 30f;

        protected float m_lastAudioTime;
        protected Entity m_entity;
        protected AudioClip m_tempClip;
        protected GameAudio m_audio => GameAudio.instance;

        protected virtual void Awake()
        {
            InitializeEntity();
            InitializeCallbacks();
        }

        protected virtual void InitializeEntity()
        {
            m_entity = GetComponent<Entity>();
        }

        protected virtual void InitializeCallbacks()
        {
            m_entity.onDamage.AddListener((amount, source, critical) => OnDamage(critical));
            m_entity.onPerformAttack.AddListener(OnPerformAttack);
            m_entity.onTargetSet.AddListener(OnTargetSet);
            m_entity.onDie.AddListener(OnDie);
            m_entity.onBlock.AddListener(() => PlayRandomClip(blockClips));
            m_entity.onStunned.AddListener(() => PlayRandomClip(stunClips));
        }

        /// <summary>
        /// Plays a given Audio Clip (with checks for cooldown, distance, and entity enable state).
        /// </summary>
        /// <param name="audioClip">The Audio Clip you want to play.</param>
        public virtual void PlayClip(AudioClip audioClip)
        {
            if (!audioClip) return;

            if (!m_entity.enabled || !gameObject.activeInHierarchy)
                return;

            if (Level.instance != null && Level.instance.player != null)
            {
                float distance = Vector3.Distance(m_entity.position, Level.instance.player.position);
                if (distance > distanceLimit)
                    return;
            }

            if (Time.time < m_lastAudioTime + m_audioCooldown)
                return;

            m_lastAudioTime = Time.time;

            m_audio.PlayEffect(audioClip);
        }

        /// <summary>
        /// Plays a random Audio Clip from an array of Audio Clips (with the same checks inside PlayClip).
        /// </summary>
        /// <param name="clips">The array of Audio Clips to choose a random one from.</param>
        protected void PlayRandomClip(AudioClip[] clips)
        {
            if (TryGetRandomClip(clips, out m_tempClip))
            {
                PlayClip(m_tempClip);
            }
        }

        /// <summary>
        /// Attempts to get a random Audio Clip from a given array.
        /// </summary>
        /// <param name="clips">The array of Audio Clips.</param>
        /// <param name="clip">Returns the randomly chosen clip.</param>
        /// <returns>True if a random clip was found (array not empty), otherwise false.</returns>
        protected bool TryGetRandomClip(AudioClip[] clips, out AudioClip clip)
        {
            clip = null;
            if (clips != null && clips.Length > 0)
            {
                clip = clips[Random.Range(0, clips.Length)];
            }
            return clip != null;
        }

        /// <summary>
        /// Called when the Entity receives damage.
        /// </summary>
        protected virtual void OnDamage(bool critical)
        {
            if (critical) PlayRandomClip(criticalHitClips);
            else PlayRandomClip(hitClips);
        }

        /// <summary>
        /// Called when the Entity performs an attack.
        /// </summary>
        protected virtual void OnPerformAttack(EntityAttackType attackType)
        {
            switch (attackType)
            {
                default:
                    PlayRandomClip(meleeAttackClips);
                    break;
                case EntityAttackType.Weapon:
                    // If entity has an equipped weapon with attack clips
                    PlayRandomClip(m_entity.items.GetWeapon().attackClips);
                    break;
                case EntityAttackType.Skill:
                    // If skill has a special sound
                    PlayClip(m_entity.skills.current?.sound);
                    break;
            }
        }

        /// <summary>
        /// Called when the Entity sets a target.
        /// </summary>
        protected virtual void OnTargetSet()
        {
            if (m_entity.target != null)
                PlayRandomClip(targetSetClips);
        }

        /// <summary>
        /// Called when the Entity dies.
        /// </summary>
        protected virtual void OnDie()
        {
            PlayRandomClip(dieClips);
        }
    }
}
