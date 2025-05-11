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
        // public Transform logContent;
        // public GameObject messagePrefab;

        // public ScrollRect scrollRect;

        [Header("Chat Settings")]
        public int maxMessages = 100;

        [Header("Overlay Chat")]
        public Transform overlayLogContent;
        public GameObject overlayMessagePrefab;
        public ScrollRect overlayScrollRect;

        public int overlayMaxMessages = 8;

        private List<string> messageHistory = new();
        private Queue<GameObject> overlayMessageQueue = new();
        // private Queue<GameObject> messageQueue = new();

        protected override void Start()
        {
            base.Start();

            if (inputField)
            {
                inputField.onEndEdit.RemoveAllListeners();
                var cm = FindAnyObjectByType<ChatManager>();
                if (cm != null)
                    // inputField.onEndEdit.AddListener(cm.SubmitMessage);
		    inputField.onEndEdit.AddListener((text) =>
                    {
			if (!string.IsNullOrWhiteSpace(text) && text != inputField.placeholder.GetComponent<TMP_Text>()?.text)
			{
				cm.SubmitMessage(text);
			}

			cm.ForceCloseChat();
		    });
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

    /*
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
    */

    public void ClearOverlayLog()
    {
        overlayMessageQueue.Clear();
        foreach (Transform child in overlayLogContent)
            Destroy(child.gameObject);
    }

        /// <summary>
        /// Dodaje nowy komunikat do logContent.
        /// </summary>

        /*
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

        */


        public void ScrollOverlayToBottom()
        {
            if (overlayScrollRect == null) return;

            Canvas.ForceUpdateCanvases();
            overlayScrollRect.verticalNormalizedPosition = 0f;
        }

        public void AddOverlayMessage(string formattedMessage)
        {
            if (!overlayMessagePrefab || !overlayLogContent) return;

            var obj = Instantiate(overlayMessagePrefab, overlayLogContent);

            var uiText = obj.GetComponent<UnityEngine.UI.Text>() ?? obj.GetComponentInChildren<UnityEngine.UI.Text>();
            if (uiText != null)
                uiText.text = formattedMessage;

            var canvasGroup = obj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = obj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;

            var hoverHandler = obj.GetComponent<MessageHoverHandler>();
            if (hoverHandler != null)
                hoverHandler.chatWindow = this;

            overlayMessageQueue.Enqueue(obj);

            if (!ChatManager.Instance.IsChatOpen)
                StartFadeOutCountdown();

            if (overlayMessageQueue.Count > overlayMaxMessages)
            {
                var oldest = overlayMessageQueue.Dequeue();
                if (oldest != null) Destroy(oldest);
            }

            StartCoroutine(ScrollToBottomDelayed());
        }

        private IEnumerator ScrollToBottomDelayed()
        {
            yield return null;
            yield return null;

            Canvas.ForceUpdateCanvases();

            if (overlayScrollRect != null)
            {
                overlayScrollRect.verticalNormalizedPosition = 0f;
            }
        }

        private Coroutine overlayFadeCoroutine;

        public void StopOverlayFadeOut()
        {
            if (overlayFadeCoroutine != null)
            {
                StopCoroutine(overlayFadeCoroutine);
                overlayFadeCoroutine = null;
            }

            foreach (Transform child in overlayLogContent)
            {
                var cg = child.GetComponent<CanvasGroup>();
                if (cg != null)
                    cg.alpha = 1f;
            }
        }

        public void StartFadeOutCountdown()
        {
            if (overlayFadeCoroutine != null)
                StopCoroutine(overlayFadeCoroutine);

            Debug.Log(">>> StartFadeOutCountdown() triggered");

            overlayFadeCoroutine = StartCoroutine(FadeOut(5f));
        }

        private IEnumerator FadeOut(float delay)
        {
            Debug.Log(">>> FadeOut() called with delay: " + delay);

            yield return new WaitForSeconds(delay);

            Debug.Log($">>> overlayLogContent name: {overlayLogContent.name}, childCount: {overlayLogContent.childCount}");

foreach (Transform child in overlayLogContent)
{
    Debug.Log(">>> Found child: " + child.name);
}


            var toFade = new List<GameObject>();

            foreach (Transform child in overlayLogContent)
            {
                if (child != null)
                    toFade.Add(child.gameObject);
            }

            List<Coroutine> fadeCoroutines = new();

            foreach (var msg in toFade)
{
    Debug.Log(">>> Fading message: " + msg.name);
    StartCoroutine(FadeAndDestroy(msg, 0f, 1f));
}


            foreach (var fade in fadeCoroutines)
            {
                yield return fade;
            }

            SetOverlayVisible(false);
        }

        public void SetOverlayVisible(bool visible)
        {
            foreach (Transform child in overlayLogContent)
            {
                var cg = child.GetComponent<CanvasGroup>();
                if (cg == null)
                    cg = child.gameObject.AddComponent<CanvasGroup>();

                cg.alpha = visible ? 1f : 0f;
                cg.blocksRaycasts = visible;
                cg.interactable = visible;
            }
        }

        private IEnumerator FadeAndDestroy(GameObject obj, float delay, float fadeDuration)
        {
            if (!obj) yield break;

            yield return new WaitForSeconds(delay);

            var canvasGroup = obj.GetComponent<CanvasGroup>();
            if (!canvasGroup)
            {
                Debug.LogWarning(">>> No CanvasGroup on: " + obj.name);
                yield break;
            }

            float time = 0f;
            float startAlpha = canvasGroup.alpha;

            while (time < fadeDuration)
            {
                float t = time / fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                time += Time.deltaTime;
                yield return null;
            }

            canvasGroup.alpha = 0f;

            Debug.Log(">>> Destroying: " + obj.name);
            Destroy(obj);
        }

        public void RepopulateOverlayFromHistory(List<string> log, int maxToShow = 10)
        {
            if (overlayLogContent == null || overlayMessagePrefab == null) return;

            foreach (Transform child in overlayLogContent)
                Destroy(child.gameObject);

            overlayMessageQueue.Clear();

            int start = Mathf.Max(0, log.Count - maxToShow);
            for (int i = start; i < log.Count; i++)
            {
                AddOverlayMessage(log[i]);
            }
        }
    }
}
