using UnityEngine;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/NPC/Guildmaster")]
    public class Guildmaster : Interactive
    {
        [Tooltip("Dialog przypisany do Guildmastera (opcjonalny).")]
        public Dialog assignedDialog;

        protected GUIGuildmaster m_guiGuildmaster =>
            GUIWindowsManager.instance.GetComponentInChildren<GUIGuildmaster>(true);

        protected override void OnInteract(object other)
        {
            if (!(other is Entity entity)) return;

            if (assignedDialog != null &&
                GUIWindowsManager.instance.dialogWindow != null)
            {
                GUIWindowsManager.instance.dialogWindow.Show(entity, this, assignedDialog);
                return;
            }

            if (m_guiGuildmaster == null)
            {
                Debug.LogError("[Guildmaster] Nie znaleziono komponentu GUIGuildmaster w scenie!");
                return;
            }

            m_guiGuildmaster.Show();

            var logger = GetComponent<NpcInteractionLogger>();
            if (logger && entity.TryGetComponent(out Collider col))
            {
                logger.LogInteraction(col);
            }
        }

        public void OpenGuildmasterService()
        {
            var guildmasterWindow = GUIWindowsManager.instance.guildmasterWindow;

            if (guildmasterWindow == null)
            {
                Debug.LogError("[Craftman] craftmanWindow is NULL in GUIWindowsManager!");
                return;
            }

            guildmasterWindow.Show();

            if (m_guiGuildmaster == null)
            {
                Debug.LogError("[Craftman] m_guiCraftman is NULL!");
                return;
            }

            m_guiGuildmaster.SetGuildmaster(this);
        }
    }
}
