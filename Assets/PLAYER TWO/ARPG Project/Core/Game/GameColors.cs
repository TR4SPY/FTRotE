using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public class GameColors
    {
        public static Color HalfBack = new Color(0.5f, 0.5f, 0.5f);
        public static Color LightRed = new Color(1, 0.5f, 0.5f);
        public static Color LightBlue = new Color(0.5f, 0.5f, 1);
        public static Color Gold = new Color(1f, 0.84f, 0f);
        public static Color Turquoise = new Color(0.25f, 0.88f, 0.82f);
        public static Color Green = new Color(0.5f, 1f, 0.5f);
        public static Color Purple = new Color(0.75f, 0.5f, 1f);
        public static Color Gray = new Color(0.75f, 0.75f, 0.75f);

        /// <summary>
        /// Zwraca kolor mnożnika (zielony dla bonusu, czerwony dla kary, niebieski dla neutralnego).
        /// </summary>
        public static Color GetMultiplierColor(float multiplier)
        {
            if (multiplier > 1f) return Green;
            if (multiplier < 1f) return LightRed;
            return LightBlue;
        }

        /// <summary>
        /// Zwraca kolor na podstawie rzadkości przedmiotu.
        /// </summary>
        public static Color GetItemRarityColor(Item.Rarity rarity)
        {
            switch (rarity)
            {
                case Item.Rarity.Common: return Gray;
                case Item.Rarity.Uncommon: return Green;
                case Item.Rarity.Rare: return LightBlue;
                case Item.Rarity.Epic: return Purple;
                case Item.Rarity.Legendary: return Gold;
                default: return Color.white;
            }
        }
    }
}
