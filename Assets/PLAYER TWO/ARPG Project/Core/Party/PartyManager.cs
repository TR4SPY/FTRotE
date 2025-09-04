using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public class PartyManager : Singleton<PartyManager>
    {
        public bool Invite(string playerName, bool isFriend)
        {
            var settings = GameSettings.instance;
            if (settings != null && !settings.CanReceiveInvite(isFriend))
            {
                Debug.Log($"[PartyManager] Invite to {playerName} blocked by settings.");
                return false;
            }

            Debug.Log($"[PartyManager] Invite sent to {playerName}.");
            return true;
        }
    }
}
