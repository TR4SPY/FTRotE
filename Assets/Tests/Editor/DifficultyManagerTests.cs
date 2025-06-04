using NUnit.Framework;
using UnityEngine;
using AI_DDA.Assets.Scripts;

public class DifficultyManagerTests
{
    [Test]
    public void GetCurvedStat_ReturnsBaseValueAtPivot()
    {
        var go = new GameObject();
        var dm = go.AddComponent<DifficultyManager>();
        int result = dm.GetCurvedStat(100, 5f);
        Assert.AreEqual(100, result);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void GetCurvedStat_IncreasesAtHighDifficulty()
    {
        var go = new GameObject();
        var dm = go.AddComponent<DifficultyManager>();
        int result = dm.GetCurvedStat(100, 10f);
        Assert.AreEqual(400, result);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void GetCurvedStat_DecreasesAtLowDifficulty()
    {
        var go = new GameObject();
        var dm = go.AddComponent<DifficultyManager>();
        int result = dm.GetCurvedStat(100, 1f);
        Assert.AreEqual(63, result);
        Object.DestroyImmediate(go);
    }
}
