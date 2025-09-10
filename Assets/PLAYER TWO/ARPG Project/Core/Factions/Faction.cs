using System.Collections.Generic;

namespace AI_DDA.Assets.Scripts
{
    public static class FactionNames
    {
        public static readonly Dictionary<Faction, string> DisplayNames = new()
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
    }
}