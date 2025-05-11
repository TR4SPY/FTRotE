using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Help Window")]
    public class GUIHelpWindow : GUIWindow
    {
        [Header("Navigation Buttons")]
        public Button stuckButton;
        [Header("Help GUI Window reference")]
        public GameObject helpGUI;
        protected override void Start()
        {
            base.Start();
            
            if (stuckButton != null)
                stuckButton.onClick.AddListener(stuckButtonAction);
            else
                Debug.LogError("[HelpWindow] Stuck Button is NULL! Assign it in the Inspector.");
        }

        /// <summary>
        /// Otwiera ustawienia badań (research).
        /// </summary>
        public void stuckButtonAction()
        {
            if (LevelRespawner.instance)
            {
                LevelRespawner.instance.TeleportToWaypoint();
            }
            else
            {
                Debug.LogWarning("[StuckButton] LevelRespawner nie jest dostępny!");
            }

            Hide();
        }
    }
}
