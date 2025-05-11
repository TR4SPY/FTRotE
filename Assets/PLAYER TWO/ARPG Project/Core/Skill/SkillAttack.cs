using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [CreateAssetMenu(fileName = "New Skill Attack", menuName = "PLAYER TWO/ARPG Project/Skills/Skill Attack")]
    public class SkillAttack : Skill
    {
        public enum DamageMode { Regular, Magic }

        public enum RequiredWeapon { None, Blade, Bow }

        [Header("Attack Settings")]
        [Tooltip("If true, the Skill will active the regular melee hitbox when casted.")]
        public bool useMeleeHitbox;

        [Tooltip("The minimum distance to a target to use this Skill.")]
        public float minAttackDistance;

        [Tooltip("The required equipped weapon type to perform the Skill.")]
        public RequiredWeapon requiredWeapon;

        [Header("Damage Settings")]
        [Tooltip("The damage mode of this Skill.")]
        public DamageMode damageMode = DamageMode.Magic;

        [Tooltip("The minimum damage this Skill can cause.")]
        public int minDamage;

        [Tooltip("The maximum damage this Skill can cause.")]
        public int maxDamage;

        [Header("Projectile Settings")]
        [Tooltip("If true, projectile or particle cast by this skill is destroyed on hit/collision.")]
        public bool destroyOnHit = true;
        public bool particleDestroyOnCollision = true;
        public bool destroyOnFirstParticleCollision = false;
        public bool particleCollideOnce = false;


        /// <summary>
        /// Returns a random value between the minimum and maximum damage of this Skill.
        /// </summary>
        public virtual int GetDamage(Entity caster)
        {
            int baseDamage = Random.Range(minDamage, maxDamage);

            float skillScaling = 1.1f;
            float energyScaling = 0f;
            float magicWeaponBonus = 0f;

            if (damageMode == DamageMode.Magic)
            {
                energyScaling = caster.stats.energy * 0.4f;
                magicWeaponBonus = (caster.stats.minMagicDamage + caster.stats.maxMagicDamage) * 0.2f;
            }

            float totalDamage = (baseDamage + energyScaling + magicWeaponBonus) * skillScaling;
            return Mathf.RoundToInt(totalDamage);
        }
    }
}

