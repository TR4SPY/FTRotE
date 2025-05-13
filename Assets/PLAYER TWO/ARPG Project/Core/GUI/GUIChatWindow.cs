using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    public class GUIChatWindow : GUIWindow
    {
        [Header("Chat UI")]
        public TMP_InputField inputField;

        [Header("Overlay Chat")]
        public Transform overlayLogContent;
        public GameObject overlayMessagePrefab;
        public ScrollRect overlayScrollRect;
        public int overlayMaxMessages = 20;

        [Header("External Controller")]
        public GUIOverlay overlayController;

        private Queue<GameObject> overlayMessageQueue = new();

        protected override void Start()
        {
            base.Start();

            // Debug.Log("[GUIChatWindow] Start()");

            if (inputField)
            {
                inputField.onEndEdit.RemoveAllListeners();
                var cm = FindAnyObjectByType<ChatManager>();
                if (cm != null)
                {
                    inputField.onEndEdit.AddListener((text) =>
                    {
                        // Debug.Log("[Chat] onEndEdit: " + text);
                        if (!string.IsNullOrWhiteSpace(text) && text != inputField.placeholder.GetComponent<TMP_Text>()?.text)
                        {
                            cm.SubmitMessage(text);
                        }
                        cm.ForceCloseChat();
                    });
                }
            }
        }

        public bool IsInputFocused => inputField != null && inputField.isFocused;

        public void FocusInput()
        {
            // Debug.Log("[Chat] FocusInput()");
            inputField?.ActivateInputField();
            inputField?.Select();
        }

        public void RemoveFocus()
        {
            // Debug.Log("[Chat] RemoveFocus()");
            inputField?.DeactivateInputField();
        }

        public void ClearOverlayLog()
        {
            // Debug.Log("[Overlay] Clearing overlay log");
            foreach (var msg in overlayMessageQueue)
            {
                if (msg != null)
                    Destroy(msg);
            }
            overlayMessageQueue.Clear();
        }

        public void AddOverlayMessage(string formattedMessage)
        {
            // Debug.Log("[Overlay] AddOverlayMessage: " + formattedMessage);

            if (!overlayMessagePrefab || !overlayLogContent) return;

            GameObject obj = Instantiate(overlayMessagePrefab, overlayLogContent);
            obj.name = $"OverlayMessage_{Time.time:F2}";

            var text = obj.GetComponentInChildren<Text>();
            if (text != null)
                text.text = formattedMessage;

            var canvasGroup = obj.GetComponent<CanvasGroup>() ?? obj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;

            overlayMessageQueue.Enqueue(obj);

            if (overlayMessageQueue.Count > overlayMaxMessages)
            {
                var oldest = overlayMessageQueue.Dequeue();
                if (oldest != null)
                {
                    // Debug.Log("[Overlay] Destroying oldest message: " + oldest.name);
                    Destroy(oldest);
                }
            }

            if (!overlayController)
            {
                Debug.LogWarning("[GUIChatWindow] overlayController not assigned!");
                return;
            }

            overlayController.FadeMessage(obj);

            ScrollOverlayToBottom();
        }

        public void ScrollOverlayToBottom()
        {
            Canvas.ForceUpdateCanvases();
            if (overlayScrollRect != null)
                overlayScrollRect.verticalNormalizedPosition = 0f;
        }

        public void RepopulateOverlayFromHistory(List<string> log, int maxToShow = 20)
        {
            // Debug.Log("[Overlay] Repopulating from history (" + log.Count + " messages)");

            if (!overlayLogContent || !overlayMessagePrefab) return;

            foreach (Transform child in overlayLogContent)
                Destroy(child.gameObject);

            overlayMessageQueue.Clear();

            int start = Mathf.Max(0, log.Count - maxToShow);
            for (int i = start; i < log.Count; i++)
            {
                var obj = Instantiate(overlayMessagePrefab, overlayLogContent);
                obj.name = "HistoryMessage_" + i;

                var text = obj.GetComponentInChildren<Text>();
                if (text != null)
                    text.text = log[i];

                var cg = obj.GetComponent<CanvasGroup>() ?? obj.AddComponent<CanvasGroup>();
                cg.alpha = 1f;
                overlayMessageQueue.Enqueue(obj);

                overlayController?.RegisterMessageWithoutFade(obj);

            }

            ScrollOverlayToBottom();
        }
    }
}
