using NUnit.Framework;
using UnityEngine;
using PLAYERTWO.ARPGProject;
using System.Collections.Generic;

public class GameTests
{
    [Test]
    public void CreateCharacter_AddsCharacterToList()
    {
        var dbGO = new GameObject();
        var database = dbGO.AddComponent<GameDatabase>();
        database.gameData = ScriptableObject.CreateInstance<GameData>();
        var charSO = ScriptableObject.CreateInstance<Character>();
        database.gameData.characters = new List<Character> { charSO };

        var gameGO = new GameObject();
        var game = gameGO.AddComponent<Game>();

        game.CreateCharacter("Hero", 0);

        Assert.AreEqual(1, game.characters.Count);
        Assert.AreEqual("Hero", game.characters[0].name);

        Object.DestroyImmediate(gameGO);
        Object.DestroyImmediate(dbGO);
    }
}
