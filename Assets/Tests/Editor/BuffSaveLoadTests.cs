using NUnit.Framework;
using UnityEngine;
using PLAYERTWO.ARPGProject;

public class BuffSaveLoadTests
{
    [Test]
    public void StatsDoNotAccumulateAfterMultipleSaveLoads()
    {
        var buff = ScriptableObject.CreateInstance<Buff>();
        buff.strength = 5;

        var characterStats = new CharacterStats(1, 10, 0, 0, 0, 0, 0);
        StatsSerializer serializer = null;

        for (int i = 0; i < 3; i++)
        {
            var go = new GameObject();
            var statsManager = go.AddComponent<EntityStatsManager>();
            var buffManager = go.AddComponent<EntityBuffManager>();

            characterStats.InitializeStats(statsManager);
            buffManager.AddBuff(buff);

            Assert.AreEqual(15, statsManager.strength);

            serializer = new StatsSerializer(characterStats, buffManager);

            Object.DestroyImmediate(go);

            characterStats = CharacterStats.CreateFromSerializer(serializer);
        }

        Object.DestroyImmediate(buff);
    }

    [Test]
    public void SessionOnlyBuffNotSerialized()
    {
        var buff = ScriptableObject.CreateInstance<Buff>();
        buff.removeOnLogout = true;

        var go = new GameObject();
        var statsManager = go.AddComponent<EntityStatsManager>();
        var buffManager = go.AddComponent<EntityBuffManager>();
        new CharacterStats(1, 10, 0, 0, 0, 0, 0).InitializeStats(statsManager);

        buffManager.AddBuff(buff);
        Assert.AreEqual(1, buffManager.buffs.Count);

        var serializer = new BuffsSerializer(buffManager);

        Object.DestroyImmediate(go);

        var go2 = new GameObject();
        var statsManager2 = go2.AddComponent<EntityStatsManager>();
        var buffManager2 = go2.AddComponent<EntityBuffManager>();
        new CharacterStats(1, 10, 0, 0, 0, 0, 0).InitializeStats(statsManager2);

        serializer.ApplyTo(buffManager2);

        Assert.AreEqual(0, buffManager2.buffs.Count);

        Object.DestroyImmediate(go2);
        Object.DestroyImmediate(buff);
    }
}
