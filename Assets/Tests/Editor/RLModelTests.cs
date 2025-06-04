using NUnit.Framework;
using UnityEngine;
using AI_DDA.Assets.Scripts;

public class RLModelTests
{
    [Test]
    public void SetCurrentDifficulty_ClampsHighValue()
    {
        var go = new GameObject();
        var model = go.AddComponent<RLModel>();
        model.SetCurrentDifficulty(15f);
        Assert.AreEqual(10f, model.GetCurrentDifficulty());
        Object.DestroyImmediate(go);
    }

    [Test]
    public void SetCurrentDifficulty_ClampsLowValue()
    {
        var go = new GameObject();
        var model = go.AddComponent<RLModel>();
        model.SetCurrentDifficulty(0f);
        Assert.AreEqual(1f, model.GetCurrentDifficulty());
        Object.DestroyImmediate(go);
    }
}