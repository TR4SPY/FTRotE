using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using PLAYERTWO.ARPGProject;

public class GUIBuffSlotExpiryFadeTests
{
    [UnityTest]
    public IEnumerator SlotAlphaOscillatesAndResetsOnRemoval()
    {
        var go = new GameObject();
        var slot = go.AddComponent<GUIBuffSlot>();
        var cg = go.AddComponent<CanvasGroup>();
        slot.canvasGroup = cg;

        slot.BeginExpiryFade(9f);
        yield return null;
        Assert.Less(cg.alpha, 1f);

        slot.BeginExpiryFade(0f);
        float elapsed = 0f;
        while (cg.alpha > 0.01f && elapsed < 5f)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        Assert.LessOrEqual(cg.alpha, 0.01f);

        slot.StopExpiryFade();
        Assert.AreEqual(1f, cg.alpha, 0.001f);

        Object.DestroyImmediate(go);
    }
}
