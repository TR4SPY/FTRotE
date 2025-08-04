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
}
