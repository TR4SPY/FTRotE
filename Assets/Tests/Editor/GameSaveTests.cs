using NUnit.Framework;
using UnityEngine;
using PLAYERTWO.ARPGProject;
using System.Reflection;
using System.IO;

public class GameSaveTests
{
    [Test]
    public void GetSaveKey_ReturnsCorrectFormat()
    {
        var go = new GameObject();
        var save = go.AddComponent<GameSave>();
        save.fileName = "testfile";
        save.saveVersion = 3;

        var method = typeof(GameSave).GetMethod("GetSaveKey", BindingFlags.NonPublic | BindingFlags.Instance);
        string key = (string)method.Invoke(save, null);
        string prefix = Application.isEditor ? "dev_" : "";
        Assert.AreEqual(prefix + "testfile_3", key);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void GetFilePath_ReturnsPathWithExtension()
    {
        var go = new GameObject();
        var save = go.AddComponent<GameSave>();
        save.fileName = "data";
        save.saveVersion = 1;
        save.mode = GameSave.Mode.JSON;

        var method = typeof(GameSave).GetMethod("GetFilePath", BindingFlags.NonPublic | BindingFlags.Instance);
        string path = (string)method.Invoke(save, null);
        string prefix = Application.isEditor ? "dev_" : "";
        string expected = Path.Combine(Application.persistentDataPath, $"{prefix}data_1.json");
        Assert.AreEqual(expected, path);

        Object.DestroyImmediate(go);
    }
}