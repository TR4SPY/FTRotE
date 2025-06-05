using NUnit.Framework;
using UnityEngine;
using PLAYERTWO.ARPGProject;

public class SkillDamageIntegrationTests
{
    private class TestStatsManager : EntityStatsManager
    {
        public void SetupStats(int baseDamage)
        {
            minDamage = baseDamage;
            maxDamage = baseDamage;
            minMagicDamage = baseDamage;
            maxMagicDamage = baseDamage;
            criticalChance = 0f;
        }

        protected override int CalculateElementResistance(MagicElement element) => 5;
    }

    [Test]
    public void SkillDamage_Reduced_By_ElementalResistance()
    {
        var go = new GameObject();
        var entity = go.AddComponent<Entity>();
        var stats = go.AddComponent<TestStatsManager>();
        stats.SetupStats(10);

        var skill = ScriptableObject.CreateInstance<SkillAttack>();
        skill.minDamage = 10;
        skill.maxDamage = 10;
        skill.damageMode = SkillAttack.DamageMode.Regular;
        skill.element = MagicElement.Fire;

        var damage = stats.GetSkillDamage(skill, skill.element, out var critical);

        Assert.AreEqual(15, damage);
        Assert.IsFalse(critical);
        Object.DestroyImmediate(go);
        Object.DestroyImmediate(skill);
    }
}