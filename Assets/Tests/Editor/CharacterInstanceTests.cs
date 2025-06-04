using NUnit.Framework;
using UnityEngine;
using PLAYERTWO.ARPGProject;

public class CharacterInstanceTests
{
    [Test]
    public void GetSetMultiplier_ReturnsExpectedValues()
    {
        var ci = new CharacterInstance();
        ci.SetMultiplier("Strength", 2f);
        Assert.AreEqual(2f, ci.GetMultiplier("Strength"));
        Assert.AreEqual(1f, ci.GetMultiplier("Dexterity"));
    }

    [Test]
    public void DialogChoice_ReturnsNextPage()
    {
        var ci = new CharacterInstance();
        ci.SetDialogPathChoice(1, 5);
        Assert.AreEqual(5, ci.GetDialogNextPage(1));
        Assert.AreEqual(-1, ci.GetDialogNextPage(2));
    }
}
