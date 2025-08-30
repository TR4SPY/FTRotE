using UnityEngine;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts.Achievements
{
    [CreateAssetMenu(menuName = "AI-DDA/Achievement Conditions/No Deaths")]
    public class NoDeathsCondition : AchievementCondition
    {
        public override bool IsMet(PlayerBehaviorLogger logger, CharacterInstance character)
            => logger != null && logger.playerDeaths == 0;
    }
}