using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

namespace PLAYERTWO.ARPGProject
{
    public class GUIChatWindow : GUIWindow
    {
        [Header("Chat UI")]
        public TMP_InputField inputField;
        public Transform logContent;
        public GameObject messagePrefab;

        public ScrollRect scrollRect;

        [Header("Chat Settings")]
        public int maxMessages = 50;

        private Queue<GameObject> messageQueue = new();
        protected override void Start()
        {
            base.Start();

            if (inputField)
            {
                inputField.onEndEdit.RemoveAllListeners();
                var cm = FindAnyObjectByType<ChatManager>();
                if (cm != null)
                    inputField.onEndEdit.AddListener(cm.SubmitMessage);
            }
        }

        /// <summary>
        /// Zwraca true, jeśli InputField ma focus.
        /// Używane przez ChatManager do wyłączania map akcji.
        /// </summary>
        public bool IsInputFocused
            => inputField != null && inputField.isFocused;

        /// <summary>
        /// Ustawia focus na polu inputu.
        /// </summary>
        public void FocusInput()
        {
            if (!inputField) return;
            inputField.ActivateInputField();
            inputField.Select();
        }

        /// <summary>
        /// Zabiera focus z inputu.
        /// </summary>
        public void RemoveFocus()
        {
            if (!inputField) return;
            inputField.DeactivateInputField();
        }

        public void ClearLog()
        {
            foreach (Transform child in logContent)
            {
                Destroy(child.gameObject);
            }

            messageQueue.Clear();
        }

        private void ScrollToBottom()
        {
            if (scrollRect == null) return;
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }

        /// <summary>
        /// Dodaje nowy komunikat do logContent.
        /// </summary>
        public void AddMessageToLog(string formattedMessage)
        {
            if (!messagePrefab || !logContent)
            {
                Debug.LogWarning("[GUIChatWindow] Missing messagePrefab or logContent!");
                return;
            }

            var obj = Instantiate(messagePrefab, logContent);

            var tmpLabel = obj.GetComponent<TMPro.TMP_Text>() ?? obj.GetComponentInChildren<TMPro.TMP_Text>();
            var uiLabel = obj.GetComponent<UnityEngine.UI.Text>() ?? obj.GetComponentInChildren<UnityEngine.UI.Text>();

            if (tmpLabel != null)
            {
                tmpLabel.text = formattedMessage;
            }
            else if (uiLabel != null)
            {
                uiLabel.text = formattedMessage;
            }
            else
            {
                Debug.LogWarning("[GUIChatWindow] No Text component (TMP_Text or UI.Text) found on message prefab!");
            }

            messageQueue.Enqueue(obj);

            if (messageQueue.Count > maxMessages)
            {
                var oldest = messageQueue.Dequeue();
                Destroy(oldest);
            }

            StartCoroutine(DelayedScrollToBottom());
        }

        private IEnumerator DelayedScrollToBottom()
        {
            yield return null;
            ScrollToBottom();
        }

    }
}
