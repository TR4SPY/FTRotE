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
            GUIWindowsManager.instance.guildmasterWindow as GUIGuildmaster;

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
                Debug.LogError("[Guildmaster] Nie znaleziono komponentu GUIGuildmaster w GUIWindowsManager!");
                return;
            }

            var logger = GetComponent<NpcInteractionLogger>();
            if (logger && entity.TryGetComponent(out Collider col))
            {
                logger.LogInteraction(col);
            }

            OpenGuildmasterService();
        }

        public void OpenGuildmasterService()
        {
            if (m_guiGuildmaster == null)
            {
                Debug.LogError("[Guildmaster] m_guiGuildmaster is NULL!");
                return;
            }

            m_guiGuildmaster.Show();
            m_guiGuildmaster.SetGuildmaster(this);
        }
    }
}