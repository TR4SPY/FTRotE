using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [System.Serializable]
    public enum CurrencyType
    {
        Amberlings, // "Amber"
        Lunaris,    // "Silver"
        Solmire     // "Gold"
    }

    [System.Serializable]
    public class Currency
    {
        //  - 1 Solmire = 1000 Lunaris = 100,000 Amberlings
        //  - 1 Lunaris = 100 Amberlings

        public int solmire;   
        public int lunaris;   
        public int amberlings;

        private const int AmberlingsToLunaris = 100;
        private const int LunarisToSolmire = 1000;

        public int TotalAmberlings => GetTotalAmberlings();

        public int GetTotalAmberlings()
        {
            return amberlings
                 + (lunaris * AmberlingsToLunaris)
                 + (solmire * LunarisToSolmire * AmberlingsToLunaris);
        }

        public void SetFromTotalAmberlings(int totalAmberlings)
        {
            solmire = totalAmberlings / (LunarisToSolmire * AmberlingsToLunaris);
            totalAmberlings %= (LunarisToSolmire * AmberlingsToLunaris);

            lunaris = totalAmberlings / AmberlingsToLunaris;
            amberlings = totalAmberlings % AmberlingsToLunaris;
        }

        public void AddAmberlings(int amount)
        {
            amberlings += amount;
            Normalize();
        }

        public bool RemoveAmberlings(int amount)
        {
            int total = GetTotalAmberlings();
            if (amount > total)
                return false;

            total -= amount;
            SetFromTotalAmberlings(total);
            return true;
        }

        public void AddSpecificCurrency(CurrencyType type, int amount)
        {
            switch (type)
            {
                case CurrencyType.Solmire:
                    solmire += amount;
                    break;
                case CurrencyType.Lunaris:
                    lunaris += amount;
                    break;
                case CurrencyType.Amberlings:
                    amberlings += amount;
                    break;
            }
            Normalize();
        }

        public static CurrencyType? ParseUnit(string unit)
        {
            switch (unit.ToLowerInvariant())
            {
                case "sol":
                case "s":
                case "solmire":
                    return CurrencyType.Solmire;

                case "lun":
                case "l":
                case "lunaris":
                    return CurrencyType.Lunaris;

                case "amb":
                case "a":
                case "amber":
                case "amberlings":
                    return CurrencyType.Amberlings;

                default:
                    return null;
            }
        }

        public static string FormatCurrencyString(int totalAmberlings)
        {
            if (totalAmberlings <= 0)
                return "0 Amberlings";

            var c = new Currency();
            c.SetFromTotalAmberlings(totalAmberlings);

            var parts = new List<string>();
            if (c.solmire > 0) parts.Add($"{c.solmire} Solmire");
            if (c.lunaris > 0) parts.Add($"{c.lunaris} Lunaris");
            if (c.amberlings > 0) parts.Add($"{c.amberlings} Amberlings");

            if (parts.Count == 0)
                return "0 Amberlings";

            return string.Join(", ", parts);
        }

        public bool RemoveSpecificCurrency(CurrencyType type, int amount)
        {
            switch (type)
            {
                case CurrencyType.Solmire:
                    if (solmire < amount) return false;
                    solmire -= amount;
                    return true;
                case CurrencyType.Lunaris:
                    if (lunaris < amount) return false;
                    lunaris -= amount;
                    return true;
                case CurrencyType.Amberlings:
                    if (amberlings < amount) return false;
                    amberlings -= amount;
                    return true;
            }
            return false;
        }

        public void Normalize()
        {
            if (amberlings >= AmberlingsToLunaris)
            {
                lunaris += amberlings / AmberlingsToLunaris;
                amberlings %= AmberlingsToLunaris;
            }

             if (lunaris >= LunarisToSolmire)
            {
                solmire += lunaris / LunarisToSolmire;
                lunaris %= LunarisToSolmire;
            }
        }

        public static void SplitCurrency(int total, out int solmire, out int lunaris, out int amberlings)
        {
            solmire = total / 100000;
            int remainder = total % 100000;

            lunaris = remainder / 100;
            amberlings = remainder % 100;
        }

        public static int ConvertToAmberlings(int amount, CurrencyType type)
        {
            switch (type)
            {
                case CurrencyType.Solmire:    return amount * 100000;
                case CurrencyType.Lunaris:    return amount * 100;
                case CurrencyType.Amberlings: return amount;
                default:                      return 0;
            }
        }

        public override string ToString()
        {
            return $"{solmire} Solmire, {lunaris} Lunaris, {amberlings} Amberlings";
        }
    }
}
