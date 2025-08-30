using System.Linq;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    public static class ProgressCalculator
    {
        public static int CountAvailableQuests(CharacterClassRestrictions cls, string playerType)
        {
            var quests = GameDatabase.instance?.quests;
            if (quests == null)
                return 0;

            return quests.Count(q => q != null && q.IsAvailableFor(cls, playerType));
        }
    }
}
