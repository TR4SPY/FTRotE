using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using PLAYERTWO.ARPGProject;

public class BuffRestrictionSceneTests
{
    private class DummyLevel : Level
    {
        protected override void Initialize() { }
    }

    [Test]
    public void SceneRestricted()
    {
        var sceneA = SceneManager.CreateScene("SceneA");
        SceneManager.SetActiveScene(sceneA);

        var levelGO = new GameObject("Level");
        Object.DontDestroyOnLoad(levelGO);
        levelGO.AddComponent<DummyLevel>();

        var go = new GameObject("Entity");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<EntityStatsManager>();
        var buffManager = go.AddComponent<EntityBuffManager>();

        var buff = ScriptableObject.CreateInstance<Buff>();
        buff.allowedScenes = new[] { "SceneA" };

        Assert.IsTrue(buffManager.AddBuff(buff));
        Assert.AreEqual(1, buffManager.buffs.Count);

        var sceneB = SceneManager.CreateScene("SceneB");
        SceneManager.SetActiveScene(sceneB);

        Assert.AreEqual(0, buffManager.buffs.Count);

        Object.DestroyImmediate(buff);
        Object.DestroyImmediate(go);
        Object.DestroyImmediate(levelGO);
    }
}
