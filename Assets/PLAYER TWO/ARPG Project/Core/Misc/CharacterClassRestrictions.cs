using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PLAYERTWO.ARPGProject
{
    [System.Flags]
    public enum CharacterClassRestrictions
    {
        None           = 0,
        
        Knight         = 1 << 0, // 1
        Vanguard       = 1 << 1, // 2
        Warlord        = 1 << 2, // 4

        Arcanist       = 1 << 3, // 8
        Spellbinder    = 1 << 4, // 16
        Clairvoyant    = 1 << 5, // 32
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
            }
        };

        public static Dictionary<string, CharacterClassRestrictions> NameToBits =
            new Dictionary<string, CharacterClassRestrictions>
        {
            { "Knight",      CharacterClassRestrictions.Knight },
            { "Vanguard",    CharacterClassRestrictions.Vanguard },
            { "Warlord",     CharacterClassRestrictions.Warlord },
            { "Arcanist",    CharacterClassRestrictions.Arcanist },
            { "Spellbinder", CharacterClassRestrictions.Spellbinder },
            { "Clairvoyant", CharacterClassRestrictions.Clairvoyant },
        };
    }
}