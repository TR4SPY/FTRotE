using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public class SkillInstance
    {
        /// <summary>
        /// Returns the scriptable object that represents this Skill Instance.
        /// </summary>
        public Skill data { get; protected set; }

        protected float m_lastPerformTime;
        protected float m_coolDownMultiplier = 1f;
        protected float m_damageMultiplier = 1f;

        public SkillInstance(Skill data)
        {
            this.data = data;
        }

        /// <summary>
        /// Performs the Skill.
        /// </summary>
        public virtual void Perform() => m_lastPerformTime = Time.time;

        /// <summary>
        /// Returns true if this Skill can be performed on this frame.
        /// </summary>
        public virtual bool CanPerform() =>
            m_lastPerformTime == 0 ||
            Time.time >= m_lastPerformTime + GetCoolDown();

        /// <summary>
        /// Apply a skill augment to this instance.
        /// </summary>
        public void ApplyAugment(SkillAugment augment)
        {
            if (augment.cooldownMultiplier != 0f)
                m_coolDownMultiplier *= augment.cooldownMultiplier;
            if (augment.damageMultiplier != 0f)
                m_damageMultiplier *= augment.damageMultiplier;
        }

        /// <summary>
        /// Reset any applied augments.
        /// </summary>
        public void ResetAugments()
        {
            m_coolDownMultiplier = 1f;
            m_damageMultiplier = 1f;
        }

        /// <summary>
        /// Get current cooldown considering augments.
        /// </summary>
        public float GetCoolDown() => data.coolDown * m_coolDownMultiplier;

        /// <summary>
        /// Get adjusted damage based on augments.
        /// </summary>
        /// <param name="baseDamage">Base damage calculated elsewhere.</param>
        public int GetAugmentedDamage(int baseDamage) =>
            Mathf.RoundToInt(baseDamage * m_damageMultiplier);
    }
}
