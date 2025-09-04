using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public class TradeManager : Singleton<TradeManager>
    {
        public void RequestTrade(string playerName)
        {
            var settings = GameSettings.instance;
            if (settings != null && settings.GetTradeConfirmations())
            {
                Debug.Log($"[TradeManager] Trade with {playerName} requires confirmation.");
            }

            Debug.Log($"[TradeManager] Trade request sent to {playerName}.");
        }
    }
}
