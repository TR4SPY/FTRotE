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

        [Tooltip("If true, the projectile is destroyed when it hits one or more targets.")]
        public bool destroyOnHit = true;

        [Header("Ground Settings")]
        public bool adjustToGround = true;
        public float minimumGroundDistance = 1f;

        protected int m_damage;
        protected bool m_critical;
        protected List<string> m_targets;

        protected Vector3 m_origin;
        protected Entity m_entity;
        protected Entity m_otherEntity;
        protected Destructible m_destructible;
        protected HashSet<Entity> m_hitEntities = new();

        public virtual void SetDamage(Entity entity, int damage, bool critical, List<string> targets)
        {
            m_entity = entity;
            m_damage = damage;
            m_critical = critical;
            m_targets = new List<string>(targets);
        }

        public void SetTarget(Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(direction);
        }

        protected virtual void OnEnable()
        {
            m_origin = transform.position;

            if (adjustToGround)
            {
                var start = transform.position;
                var end = start + Vector3.down * minimumGroundDistance;

                if (Physics.Linecast(start, end, out var hit))
                {
                    var safeDistance = Vector3.Distance(hit.point, end);
                    transform.position += Vector3.up * safeDistance;
                }
            }
        }

        protected virtual void Update()
        {
            HandleMovement();
            HandleDistanceCulling();
        }

        protected virtual void HandleMovement()
        {
            transform.position += speed * Time.deltaTime * transform.forward;
        }

        protected virtual void HandleDistanceCulling()
        {
            Vector3 originFlat = new Vector3(m_origin.x, 0, m_origin.z);
            Vector3 currentFlat = new Vector3(transform.position.x, 0, transform.position.z);

            if (Vector3.Distance(originFlat, currentFlat) >= maxDistance)
                Destroy(gameObject);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            bool hitSomething = false;

            if (HandleEntityAttack(other))
                hitSomething = true;

            if (HandleDestructibleAttack(other))
                hitSomething = true;

            if (destroyOnHit && hitSomething)
                Destroy(gameObject);
        }

        protected virtual bool HandleEntityAttack(Collider other)
        {
            if (GameTags.InTagList(other, m_targets) &&
                other.TryGetComponent(out m_otherEntity) &&
                !m_hitEntities.Contains(m_otherEntity))
            {
                m_hitEntities.Add(m_otherEntity);
                m_otherEntity.Damage(m_entity, m_damage, m_critical);
                return true;
            }

            return false;
        }

        protected virtual bool HandleDestructibleAttack(Collider other)
        {
            if (
                m_entity.CompareTag(GameTags.Player) &&
                other.CompareTag(GameTags.Destructible) &&
                other.TryGetComponent(out m_destructible))
            {
                m_destructible.Damage(m_damage);
                return true;
            }

            return false;
        }
    }
}
