using NUnit.Framework;
using UnityEngine;
using PLAYERTWO.ARPGProject;
using System.Collections.Generic;

public class ItemElementsTests
{
    private class DummyEquip : ItemEquippable { }

    [Test]
    public void ModifyResistance_IncreasesAndDecreases()
    {
        var elements = new ItemElements();
        elements.ModifyResistance(MagicElement.Fire, 2);
        Assert.AreEqual(2, elements.fireResistance);
        elements.ModifyResistance(MagicElement.Fire, -2);
        Assert.AreEqual(0, elements.fireResistance);
    }

    [Test]
    public void StoneOfElementsRule_MatchesOnlyWithAllStones()
    {
        var rule = new StoneOfElementsRule();
        var names = new[] {
            "Fire Stone", "Water Stone", "Ice Stone", "Earth Stone", "Air Stone",
            "Lightning Stone", "Shadow Stone", "Stone of Light", "Arcane Stone"
        };

        var items = new List<ItemInstance>();
        foreach (var n in names)
        {
            var jewel = ScriptableObject.CreateInstance<ItemJewel>();
            jewel.name = n;
            items.Add(new ItemInstance(jewel, false, false));
        }

        Assert.IsTrue(rule.Matches(items));

        items.RemoveAt(0);
        Assert.IsFalse(rule.Matches(items));
    }
}

