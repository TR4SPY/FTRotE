using UnityEngine;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/NPC/Cartographer")]
    public class Cartographer : Interactive
    {
        public CartographyManager manager;
        public Dialog assignedDialog;

        protected GUICartography m_guiCartography => GUIWindowsManager.instance.GetCartography();

        protected override void OnInteract(object other)
        {
            if (!(other is Entity entity))
                return;

            if (assignedDialog != null)
            {
                GUIWindowsManager.instance.dialogWindow.Show(entity, this, assignedDialog);
                return;
            }

            OpenCartographyService();
        }

        public void OpenCartographyService()
        {
            var cartoWindow = GUIWindowsManager.instance.cartographWindow;
            var inventoryWindow = GUIWindowsManager.instance.inventoryWindow;

            if (cartoWindow == null || inventoryWindow == null)
            {
                Debug.LogError("[Cartographer] Missing GUI windows.");
                return;
            }

            cartoWindow.Show();
            inventoryWindow.Show();

            if (m_guiCartography != null)
                m_guiCartography.SetManager(manager);
        }
    }
}
