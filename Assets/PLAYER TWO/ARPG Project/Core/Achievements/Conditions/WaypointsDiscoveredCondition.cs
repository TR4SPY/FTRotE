using UnityEngine;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts.Achievements
{
    [CreateAssetMenu(menuName = "AI-DDA/Achievement Conditions/Waypoints Discovered")]
    public class WaypointsDiscoveredCondition : AchievementCondition
    {
        public int waypointsRequired = 1;
        public override bool IsMet(PlayerBehaviorLogger logger, CharacterInstance character)
            => logger != null && logger.waypointsDiscovered >= waypointsRequired;
    }
}