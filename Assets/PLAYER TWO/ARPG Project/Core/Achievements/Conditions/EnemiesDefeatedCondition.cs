using UnityEngine;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts.Achievements
{
    [CreateAssetMenu(menuName = "AI-DDA/Achievement Conditions/Enemies Defeated")]
    public class EnemiesDefeatedCondition : AchievementCondition
    {
        public int enemiesRequired = 1;
        public override bool IsMet(PlayerBehaviorLogger logger, CharacterInstance character)
            => logger != null && logger.enemiesDefeated >= enemiesRequired;
    }
}
