using System;

namespace PLAYERTWO.ARPGProject
{
    [Serializable]
    public struct SkillAugment
    {
        public Skill skill;
        public float cooldownMultiplier;
        public float damageMultiplier;
    }
}
