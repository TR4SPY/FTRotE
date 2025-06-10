using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Game/Guild Manager")]
    public class GuildManager : Singleton<GuildManager>
    {
        public string currentGuildName;
        public Sprite currentGuildCrest;

        public static void CreateGuild(string name, Sprite crest)
        {
            if (instance != null)
                instance.CreateInternal(name, crest);
        }

        protected virtual void CreateInternal(string name, Sprite crest)
        {
            currentGuildName = name;
            currentGuildCrest = crest;

            if (Game.instance?.currentCharacter != null)
                Game.instance.currentCharacter.guildName = name;

            Debug.Log($"Guild created: {name}");
        }
    }
}
