using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    public class GUIErrorMessageWindow : GUIWindow
    {
        [Header("Components")]
        public Text messageText;
        public Button okButton;

        public void Show(string message)
        {
            if (messageText)
                messageText.text = message;

            if (okButton)
            {
                okButton.onClick.RemoveAllListeners();
                okButton.onClick.AddListener(Hide);
            }

            base.Show();
        }
    }
}
