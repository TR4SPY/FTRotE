using NUnit.Framework;
using UnityEngine;
using PLAYERTWO.ARPGProject;

public class EntityTests
{
    [Test]
    public void AddAndRemoveAttackedBy_ModifiesList()
    {
        var go1 = new GameObject();
        var e1 = go1.AddComponent<Entity>();
        var go2 = new GameObject();
        var e2 = go2.AddComponent<Entity>();

        e1.AddAttackedBy(e2);
        Assert.Contains(e2, e1.attackedBy);

        e1.RemoveAttackedBy(e2);
        Assert.IsFalse(e1.attackedBy.Contains(e2));

        Object.DestroyImmediate(go1);
        Object.DestroyImmediate(go2);
    }
}