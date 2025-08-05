using NUnit.Framework;
using UnityEngine;
using PLAYERTWO.ARPGProject;

public class BuffImmunityTests
{
    [Test]
    public void DebuffBlockedWhenAnyImmunityItemPresent()
    {
        var go = new GameObject();
        go.AddComponent<Entity>();
        var stats = go.AddComponent<EntityStatsManager>();
        var inventory = go.AddComponent<EntityInventory>();
        var buffManager = go.AddComponent<EntityBuffManager>();

        var item1 = ScriptableObject.CreateInstance<Item>();
        var item2 = ScriptableObject.CreateInstance<Item>();

        inventory.instance.TryAddItem(new ItemInstance(item1));

        var buff = ScriptableObject.CreateInstance<Buff>();
        buff.isDebuff = true;
        buff.immunityItems = new[] { item1, item2 };
        buff.requireAllItems = false;

        var result = buffManager.AddBuff(buff, true);

        Assert.IsFalse(result);
        Assert.AreEqual(0, buffManager.buffs.Count);

        Object.DestroyImmediate(buff);
        Object.DestroyImmediate(item1);
        Object.DestroyImmediate(item2);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void DebuffBlockedOnlyWhenAllImmunityItemsPresent()
    {
        var item1 = ScriptableObject.CreateInstance<Item>();
        var item2 = ScriptableObject.CreateInstance<Item>();
        var buff = ScriptableObject.CreateInstance<Buff>();
        buff.isDebuff = true;
        buff.immunityItems = new[] { item1, item2 };
        buff.requireAllItems = true;

        // CASE SCENARIO - One item is missing -> debuff applies
        var go = new GameObject();
        go.AddComponent<Entity>();
        var stats = go.AddComponent<EntityStatsManager>();
        var inventory = go.AddComponent<EntityInventory>();
        var buffManager = go.AddComponent<EntityBuffManager>();

        inventory.instance.TryAddItem(new ItemInstance(item1));

        var resultApplied = buffManager.AddBuff(buff, true);
        Assert.IsTrue(resultApplied);
        Assert.AreEqual(1, buffManager.buffs.Count);

        Object.DestroyImmediate(go);

        // CASE SCENARIO - All items are present -> debuff blocked
        go = new GameObject();
        go.AddComponent<Entity>();
        stats = go.AddComponent<EntityStatsManager>();
        inventory = go.AddComponent<EntityInventory>();
        buffManager = go.AddComponent<EntityBuffManager>();

        inventory.instance.TryAddItem(new ItemInstance(item1));
        inventory.instance.TryAddItem(new ItemInstance(item2));

        var resultBlocked = buffManager.AddBuff(buff, true);
        Assert.IsFalse(resultBlocked);
        Assert.AreEqual(0, buffManager.buffs.Count);

        Object.DestroyImmediate(buff);
        Object.DestroyImmediate(item1);
        Object.DestroyImmediate(item2);
        Object.DestroyImmediate(go);
    }
}
