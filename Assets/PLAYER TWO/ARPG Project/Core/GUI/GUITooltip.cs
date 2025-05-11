// ✅ FINALNY KOMPLETNY SYSTEM TOOLTIP
// GUITooltip.cs
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

        [Header("Dynamic Container (new)")]
        public Transform dynamicContainer;
        public GameObject textLinePrefab;
        public GameObject priceTagPrefab;

        [Header("Currency Icons")]
        public Sprite solmireIcon;
        public Sprite lunarisIcon;
        public Sprite amberlingsIcon;

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
                tooltipName.gameObject.SetActive(!string.IsNullOrEmpty(title));
                tooltipName.text = title;
            }

            if (tooltipText != null)
            {
                tooltipText.gameObject.SetActive(true);
                tooltipText.text = message;
            }

            PrepareTooltipDisplay(target);
        }

        public void ShowQuestRewardsDynamic(Quest quest, int finalExp, int finalCoins, GameObject target)
        {
            lastTarget = target;
            isHovered = true;

            ClearDynamicContainer();

            if (tooltipName) tooltipName.gameObject.SetActive(false);
            if (tooltipText) tooltipText.gameObject.SetActive(false);

            AddLine($"Quest Rewards: {quest.title}", target);

            if (quest.experience > 0)
            {
                int baseExp = quest.experience;
                float multiplierExp = (float)finalExp / baseExp;
                AddLine($"Experience: Base {baseExp} + {(int)((multiplierExp - 1f) * 100)}% = {finalExp} XP", target);
            }

            if (quest.coins > 0)
            {
                int baseCoins = quest.coins;
                float multiplierCoins = (float)finalCoins / baseCoins;
                AddLine($"Coins: Base {baseCoins} + {(int)((multiplierCoins - 1f) * 100)}% = {finalCoins}", target);

                var c = new Currency();
                c.SetFromTotalAmberlings(finalCoins);

                if (c.solmire > 0) AddPriceTag(c.solmire, solmireIcon, target);
                if (c.lunaris > 0) AddPriceTag(c.lunaris, lunarisIcon, target);
                if (c.amberlings > 0) AddPriceTag(c.amberlings, amberlingsIcon, target);
            }

            PrepareTooltipDisplay(target);
        }

        private void PrepareTooltipDisplay(GameObject target)
        {
            if (!isActiveAndEnabled) return;

            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            if (target != null && target.TryGetComponent<RectTransform>(out RectTransform targetRect))
                SetPositionRelativeTo(targetRect);
            else
                UpdatePosition();

            StopAllCoroutines();
            StartCoroutine(FadeIn());
        }


        public void HideTooltip()
        {
            isHovered = false;
            lastTarget = null;

            if (!isActiveAndEnabled) return;

            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }

        public void HideTooltipDynamic()
        {
            isHovered = false;
            lastTarget = null;

            if (!gameObject.activeInHierarchy) return;

            ClearDynamicContainer();

            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }

        private void ClearDynamicContainer()
        {
            if (!dynamicContainer) return;
            foreach (Transform child in dynamicContainer)
                Destroy(child.gameObject);
        }

        private void AddLine(string text, GameObject target)
        {
            if (!dynamicContainer || !textLinePrefab) return;

            var go = Instantiate(textLinePrefab, dynamicContainer);
            var txt = go.transform.Find("Text")?.GetComponent<Text>();
            if (txt) txt.text = text;

            AddTooltipTrigger(go, target);
        }

        private void AddPriceTag(int amount, Sprite icon, GameObject target)
        {
            if (!dynamicContainer || !priceTagPrefab) return;

            var go = Instantiate(priceTagPrefab, dynamicContainer);
            var txt = go.transform.Find("Name")?.GetComponent<Text>();
            if (txt) txt.text = amount.ToString();

            var img = go.transform.Find("Icon")?.GetComponent<Image>();
            if (img && icon) img.sprite = icon;

            AddTooltipTrigger(go, target);
        }

        private void AddTooltipTrigger(GameObject go, GameObject target)
        {
            var trigger = go.GetComponent<EventTrigger>() ?? go.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            var entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entryEnter.callback.AddListener((eventData) => ShowTooltip("", "", target));
            trigger.triggers.Add(entryEnter);

            var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            entryExit.callback.AddListener((eventData) => HideTooltipDynamic());
            trigger.triggers.Add(entryExit);
        }

        protected virtual void OnDisable()
        {
            isHovered = false;
            lastTarget = null;
            ClearDynamicContainer();
        }

#if UNITY_STANDALONE || UNITY_WEBGL
        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (canvasGroup.alpha > 0f && lastTarget != null)
            {
                bool isOverTarget = IsPointerOverTarget(lastTarget);
                bool isOverTooltip = IsPointerOverTarget(tooltipPanel);
                bool isOverUI = IsPointerOverUI();

                if (!isOverTarget && !isOverTooltip && !isOverUI)
                {
                    HideTooltipDynamic();
                    HideTooltip();
                }
            }
        }
#endif

        private bool IsPointerOverTarget(GameObject target)
        {
            var eventData = new PointerEventData(EventSystem.current) { position = GetMousePosition() };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results.Exists(result => result.gameObject == target);
        }

        private bool IsPointerOverUI()
        {
            var eventData = new PointerEventData(EventSystem.current) { position = GetMousePosition() };
            var results = new List<RaycastResult>();
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
            //gameObject.SetActive(false);
        }
    }
}
