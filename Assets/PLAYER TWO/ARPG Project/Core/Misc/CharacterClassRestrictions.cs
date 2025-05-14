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

        public static int GetTier(CharacterClassRestrictions classType)
        {
            foreach (var family in ClassHierarchy.Families)
            {
                for (int i = 0; i < family.Tiers.Length; i++)
                {
                    if (family.Tiers[i] == classType)
                        return i;
                }
            }
            return 0; // fallback
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