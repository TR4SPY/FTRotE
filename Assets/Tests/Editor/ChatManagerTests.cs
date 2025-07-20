using NUnit.Framework;
using UnityEngine;
using PLAYERTWO.ARPGProject;
using AI_DDA.Assets.Scripts;

public class ChatManagerTests
{
    [Test]
    public void SubmitMessage_AddsToLog()
    {
        var gameGO = new GameObject();
        gameGO.AddComponent<Game>();

        var go = new GameObject();
        var chat = go.AddComponent<ChatManager>();
        chat.SubmitMessage("hello");

        Assert.AreEqual(1, chat.GetLog().Count);

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(gameGO);
    }

    [Test]
    public void PauseAndResumeCommands_ChangeTimeScale()
    {
        Time.timeScale = 1f;

        var gameGO = new GameObject();
        gameGO.AddComponent<Game>();
        var pauseGO = new GameObject();
        pauseGO.AddComponent<GamePause>();

        var go = new GameObject();
        var chat = go.AddComponent<ChatManager>();

        chat.SubmitMessage("/pause");
        Assert.AreEqual(0f, Time.timeScale);

        chat.SubmitMessage("/resume");
        Assert.AreEqual(1f, Time.timeScale);

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(pauseGO);
        Object.DestroyImmediate(gameGO);
    }


    [Test]
    public void DdaToggle_FlipsUseAIDDA()
    {
        var gameGO = new GameObject();
        gameGO.AddComponent<Game>();

        var dmGO = new GameObject();
        var dm = dmGO.AddComponent<DifficultyManager>();

        var go = new GameObject();
        var chat = go.AddComponent<ChatManager>();

        bool initial = dm.useAIDDA;
        chat.SubmitMessage("/dda toggle");
        Assert.AreEqual(!initial, dm.useAIDDA);

        chat.SubmitMessage("/dda toggle");
        Assert.AreEqual(initial, dm.useAIDDA);

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(dmGO);
        Object.DestroyImmediate(gameGO);
        DifficultyManager.Instance = null;
    }
    
    [Test]
    public void BuildStatsLines_ReturnsValues()
    {
        var statsGO = new GameObject();
        var stats = statsGO.AddComponent<EntityStatsManager>();

        stats.level = 2;
        stats.strength = 3;
        stats.dexterity = 4;
        stats.vitality = 5;
        stats.energy = 6;
        stats.health = 40;
        stats.mana = 20;
        typeof(EntityStatsManager).GetProperty("maxHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(stats, 50);
        typeof(EntityStatsManager).GetProperty("maxMana", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(stats, 30);
        typeof(EntityStatsManager).GetProperty("nextLevelExp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(stats, 100);
        typeof(EntityStatsManager).GetProperty("minDamage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(stats, 1);
        typeof(EntityStatsManager).GetProperty("maxDamage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(stats, 3);

        var lines = ChatManager.BuildStatsLines(stats);

        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[0].Contains("Level"));

        Object.DestroyImmediate(statsGO);
    }
}