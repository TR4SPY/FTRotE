using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PLAYERTWO.ARPGProject
{
    public enum Affinity
    {
        None,
        Light,
        Dark,
        Master
    }

    public static class AffinityNaming
    {
        public static readonly Dictionary<Affinity, string[]> TierNames = new()
        {
            { Affinity.Light,   new[] { "Light", "Holy", "Divine" } },
            { Affinity.Dark,    new[] { "Dark", "Evil", "Forsaken" } },
            { Affinity.Master,  new[] { "Master", "Master", "Master" } }
            // None → No prefix
        };
    }

    [System.Flags]
    public enum CharacterClassRestrictions
    {
        None           = 0,

        Knight         = 1 << 0,   // 1
        Vanguard       = 1 << 1,   // 2
        Warlord        = 1 << 2,   // 4

        Arcanist       = 1 << 3,   // 8
        Spellbinder    = 1 << 4,   // 16
        Clairvoyant    = 1 << 5,   // 32

        Ranger         = 1 << 6,   // 64
        Wildstrider    = 1 << 7,   // 128
        Shadowstalker  = 1 << 8,   // 256

        Zealot         = 1 << 9,   // 512
        Acolyte        = 1 << 10,  // 1024
        Inquisitor     = 1 << 11,  // 2048

        Reaver         = 1 << 12,  // 4096
        Slayer         = 1 << 13,  // 8192
        Nightblade     = 1 << 14,  // 16384

        Summoner       = 1 << 15,  // 32768
        Archvesper     = 1 << 16,  // 65536
        Exanimar       = 1 << 17,  // 131072
    }

    [System.Serializable]
    public class ClassFamily
    {
        public string FamilyName;
        public CharacterClassRestrictions[] Tiers;
    }

    public static class ClassHierarchy
    {
        public static ClassFamily[] Families =
        {
            new ClassFamily
            {
                FamilyName = "Knight",
                Tiers = new []
                {
                    CharacterClassRestrictions.Knight,
                    CharacterClassRestrictions.Vanguard,
                    CharacterClassRestrictions.Warlord
                }
            },
            new ClassFamily
            {
                FamilyName = "Arcanist",
                Tiers = new []
                {
                    CharacterClassRestrictions.Arcanist,
                    CharacterClassRestrictions.Spellbinder,
                    CharacterClassRestrictions.Clairvoyant
                }
            },
            new ClassFamily
            {
                FamilyName = "Ranger",
                Tiers = new []
                {
                    CharacterClassRestrictions.Ranger,
                    CharacterClassRestrictions.Wildstrider,
                    CharacterClassRestrictions.Shadowstalker
                }
            },
            new ClassFamily
            {
                FamilyName = "Zealot",
                Tiers = new []
                {
                    CharacterClassRestrictions.Zealot,
                    CharacterClassRestrictions.Acolyte,
                    CharacterClassRestrictions.Inquisitor
                }
            },
            new ClassFamily
            {
                FamilyName = "Reaver",
                Tiers = new []
                {
                    CharacterClassRestrictions.Reaver,
                    CharacterClassRestrictions.Slayer,
                    CharacterClassRestrictions.Nightblade
                }
            },
            new ClassFamily
            {
                FamilyName = "Summoner",
                Tiers = new []
                {
                    CharacterClassRestrictions.Summoner,
                    CharacterClassRestrictions.Archvesper,
                    CharacterClassRestrictions.Exanimar
                }
            }
        };

        public static Dictionary<string, CharacterClassRestrictions> NameToBits =
            new Dictionary<string, CharacterClassRestrictions>
        {
            // Knight
            { "Knight",        CharacterClassRestrictions.Knight },
            { "Vanguard",      CharacterClassRestrictions.Vanguard },
            { "Warlord",       CharacterClassRestrictions.Warlord },

            // Arcanist
            { "Arcanist",      CharacterClassRestrictions.Arcanist },
            { "Spellbinder",   CharacterClassRestrictions.Spellbinder },
            { "Clairvoyant",   CharacterClassRestrictions.Clairvoyant },

            // Ranger
            { "Ranger",        CharacterClassRestrictions.Ranger },
            { "Wildstrider",   CharacterClassRestrictions.Wildstrider },
            { "Shadowstalker", CharacterClassRestrictions.Shadowstalker },

            // Zealot
            { "Zealot",        CharacterClassRestrictions.Zealot },
            { "Acolyte",       CharacterClassRestrictions.Acolyte },
            { "Inquisitor",    CharacterClassRestrictions.Inquisitor },

            // Reaver
            { "Reaver",        CharacterClassRestrictions.Reaver },
            { "Slayer",        CharacterClassRestrictions.Slayer },
            { "Nightblade",    CharacterClassRestrictions.Nightblade },

            // Summoner
            { "Summoner",      CharacterClassRestrictions.Summoner },
            { "Archvesper",    CharacterClassRestrictions.Archvesper },
            { "Exanimar",      CharacterClassRestrictions.Exanimar }
        };

        public static int GetTier(CharacterClassRestrictions classType)
        {
            foreach (var family in Families)
            {
                for (int i = 0; i < family.Tiers.Length; i++)
                {
                    if (family.Tiers[i] == classType)
                        return i;
                }
            }
            return 0; // Fallback
        }

        public static CharacterClassRestrictions GetNextTierClass(CharacterClassRestrictions currentClass)
        {
            foreach (var family in Families)
            {
                for (int i = 0; i < family.Tiers.Length - 1; i++)
                {
                    if (family.Tiers[i] == currentClass)
                    {
                        return family.Tiers[i + 1];
                    }
                }
            }

            return CharacterClassRestrictions.None;
        }
    }
}