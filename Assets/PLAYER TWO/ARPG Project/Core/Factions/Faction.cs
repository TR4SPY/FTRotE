using System.Collections.Generic;
using UnityEngine;

namespace AI_DDA.Assets.Scripts
{
    /// <summary>
    /// List of all available factions in the game. Expand as needed.
    /// </summary>
    public enum Faction
    {
        None,
        Emberforge_Enclave,
        The_Veilborne,
        Vel_Asheri_Codex,
        The_Ninefold_Loom,
        Order_of_the_Second_Sight,

        Halethrim,
        Velmari_Synod,
        Ithrenhal,
        Ardrel_Thass,
        The_Ashenvale_Quietus,

        Sorrowclad,
        Namarrix_Order,
        Children_of_Hollow_Earth,
        Vaer_khaal,
        Qireth_Noon,
        Nulthen_Dar,
        Vurex_tal,
        Kelorim_Syn,

        Thalanic_Depths,
        Dravarnir_Clans,
        Rhelakkar,

        Covenant_of_Ashenvale,
        Blackwake_Covenant,
        Zaurm_Kei,
        TherVaal_Dominion,

        Ashenvale_Protectors,
        Ironclasp_Regime,
        Oathsworn_Hold,
        Vanguards_of_Therenfall,
        Crimson_Pledge,
        The_Bastards_Steel
    }

    /// <summary>
    /// Utility helpers for faction display information.
    /// </summary>
    public static class FactionUtility
    {
        /// <summary>
        /// Default display names for each faction. These can be overridden by
        /// a <see cref="FactionData"/> asset at runtime.
        /// </summary>
        public static readonly Dictionary<Faction, string> DefaultNames = new()
        {
            // DEFAULT
            { Faction.None, "None" },

            // Neutral / Craftman / Builder / Blacksmith
            { Faction.Emberforge_Enclave, "Emberforge Enclave" },

            // Mystic / Arcane
            { Faction.The_Veilborne, "The Veilborne" },
            { Faction.Vel_Asheri_Codex, "Vel'Asheri Codex" },
            { Faction.The_Ninefold_Loom, "The Ninefold Loom" },
            { Faction.Order_of_the_Second_Sight, "Order of the Second Sight" },

            // Elder / Ancient Orders
            { Faction.Halethrim, "Halethrim" },
            { Faction.Velmari_Synod, "Velmari Synod" },
            { Faction.Ithrenhal, "Ithrenhal" },
            { Faction.Ardrel_Thass, "Ardrel Thass" },
            { Faction.The_Ashenvale_Quietus, "The Ashenvale Quietus"},

            // Dark Cults / Obscure
            { Faction.Sorrowclad, "Sorrowclad" },
            { Faction.Namarrix_Order, "Namarrix Order" },
            { Faction.Children_of_Hollow_Earth, "Children of Hollow Earth" },
            { Faction.Vaer_khaal, "Vaer'khaal" },
            { Faction.Qireth_Noon, "Qireth'Noon" },
            { Faction.Nulthen_Dar, "Nulthen Dar" },
            { Faction.Vurex_tal, "Vurex’tal" },
            { Faction.Kelorim_Syn, "Kelorim Syn" },

            // Nomadic / Tribal / Natural
            { Faction.Thalanic_Depths, "Thalanic Depths" },
            { Faction.Dravarnir_Clans, "Dravarnir Clans" },
            { Faction.Rhelakkar, "Rhelakkar" },

            // Shadow / Rogue / Secretive
            { Faction.Covenant_of_Ashenvale, "Covenant of Ashenvale" },
            { Faction.Blackwake_Covenant, "Blackwake Covenant" },
            { Faction.Zaurm_Kei, "Zaurm-Kei" },
            { Faction.TherVaal_Dominion, "Ther’Vaal Dominion" },

            // Military
            { Faction.Ashenvale_Protectors, "Ashenvale Protectors" },
            { Faction.Ironclasp_Regime, "Ironclasp Regime" },
            { Faction.Oathsworn_Hold, "Oathsworn Hold" },
            { Faction.Vanguards_of_Therenfall, "Vanguards of Therenfall" },
            { Faction.Crimson_Pledge, "Crimson Pledge" },
            { Faction.The_Bastards_Steel, "The Bastards Steel" }
        };

        /// <summary>
        /// Returns the display name for a faction using either the custom
        /// <see cref="FactionData"/> mapping or the default dictionary.
        /// </summary>
        public static string GetDisplayName(Faction faction)
        {
            var entry = FactionData.Instance?.Get(faction);
            if (entry != null && !string.IsNullOrEmpty(entry.displayName))
                return entry.displayName;

            return DefaultNames.TryGetValue(faction, out var name)
                ? name
                : faction.ToString();
        }

        /// <summary>
        /// Retrieves the icon associated with a faction, if any.
        /// </summary>
        public static Sprite GetIcon(Faction faction)
        {
            return FactionData.Instance?.Get(faction)?.icon;
        }
    }
}
