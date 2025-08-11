using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.ARPGProject
{
    public class CharacterStats
    {
        public int initialLevel;
        public int initialStrength;
        public int initialDexterity;
        public int initialVitality;
        public int initialEnergy;
        public int initialAvailablePoints;
        public int initialExperience;

        public int currentLevel => m_stats ? m_stats.level : initialLevel;
        public int currentStrength => m_stats ? m_stats.strength : initialStrength;
        public int currentDexterity => m_stats ? m_stats.dexterity : initialDexterity;
        public int currentVitality => m_stats ? m_stats.vitality : initialVitality;
        public int currentEnergy => m_stats ? m_stats.energy : initialEnergy;
        public int currentAvailablePoints => m_stats ? m_stats.availablePoints : initialAvailablePoints;
        public int currentExperience => m_stats ? m_stats.experience : initialExperience;

        protected EntityStatsManager m_stats;

        public UnityEvent onLevelUp { get; } = new UnityEvent();

        public CharacterStats(Character character)
        {
            Initialize(character.level, character.strength,
                character.dexterity, character.vitality,
                character.energy, 0, 0);
        }

        public CharacterStats(int initialLevel, int initialStrength,
            int initialDexterity, int initialVitality, int initialEnergy,
            int initialAvailablePoints, int initialExperience)
        {
            Initialize(initialLevel, initialStrength,
                initialDexterity, initialVitality, initialEnergy,
                initialAvailablePoints, initialExperience);
        }

        protected virtual void Initialize(int initialLevel, int initialStrength,
            int initialDexterity, int initialVitality, int initialEnergy,
            int initialAvailablePoints, int initialExperience)
        {
            this.initialLevel = initialLevel;
            this.initialStrength = initialStrength;
            this.initialDexterity = initialDexterity;
            this.initialVitality = initialVitality;
            this.initialEnergy = initialEnergy;
            this.initialAvailablePoints = initialAvailablePoints;
            this.initialExperience = initialExperience;
        }

        /// <summary>
        /// Initializes a given Entity Stats Manager.
        /// </summary>
        /// <param name="stats">The Entity Stats Manager you want to initialize.</param>
        public virtual void InitializeStats(EntityStatsManager stats, bool resetVitals = true)
        {
            if (m_stats != null)
                m_stats.onLevelUp.RemoveListener(HandleLevelUp);

            m_stats = stats;

            if (!resetVitals)
            {
                m_stats.Recalculate();
            }
            else
            {
                m_stats.BulkUpdate(initialLevel, initialStrength,
                initialDexterity, initialVitality, initialEnergy,
                initialAvailablePoints, initialExperience);
            }

            m_stats.onLevelUp.AddListener(HandleLevelUp);
        }

        protected virtual void HandleLevelUp()
        {
            onLevelUp.Invoke();
        }

        public static CharacterStats CreateFromSerializer(StatsSerializer serializer)
        {
            return new CharacterStats(serializer.level, serializer.strength,
                serializer.dexterity, serializer.vitality, serializer.energy,
                serializer.availablePoints, serializer.experience);
        }
    }
}