using UnityEngine;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [CreateAssetMenu(fileName = "New Summon Skill", menuName = "PLAYER TWO/ARPG Project/Skills/Summon Skill")]
    public class SkillSummon : Skill
    {
        [Header("Summoning Settings")]
        [Tooltip("A list of entities' prefabs to be summoned when casting this skill.")]
        public Entity[] entitiesPrefabs;

        [Tooltip("The distance radius from the leader.")]
        public float distanceFromLeader = 3f;

        [Tooltip("Maximum allowed distance before summon is teleported back.")]
        public float maxDistanceFromLeader = 15f;

        [Tooltip("Cooldown (in seconds) before summon can be teleported again.")]
        public float teleportCooldown = 5f;

        protected EntityAI m_tempAI;
        public override GameObject Cast(Entity caster, Vector3 position, Quaternion rotation)
        {
            if (entitiesPrefabs == null || entitiesPrefabs.Length == 0)
                return null;

            ClearSummons(caster);
            SummonEntities(caster, position, rotation);

            return null;
        }
        
        protected virtual void SummonEntities(Entity caster, Vector3 position, Quaternion rotation)
        {
            for (int i = 0; i < entitiesPrefabs.Length; i++)
            {
                var radians = 2 * Mathf.PI / entitiesPrefabs.Length * i;
                var vertical = Mathf.Sin(radians);
                var horizontal = Mathf.Cos(radians);
                var offset = new Vector3(horizontal, 0, vertical) * distanceFromLeader;

                var instance = Instantiate(entitiesPrefabs[i], position + offset, rotation);

                if (instance.TryGetComponent(out m_tempAI))
                {
                    m_tempAI.leader = caster;
                    m_tempAI.leaderOffset = offset;
                    m_tempAI.transform.position = position + offset;
                }

                DifficultyManager.Instance?.ApplyCurrentDifficultyToNewEnemy(instance);

                var follower = instance.gameObject.AddComponent<SummonFollower>();
                follower.leader = caster;
                follower.maxDistance = maxDistanceFromLeader;
                follower.cooldown = teleportCooldown;
                follower.circleRadius = distanceFromLeader;

                if (i == 0)
                    follower.isTeleportLeader = true;

                caster.summonedEntities.Add(instance);
            }
        }

        protected virtual void ClearSummons(Entity caster)
        {
            if (caster.summonedEntities == null)
                return;

            foreach (var entity in caster.summonedEntities)
            {
                if (entity)
                    Destroy(entity.gameObject);
            }

            caster.summonedEntities.Clear();
        }

        private class SummonFollower : MonoBehaviour
        {
            public Entity leader;
            public float maxDistance = 15f;
            public float cooldown = 5f;
            public float circleRadius = 3f;
            public bool isTeleportLeader = false;

            private float lastTeleportTime = -999f;
            private Entity entity;

            void Start()
            {
                entity = GetComponent<Entity>();
            }

            void Update()
            {
                if (!isTeleportLeader || leader == null || entity == null || entity.isDead)
                    return;

                float distance = Vector3.Distance(transform.position, leader.position);
                if (distance > maxDistance && Time.time - lastTeleportTime > cooldown)
                {
                    TeleportAllSummons();
                    lastTeleportTime = Time.time;
                }
            }

            private void TeleportAllSummons()
            {
                var summons = leader.summonedEntities;
                if (summons == null || summons.Count == 0)
                    return;

                int count = summons.Count;
                for (int i = 0; i < count; i++)
                {
                    Entity summon = summons[i];
                    if (summon == null || summon.isDead)
                        continue;

                    float angle = 2 * Mathf.PI / count * i;
                    float x = Mathf.Cos(angle) * circleRadius;
                    float z = Mathf.Sin(angle) * circleRadius;
                    Vector3 offset = new Vector3(x, 0, z);

                    summon.transform.position = leader.position + offset;
                }
            }
        }
    }
}
