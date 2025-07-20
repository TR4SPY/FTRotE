using UnityEngine;
using TMPro;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Game/Guild Manager")]
    public class GuildManager : Singleton<GuildManager>
    {
        public static void CreateGuild(string name, Sprite crest, TMP_SpriteAsset crestAsset)
        {
            instance?.CreateInternal(name, crest, crestAsset);
        }

        public static void DeleteGuild()
        {
            instance?.DeleteInternal();
        }

        public static string GetCurrentGuildName()
        {
            return Game.instance?.currentCharacter?.guildName;
        }

        public static Sprite GetCurrentGuildCrest()
        {
            return Game.instance?.currentCharacter?.GetGuildCrest();
        }

        protected virtual void CreateInternal(string name, Sprite crest, TMP_SpriteAsset crestAsset)
        {
            var character = Game.instance?.currentCharacter;
            if (character != null)
            {
                character.SetGuild(name, crest, crestAsset);
                if (character.Entity?.nametag != null)
                {
                    character.Entity.nametag.SetNametag(character.name, character.stats.currentLevel, name, character.GetName());
                }
            }

            Debug.Log($"Guild created: {name}");
        }

        protected virtual void DeleteInternal()
        {
            var character = Game.instance?.currentCharacter;
            if (character != null)
            {
                string guild = character.guildName;
                character.SetGuild(string.Empty, null, null);
                if (character.Entity?.nametag != null)
                {
                    character.Entity.nametag.SetNametag(character.name, character.stats.currentLevel, string.Empty, character.GetName());
                }

                Debug.Log($"Guild deleted: {guild}");
            }
        }
    }
}
