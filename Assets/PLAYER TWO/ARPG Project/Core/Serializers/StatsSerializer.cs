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

        // Dodane pola dla mnożników trudności
      //  public float dexterityMultiplier;
      //  public float strengthMultiplier;
       // public float speedMultiplier;

        public StatsSerializer(CharacterStats stats)
        {
            level = stats.currentLevel;
            strength = stats.currentStrength;
            dexterity = stats.currentDexterity;
            vitality = stats.currentVitality;
            energy = stats.currentEnergy;
            availablePoints = stats.currentAvailablePoints;
            experience = stats.currentExperience;

            // Przypisanie mnożników trudności
         //   dexterityMultiplier = stats.dexterityMultiplier;
         //   strengthMultiplier = stats.strengthMultiplier;
            //speedMultiplier = stats.speedMultiplier;
        }

        public virtual string ToJson() => JsonUtility.ToJson(this);

        public virtual StatsSerializer FromJson(string json) =>
            JsonUtility.FromJson<StatsSerializer>(json);
    }
}