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
}