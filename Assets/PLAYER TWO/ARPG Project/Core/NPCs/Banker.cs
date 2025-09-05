using UnityEngine;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Misc/Banker")]
    public class Banker : Interactive
    {
        [Header("Banker Settings")]
        [Tooltip("Dialog shown when interacting with the banker.")]
        public Dialog assignedDialog;

        protected override void OnInteract(object other)
        {
            if (!(other is Entity entity)) return;

            if (assignedDialog != null)
            {
                if (GUIWindowsManager.instance.dialogWindow == null)
                {
                    Debug.LogError("dialogWindow is NULL in GUIWindowsManager! Ensure it is assigned.");
                    return;
                }

                GUIWindowsManager.instance.dialogWindow.Show(entity, this, assignedDialog);
                return;
            }

            if (GUIWindowsManager.instance.bankWindow == null)
            {
                Debug.LogError("bankWindow is NULL in GUIWindowsManager! Ensure it is assigned.");
                return;
            }

            GUIWindowsManager.instance.bankWindow.Show();
        }

        public void OpenBankWindow()
        {
            if (GUIWindowsManager.instance.bankWindow == null)
            {
                Debug.LogError("bankWindow is NULL in GUIWindowsManager! Ensure it is assigned.");
                return;
            }

            GUIWindowsManager.instance.bankWindow.Show();
        }
    }
}