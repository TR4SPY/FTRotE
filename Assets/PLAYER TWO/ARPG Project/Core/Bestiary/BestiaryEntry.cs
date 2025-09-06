using System;
using UnityEngine;
using UnityEngine.Localization;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    [Serializable]
    public class BestiaryEntry
    {
        public int enemyId;
        public LocalizedString name;
        public LocalizedString description;
        public Sprite icon;
        public int encounters;
        public int kills;
    }

    /// <summary>
    /// Lightweight representation of a BestiaryEntry used for save data.
    /// Contains only primitive fields to simplify serialization.
    /// </summary>
    [Serializable]
    public class BestiaryEntrySaveData
    {
        public int enemyId;
        public int encounters;
        public int kills;
    }

}