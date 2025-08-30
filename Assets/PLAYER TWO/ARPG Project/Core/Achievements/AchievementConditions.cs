using UnityEngine;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts.Achievements
{
    public abstract class AchievementCondition : ScriptableObject
    {
        public abstract bool IsMet(PlayerBehaviorLogger logger, CharacterInstance character);
    }
}