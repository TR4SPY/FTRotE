using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Game/Guild Manager")]
    public class GuildManager : Singleton<GuildManager>
    {
        public static void CreateGuild(string name, Sprite crest)
        {
            instance?.CreateInternal(name, crest);
        }

        public static string GetCurrentGuildName()
        {
            return Game.instance?.currentCharacter?.guildName;
        }

        public static Sprite GetCurrentGuildCrest()
        {
            return Game.instance?.currentCharacter?.GetGuildCrest();
        }

        protected virtual void CreateInternal(string name, Sprite crest)
        {
            var character = Game.instance?.currentCharacter;
            if (character != null)
            {
                character.SetGuild(name, crest);
                if (character.Entity?.nametag != null)
                {
                    character.Entity.nametag.SetNametag(character.name, character.stats.currentLevel, name, character.GetName());
                }
            }

            Debug.Log($"Guild created: {name}");
        }
    }
}
