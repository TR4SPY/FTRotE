using NUnit.Framework;
using UnityEngine;
using PLAYERTWO.ARPGProject;

public class BuffRestrictionClassTests
{
    [Test]
    public void BuffAppliesToAllowedClass()
    {
        var buff = ScriptableObject.CreateInstance<Buff>();
        buff.strength = 5;
        buff.allowedClasses = CharacterClassRestrictions.Knight;

        var characterStats = new CharacterStats(1, 10, 0, 0, 0, 0, 0);

        var go = new GameObject("Knight");
        var statsManager = go.AddComponent<EntityStatsManager>();
        var buffManager = go.AddComponent<EntityBuffManager>();
        characterStats.InitializeStats(statsManager);

        bool result = buffManager.AddBuff(buff);

        Assert.IsTrue(result);
        Assert.AreEqual(15, statsManager.strength);

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(buff);
    }

    [Test]
    public void BuffRejectedForDisallowedClass()
    {
        var buff = ScriptableObject.CreateInstance<Buff>();
        buff.strength = 5;
        buff.allowedClasses = CharacterClassRestrictions.Knight;

        var characterStats = new CharacterStats(1, 10, 0, 0, 0, 0, 0);

        var go = new GameObject("Arcanist");
        var statsManager = go.AddComponent<EntityStatsManager>();
        var buffManager = go.AddComponent<EntityBuffManager>();
        characterStats.InitializeStats(statsManager);

        bool result = buffManager.AddBuff(buff);

        Assert.IsFalse(result);
        Assert.AreEqual(10, statsManager.strength);
        Assert.IsEmpty(buffManager.buffs);

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(buff);
    }
}
