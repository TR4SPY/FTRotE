using NUnit.Framework;
using UnityEngine;
using AI_DDA.Assets.Scripts;
using PLAYERTWO.ARPGProject;
using System.Reflection;

public class AchievementManagerTests
{
    private class TestLogger : PlayerBehaviorLogger { }

    [Test]
    public void AchievementCondition_FirstBlood_ReturnsTrue()
    {
        var loggerGO = new GameObject();
        var logger = loggerGO.AddComponent<TestLogger>();
        logger.enemiesDefeated = 1;

        var charInstance = new CharacterInstance();
        charInstance.stats = new CharacterStats(1,1,1,1,1,0,0);

        var managerGO = new GameObject();
        var manager = managerGO.AddComponent<AchievementManager>();
        var method = typeof(AchievementManager).GetMethod("AchievementConditionMet", BindingFlags.NonPublic | BindingFlags.Instance);
        bool result = (bool)method.Invoke(manager, new object[]{"First Blood", logger, charInstance});

        Assert.IsTrue(result);

        Object.DestroyImmediate(loggerGO);
        Object.DestroyImmediate(managerGO);
    }
}
