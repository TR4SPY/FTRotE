using UnityEngine;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts.Achievements
{
    [CreateAssetMenu(menuName = "AI-DDA/Achievement Conditions/Level Reached")]
    public class LevelReachedCondition : AchievementCondition
    {
        public int levelRequired = 1;
        public override bool IsMet(PlayerBehaviorLogger logger, CharacterInstance character)
            => character != null && character.stats != null && character.stats.currentLevel >= levelRequired;
    }
}