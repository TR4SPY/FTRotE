using NUnit.Framework;
using UnityEngine;
using AI_DDA.Assets.Scripts;
using PLAYERTWO.ARPGProject;
using System.Reflection;

public class PlayerBehaviorLoggerTests
{
    private class DummyEntity : Entity { }

    [Test]
    public void GetActor_ReturnsExpectedStrings()
    {
        var go = new GameObject();
        var logger = go.AddComponent<PlayerBehaviorLogger>();

        var playerGO = new GameObject();
        var player = playerGO.AddComponent<DummyEntity>();
        player.isPlayer = true;
        Assert.AreEqual("Player", logger.GetActor(player));

        var agentGO = new GameObject();
        var agent = agentGO.AddComponent<DummyEntity>();
        agent.isAgent = true;
        Assert.AreEqual("AI Agent", logger.GetActor(agent));

        Assert.AreEqual("Unknown", logger.GetActor(null));

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(playerGO);
        Object.DestroyImmediate(agentGO);
    }

    [Test]
    public void ResetLogs_ResetsFlags()
    {
        var go = new GameObject();
        var logger = go.AddComponent<PlayerBehaviorLogger>();
        logger.difficultyMultiplier = 5;
        var field = typeof(PlayerBehaviorLogger).GetField("difficultyAdjusted", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(logger, true);

        logger.ResetLogs();

        Assert.AreEqual(0, logger.difficultyMultiplier);
        Assert.IsFalse((bool)field.GetValue(logger));

        Object.DestroyImmediate(go);
    }
}