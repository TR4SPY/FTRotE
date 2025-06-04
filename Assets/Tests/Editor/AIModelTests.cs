using NUnit.Framework;
using UnityEngine;
using AI_DDA.Assets.Scripts;

public class AIModelTests
{
    [Test]
    public void PredictDifficulty_ReturnsDefaultWhenWorkerMissing()
    {
        var go = new GameObject();
        var model = go.AddComponent<AIModel>();

        float result = model.PredictDifficulty(1f, 1f, 1f, 1f);
        Assert.AreEqual(5f, result);
        Object.DestroyImmediate(go);
    }
}