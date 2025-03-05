using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class GUITooltip : GUIInspector<GUITooltip>
    {
        public static new GUITooltip instance { get; private set; }

        [Header("Tooltip References")]
        public GameObject tooltipPanel;
        public Text tooltipName;
        public Text tooltipText;

        private GameObject lastTarget;
        private bool isHovered = false;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        public void ShowTooltip(string title, string message, GameObject target)
        {
            lastTarget = target;
            isHovered = true;

            if (tooltipName != null)
            {
                if (!string.IsNullOrEmpty(title))
                {
                    tooltipName.gameObject.SetActive(true);
                    tooltipName.text = title;
                }
                else
                {
                    tooltipName.gameObject.SetActive(false);
                }
            }

            if (tooltipText != null) tooltipText.text = message;

            if (!isActiveAndEnabled) return;

            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            if (lastTarget != null && lastTarget.TryGetComponent<RectTransform>(out RectTransform targetRect))
            {
                SetPositionRelativeTo(targetRect);
            }
            else
            {
                UpdatePosition();
            }

            StopAllCoroutines();
            StartCoroutine(FadeIn());
        }

        public void HideTooltip()
        {
            isHovered = false;
            lastTarget = null; // ✅ Wymuszone usunięcie referencji do ostatniego aktywatora tooltipa

            if (!isActiveAndEnabled) return;

            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }

        protected virtual void OnDisable()
        {
            HideTooltip();
        }

#if UNITY_STANDALONE || UNITY_WEBGL
        protected override void LateUpdate()
        {
            base.LateUpdate();

            // ✅ Jeśli tooltip jest aktywny i kursor NIE jest nad UI, chowamy go
            if (canvasGroup.alpha > 0f && lastTarget != null)
            {
                bool isOverTarget = IsPointerOverTarget(lastTarget);
                bool isOverTooltip = IsPointerOverTarget(tooltipPanel);
                bool isOverUI = IsPointerOverUI(); 

                if (!isOverTarget && !isOverTooltip && !isOverUI)
                {
                    HideTooltip();
                }
            }
        }
#endif

        private bool IsPointerOverTarget(GameObject target)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = GetMousePosition()
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject == target)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsPointerOverUI()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = GetMousePosition()
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            return results.Count > 0;
        }

        private Vector2 GetMousePosition()
        {
            return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        }

        private IEnumerator FadeIn()
        {
            float duration = 0.1f;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, time / duration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut()
        {
            float duration = 0.1f;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, time / duration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }
    }
}
