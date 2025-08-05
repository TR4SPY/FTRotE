using NUnit.Framework;
using UnityEngine;
using PLAYERTWO.ARPGProject;

public class BuffRestrictionIncompatibilityTests
{
    [Test]
    public void AddBuff_FailsWhenConflictingBuffActive()
    {
        var buffA = ScriptableObject.CreateInstance<Buff>();
        buffA.strength = 5;

        var buffB = ScriptableObject.CreateInstance<Buff>();
        buffB.strength = 10;
        buffB.incompatibleBuffs = new[] { buffA };

        var go = new GameObject();
        var statsManager = go.AddComponent<EntityStatsManager>();
        var buffManager = go.AddComponent<EntityBuffManager>();
        var characterStats = new CharacterStats(1, 0, 0, 0, 0, 0, 0);
        characterStats.InitializeStats(statsManager);

        Assert.IsTrue(buffManager.AddBuff(buffA));
        Assert.AreEqual(5, statsManager.strength);

        Assert.IsFalse(buffManager.AddBuff(buffB));
        Assert.AreEqual(5, statsManager.strength);
        Assert.AreEqual(1, buffManager.buffs.Count);

        Object.DestroyImmediate(buffA);
        Object.DestroyImmediate(buffB);
        Object.DestroyImmediate(go);
    }
}
