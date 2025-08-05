using NUnit.Framework;
using UnityEngine;
using PLAYERTWO.ARPGProject;
using System.Reflection;

public class BuffRestrictionResistanceTests
{
    [Test]
    public void DebuffIsBlockedWhenRequirementMet()
    {
        var buff = ScriptableObject.CreateInstance<Buff>();
        buff.isDebuff = true;
        buff.strength = -5;
        buff.ignoreIfResistant = new[]
        {
            new ResistanceRequirement { statName = "fireResistance", minimumValue = 10 }
        };

        var go = new GameObject();
        var stats = go.AddComponent<EntityStatsManager>();
        var manager = go.AddComponent<EntityBuffManager>();

        var prop = typeof(EntityStatsManager).GetProperty("fireResistance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        prop.SetValue(stats, 10);

        bool added = manager.AddBuff(buff, true);

        Assert.IsFalse(added);
        Assert.AreEqual(20, stats.strength);
        Assert.IsEmpty(manager.buffs);

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(buff);
    }

    [Test]
    public void DebuffAppliesWhenResistanceBelowRequirement()
    {
        var buff = ScriptableObject.CreateInstance<Buff>();
        buff.isDebuff = true;
        buff.strength = -5;
        buff.ignoreIfResistant = new[]
        {
            new ResistanceRequirement { statName = "fireResistance", minimumValue = 10 }
        };

        var go = new GameObject();
        var stats = go.AddComponent<EntityStatsManager>();
        var manager = go.AddComponent<EntityBuffManager>();

        var prop = typeof(EntityStatsManager).GetProperty("fireResistance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        prop.SetValue(stats, 0);

        bool added = manager.AddBuff(buff, true);

        Assert.IsTrue(added);
        Assert.AreEqual(15, stats.strength);
        Assert.AreEqual(1, manager.buffs.Count);

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(buff);
    }
}
