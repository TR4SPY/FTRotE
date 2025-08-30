using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using AI_DDA.Assets.Scripts;
using AI_DDA.Assets.Scripts.Achievements;
using PLAYERTWO.ARPGProject;

public class AchievementManagerTests
{
    private class TestLogger : PlayerBehaviorLogger { }

    [Test]
    public void FirstBlood_AchievementUnlocks_FromAsset()
    {
        var achievement = AssetDatabase.LoadAssetAtPath<AchievementData>("Assets/Achievements/FirstBlood.asset");
        Assert.IsNotNull(achievement);

        var loggerGO = new GameObject();
        var logger = loggerGO.AddComponent<TestLogger>();
        logger.enemiesDefeated = 1;

        var charInstance = new CharacterInstance();
        charInstance.stats = new CharacterStats(1,1,1,1,1,0,0);

        var managerGO = new GameObject();
        var manager = managerGO.AddComponent<AchievementManager>();
        manager.achievements = new List<AchievementData> { achievement };

        manager.CheckAchievements(logger);

        Assert.Contains("first_blood", logger.unlockedAchievements);

        Object.DestroyImmediate(loggerGO);
        Object.DestroyImmediate(managerGO);
    }
}
