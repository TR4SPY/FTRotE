using UnityEngine;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts.Achievements
{
    [CreateAssetMenu(menuName = "AI-DDA/Achievement Conditions/Zones Discovered")]
    public class ZonesDiscoveredCondition : AchievementCondition
    {
        public int zonesRequired = 1;
        public override bool IsMet(PlayerBehaviorLogger logger, CharacterInstance character)
            => logger != null && logger.zonesDiscovered >= zonesRequired;
    }
}
