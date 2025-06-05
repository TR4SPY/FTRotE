using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public class GameColors
    {
        // Neutralne / bazowe
        public static Color White       = new Color(1f, 1f, 1f);
        public static Color Gray        = new Color(0.75f, 0.75f, 0.75f);
        public static Color HalfBack    = new Color(0.5f, 0.5f, 0.5f);
        public static Color SlateGray   = new Color(0.44f, 0.5f, 0.56f);

        // Jasne kolory
        public static Color LightRed    = new Color(1f, 0.5f, 0.5f);
        public static Color LightBlue   = new Color(0.5f, 0.5f, 1f);
        public static Color Cyan        = new Color(0f, 1f, 1f);
        public static Color Green       = new Color(0.5f, 1f, 0.5f);
        public static Color Purple      = new Color(0.75f, 0.5f, 1f);
        public static Color Gold        = new Color(1f, 0.84f, 0f);
        public static Color Turquoise   = new Color(0.25f, 0.88f, 0.82f);
        public static Color Ivory       = new Color(1f, 1f, 0.94f);

        // Mocne, wyraziste kolory
        public static Color Crimson     = new Color(0.86f, 0.08f, 0.24f);  // mocna czerwień
        public static Color RoyalBlue   = new Color(0.25f, 0.41f, 0.88f);  // głęboki niebieski
        public static Color Magenta     = new Color(1f, 0f, 1f);           // róż/fiolet
        public static Color Orange      = new Color(1f, 0.55f, 0f);        // pomarańcz
        public static Color Emerald     = new Color(0.31f, 0.78f, 0.47f);  // zielony szmaragd
        public static Color Amber       = new Color(1f, 0.75f, 0f);        // złoto-pomarańcz
        public static Color CyanBright  = new Color(0f, 1f, 1f);           // czysty cyjan
        public static Color Lime        = new Color(0.75f, 1f, 0f);        // limonkowy
        public static Color HotPink     = new Color(1f, 0.08f, 0.58f);     // jaskrawy róż
        public static Color NeonYellow  = new Color(1f, 1f, 0.1f);         // neonowy żółty
        public static Color BrightTeal  = new Color(0f, 0.9f, 0.8f);       // mocny turkus
        public static Color DarkPurple  = new Color(0.4f, 0f, 0.6f);       // ciemny fiolet

        /// <summary>
        /// Zwraca kolor mnożnika (zielony dla bonusu, czerwony dla kary, niebieski dla neutralnego).
        /// </summary>
        public static Color MultiplierColor(float multiplier)
        {
            if (multiplier > 1f) return Green;
            if (multiplier < 1f) return LightRed;
            return LightBlue;
        }

        /// <summary>
        /// Zwraca kolor na podstawie rzadkości przedmiotu.
        /// </summary>
        public static Color RarityColor(Item.Rarity rarity)
        {
            switch (rarity)
            {
                case Item.Rarity.Common: return White;
                case Item.Rarity.Uncommon: return Turquoise;
                case Item.Rarity.Rare: return RoyalBlue;
                case Item.Rarity.Epic: return Purple;
                case Item.Rarity.Legendary: return Emerald;
                default: return Color.white;
            }
        }

        public static Color ElementColor(MagicElement element)
        {
            return element switch
            {
                MagicElement.Fire => Orange,
                MagicElement.Ice => Cyan,
                MagicElement.Lightning => NeonYellow,
                MagicElement.Shadow => Purple,
                MagicElement.Light => Gold,
                MagicElement.Arcane => LightBlue,
                _ => White
            };
        }
    }
}
