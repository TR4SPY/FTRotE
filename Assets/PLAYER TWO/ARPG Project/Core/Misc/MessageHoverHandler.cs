using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PLAYERTWO.ARPGProject;

public class MessageHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IScrollHandler
{
    private GUIOverlay overlay;
    private ScrollRect scrollRect;

    void Awake()
    {
        overlay = Object.FindFirstObjectByType<GUIOverlay>();
        scrollRect = GetComponentInParent<ScrollRect>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[Hover] Pointer ENTER on: {gameObject.name}");

        overlay?.PauseAllFade();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"[Hover] Pointer EXIT from: {gameObject.name}");

        bool stillHoveringAny = false;

        foreach (var hover in overlay.GetComponentsInChildren<MessageHoverHandler>())
        {
            if (hover == null) continue;

            var rt = hover.GetComponent<RectTransform>();
            if (rt != null && RectTransformUtility.RectangleContainsScreenPoint(rt, eventData.position, eventData.enterEventCamera))
            {
                stillHoveringAny = true;
                break;
            }
        }

        if (!stillHoveringAny)
        {
            overlay?.ResumeAllFade();
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        scrollRect?.OnScroll(eventData);
    }
}
