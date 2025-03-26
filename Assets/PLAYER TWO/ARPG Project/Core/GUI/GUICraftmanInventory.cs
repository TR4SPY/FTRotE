using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Craftman Inventory")]
    public class GUICraftmanInventory : GUIInventory
    {
        protected GUICraftman m_craftman;

        protected virtual void InitializeCraftman() =>
                    m_craftman = GetComponentInParent<GUICraftman>();

        public override bool TryPlace(GUIItem item)
        {
            if (item == null || item.item == null)
                return false;

            if (item.item.data.cannotBeDropped)
            {
                GameAudio.instance?.PlayDeniedSound();
                return false;
            }

            if (GUIWindowsManager.instance.GetInventory().Contains(item) &&
                !GUIWindowsManager.instance.GetInventory().TryRemove(item))
            {
                GameAudio.instance?.PlayDeniedSound();
                return false;
            }

            if (!TryAutoInsert(item))
            {
                GameAudio.instance?.PlayDeniedSound();
                return false;
            }

            return true;
        }

        protected virtual void Start()
        {
            InitializeCraftman();
        }
    }
}
