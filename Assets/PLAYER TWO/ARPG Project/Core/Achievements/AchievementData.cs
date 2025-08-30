using System.Collections.Generic;
using UnityEngine;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts.Achievements
{
    [CreateAssetMenu(menuName = "AI-DDA/Achievement")]
    public class AchievementData : ScriptableObject
    {
        public string id;
        public string achievementName;
        [TextArea]
        public string description;
        public string forWho;
        public CharacterClassRestrictions forClass = CharacterClassRestrictions.None;

        [Header("Rewards")]
        public int experience;
        public int coins;
        public bool extraRewards;
        public int additionalExperience;
        public int additionalCoins;

        [Header("Conditions")]
        public bool requireAllConditions = true;
        public List<AchievementCondition> conditions = new List<AchievementCondition>();

        public bool IsMet(PlayerBehaviorLogger logger, CharacterInstance character)
        {
            if (conditions == null || conditions.Count == 0)
                return false;

            if (requireAllConditions)
            {
                foreach (var c in conditions)
                    if (c == null || !c.IsMet(logger, character))
                        return false;
                return true;
            }
            else
            {
                foreach (var c in conditions)
                    if (c != null && c.IsMet(logger, character))
                        return true;
                return false;
            }
        }
    }
}