using NUnit.Framework;
using UnityEngine;
using PLAYERTWO.ARPGProject;

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
}