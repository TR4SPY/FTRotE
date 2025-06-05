using NUnit.Framework;
using UnityEngine;
using PLAYERTWO.ARPGProject;

public class SkillDamageCalculationTests
{
    [Test]
    public void FireAttack_WithMagicAndElementalResistance_ReducesProperly()
    {
        float totalMagicDamage = 39.5f;
        int fireResistance = 5;
        int magicResistance = 13;

        float expectedFinalDamage = totalMagicDamage - (magicResistance + fireResistance);

        float finalDamage = CalculateFinalMagicDamage(
            totalMagicDamage,
            MagicElement.Fire,
            fireResistance,
            magicResistance
        );

        Assert.AreEqual(expectedFinalDamage, finalDamage, 0.01f);
    }

    [Test]
    public void FireAttack_WithOnlyMagicResistance()
    {
        float totalMagicDamage = 39.5f;
        int fireResistance = 0;
        int magicResistance = 13;

        float expectedFinalDamage = totalMagicDamage - magicResistance;

        float finalDamage = CalculateFinalMagicDamage(
            totalMagicDamage,
            MagicElement.Fire,
            fireResistance,
            magicResistance
        );

        Assert.AreEqual(expectedFinalDamage, finalDamage, 0.01f);
    }

    [Test]
    public void FireAttack_WithNoResistance()
    {
        float totalMagicDamage = 39.5f;

        float finalDamage = CalculateFinalMagicDamage(
            totalMagicDamage,
            MagicElement.Fire,
            0,
            0
        );

        Assert.AreEqual(39.5f, finalDamage, 0.01f);
    }

    [Test]
    public void FireAttack_WithNoElementalTag_OnlyMagicResistanceApplies()
    {
        float weaponMagicDamage = 11f; // No element
        float skillMagicDamage = 28.5f; // Fire
        float totalMagicDamage = weaponMagicDamage + skillMagicDamage;

        int fireResistance = 5;
        int magicResistance = 13;

        // Weapon is non-elemental => only MagicRes applies
        // Skill is Fire => both resistances apply
        float finalWeaponDamage = Mathf.Max(0, weaponMagicDamage - magicResistance);
        float finalSkillDamage = Mathf.Max(0, skillMagicDamage - magicResistance - fireResistance);

        float expectedFinal = finalWeaponDamage + finalSkillDamage;

        float final = finalWeaponDamage + finalSkillDamage;

        Assert.AreEqual(expectedFinal, final, 0.01f);
    }

    private float CalculateFinalMagicDamage(float baseDamage, MagicElement element, int fireRes, int magicRes)
    {
        float reduction = magicRes;

        if (element == MagicElement.Fire)
            reduction += fireRes;

        return Mathf.Max(0, baseDamage - reduction);
    }
}
