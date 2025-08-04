using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [System.Serializable]
    public class StatsSerializer
    {
        public int level;
        public int strength;
        public int dexterity;
        public int vitality;
        public int energy;
        public int availablePoints;
        public int experience;

        public StatsSerializer(CharacterStats stats, EntityBuffManager buffManager = null)
        {
            level = stats.currentLevel;
            strength = stats.currentStrength;
            dexterity = stats.currentDexterity;
            vitality = stats.currentVitality;
            energy = stats.currentEnergy;

            if (buffManager != null)
            {
                foreach (var instance in buffManager.buffs)
                {
                    if (!instance.isActive)
                        continue;

                    strength -= instance.buff.strength;
                    dexterity -= instance.buff.dexterity;
                    vitality -= instance.buff.vitality;
                    energy -= instance.buff.energy;
                }
            }

            availablePoints = stats.currentAvailablePoints;
            experience = stats.currentExperience;
        }
        public virtual string ToJson() => JsonUtility.ToJson(this);
        public virtual StatsSerializer FromJson(string json) =>
            JsonUtility.FromJson<StatsSerializer>(json);
    }
}