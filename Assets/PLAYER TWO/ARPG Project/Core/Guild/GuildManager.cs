using UnityEngine;
using TMPro;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Game/Guild Manager")]
    public class GuildManager : Singleton<GuildManager>
    {
        [Header("Creation Cost")]
        [SerializeField] int createCostSolmire = 5;
        [SerializeField] int createCostLunaris = 20;
        [SerializeField] int createCostAmberlings = 999;

        [Header("Rebrand Cost")]
        [SerializeField] int rebrandCostSolmire = 1;
        [SerializeField] int rebrandCostLunaris = 5;
        [SerializeField] int rebrandCostAmberlings = 100;

        [Header("Transfer Cost")]
        [SerializeField] int transferCostSolmire = 2;
        [SerializeField] int transferCostLunaris = 10;
        [SerializeField] int transferCostAmberlings = 250;

        [Header("Guild Settings")]
        [SerializeField] int maxGuildsAllowed = 1;
        [SerializeField] bool guildWarsAllowed = false;
        [SerializeField] bool castleBattlesAllowed = false;

        public int GetRebrandCostSolmire() => rebrandCostSolmire;
        public int GetRebrandCostLunaris() => rebrandCostLunaris;
        public int GetRebrandCostAmberlings() => rebrandCostAmberlings;

        public int GetTransferCostSolmire() => transferCostSolmire;
        public int GetTransferCostLunaris() => transferCostLunaris;
        public int GetTransferCostAmberlings() => transferCostAmberlings;

        public int GetMaxGuildsAllowed() => maxGuildsAllowed;
        public bool GetGuildWarsAllowed() => guildWarsAllowed;
        public bool GetCastleBattlesAllowed() => castleBattlesAllowed;

        public int GetCreateCostInAmberlings() =>
            Currency.ConvertToAmberlings(createCostAmberlings, CurrencyType.Amberlings)
          + Currency.ConvertToAmberlings(createCostLunaris, CurrencyType.Lunaris)
          + Currency.ConvertToAmberlings(createCostSolmire, CurrencyType.Solmire);
          
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