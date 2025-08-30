using UnityEngine;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts.Achievements
{
    [CreateAssetMenu(menuName = "AI-DDA/Achievement Conditions/Quests Completed")]
    public class QuestsCompletedCondition : AchievementCondition
    {
        public int questsRequired = 1;
        public override bool IsMet(PlayerBehaviorLogger logger, CharacterInstance character)
            => logger != null && logger.questsCompleted >= questsRequired;
    }
}