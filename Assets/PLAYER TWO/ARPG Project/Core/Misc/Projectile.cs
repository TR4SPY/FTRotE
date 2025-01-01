using System.Collections.Generic;
using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("PLAYER TWO/ARPG Project/Misc/Projectile")]
    public class Projectile : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("The maximum distance this Projectile can reach.")]
        public float maxDistance = 15f;

        [Tooltip("The speed at which this Projectile moves.")]
        public float speed = 15f;

        [Header("Ground Settings")]
        [Tooltip("If true, the Projectile will adjust its position relative to the ground.")]
        public bool adjustToGround = true;

        [Tooltip(
            "The minimum distance this Projectile can reach from the ground to start adjusting its position."
        )]
        public float minimumGroundDistance = 1f;

        protected int m_damage;
        protected bool m_critical;
        protected List<string> m_targets;

        protected Vector3 m_origin;

        protected Entity m_entity;
        protected Entity m_otherEntity;
        protected Destructible m_destructible;

        /// <summary>
        /// Sets the damage data for this Projectile.
        /// </summary>
        /// <param name="entity">The Entity casting this Projectile.</param>
        /// <param name="damage">The amount of damage points.</param>
        /// <param name="critical">If true, the Projectile damage will be considered critical.</param>
        /// <param name="target">The list of targets' tags for this Projectile to interact with.</param>
        public virtual void SetDamage(
            Entity entity,
            int damage,
            bool critical,
            List<string> targets
        )
        {
            m_entity = entity;
            m_damage = damage;
            m_critical = critical;
            m_targets = new List<string>(targets);
        }

        protected virtual void Update()
        {
            HandleMovement();
            HandleDistanceCulling();
            HandleGroundDistance();
        }

        protected virtual void OnEnable() => m_origin = transform.position;

        protected virtual void OnTriggerEnter(Collider other)
        {
            HandleEntityAttack(other);
            HandleDestructibleAttack(other);
        }

        protected virtual void HandleEntityAttack(Collider other)
        {
            if (GameTags.InTagList(other, m_targets) && other.TryGetComponent(out m_otherEntity))
            {
                gameObject.SetActive(false);
                m_otherEntity.Damage(m_entity, m_damage, m_critical);
                Destroy(gameObject);
            }
        }

        protected virtual void HandleDestructibleAttack(Collider other)
        {
            if (
                m_entity.CompareTag(GameTags.Player)
                && other.CompareTag(GameTags.Destructible)
                && other.TryGetComponent(out m_destructible)
            )
            {
                m_destructible.Damage(m_damage);
                Destroy(gameObject);
            }
        }

        protected virtual void HandleMovement() =>
            transform.position += speed * Time.deltaTime * transform.forward;

        protected virtual void HandleDistanceCulling()
        {
            if (Vector3.Distance(m_origin, transform.position) >= maxDistance)
                Destroy(gameObject);
        }

        protected virtual void HandleGroundDistance()
        {
            if (!adjustToGround)
                return;

            var start = transform.position;
            var end = start + Vector3.down * minimumGroundDistance;

            if (Physics.Linecast(start, end, out var hit))
            {
                var safeDistance = Vector3.Distance(hit.point, end);
                transform.position += Vector3.up * safeDistance;
            }
        }
    }
}
