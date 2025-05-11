using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PLAYERTWO.ARPGProject;

public class MessageHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IScrollHandler
{
    public GUIChatWindow chatWindow;

    public void OnPointerEnter(PointerEventData eventData)
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        chatWindow?.StopOverlayFadeOut();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        bool cursorStillOnAny = false;
        foreach (var msg in chatWindow.overlayLogContent.GetComponentsInChildren<MessageHoverHandler>())
        {
            if (msg == null) continue;

            var rt = msg.GetComponent<RectTransform>();
            if (rt != null && RectTransformUtility.RectangleContainsScreenPoint(rt, Input.mousePosition))
            {
                cursorStillOnAny = true;
                break;
            }
        }

        if (!cursorStillOnAny)
        {
            chatWindow?.StartFadeOutCountdown();
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        var scrollRect = chatWindow?.overlayScrollRect;
        if (scrollRect != null)
        {
            scrollRect.OnScroll(eventData);
        }
    }
}
