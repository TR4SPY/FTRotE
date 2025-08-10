using System;

namespace PLAYERTWO.ARPGProject
{
    [Serializable]
    public struct StatModifier
    {
        public string statName;
        public int value;
        public bool percentage;

        public StatModifier(string statName, int value, bool percentage = false)
        {
            this.statName = statName;
            this.value = value;
            this.percentage = percentage;
        }
    }
}
