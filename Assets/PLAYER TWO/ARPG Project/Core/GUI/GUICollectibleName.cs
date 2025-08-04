using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Collectible Name")]
    public class GUICollectibleName : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Tooltip("Reference to the background image.")]
        public Image backgroundImage;

        [Tooltip("Reference to the container that holds the entries.")]
        public Transform container;

        [Tooltip("Reference to the prefab for each currency entry.")]
        public GameObject entryPrefab;

        [Tooltip("The offset applied when this object is instantiated.")]
        public Vector3 offset = new Vector3(0, 0.5f, 0);

        protected Collectible m_target;
        protected Camera m_camera;

        private List<Text> m_texts = new();
        private Color m_baseTextColor;
        private bool m_disableHoverBackground = false;
        private bool m_invertHoverColors = false;

        private static readonly Color defaultBackgroundColor = new Color(0, 0, 0, 0.5f);
        private static readonly Color hoverTextColor = new Color(0, 0, 0, 0.5f);

        protected virtual void InitializeCamera() => m_camera = Camera.main;

        protected virtual void InitializeParent()
        {
            if (GUI.instance && GUI.instance.collectiblesContainer)
                transform.SetParent(GUI.instance.collectiblesContainer);
        }

        private void ClearContainer()
        {
            m_texts.Clear();

            foreach (Transform child in container)
                Destroy(child.gameObject);
        }

        private void AddEntry(string text, Sprite icon, Color color)
        {
            var entry = Instantiate(entryPrefab, container);
            var amountText = entry.transform.Find("Name").GetComponent<Text>();
            var currencyIcon = entry.transform.Find("Icon").GetComponent<Image>();

            amountText.text = text;
            amountText.color = color;

            m_texts.Add(amountText);
            m_baseTextColor = color;

            if (icon != null)
            {
                currencyIcon.sprite = icon;
                currencyIcon.gameObject.SetActive(true);
            }
            else
            {
                currencyIcon.gameObject.SetActive(false);
            }
        }

        public virtual void SetCollectible(Collectible collectible, Color color)
        {
            if (!collectible) return;

            m_target = collectible;
            ClearContainer();
            m_disableHoverBackground = false;
            m_invertHoverColors = false;

            if (collectible is CollectibleItem collectibleItem && collectibleItem.item != null)
            {
                if (collectibleItem.item.data != null)
                {
                    m_disableHoverBackground = collectibleItem.item.data.disableHoverHighlight;

                    if (collectibleItem.item.data.IsQuestSpecific)
                    {
                        m_invertHoverColors = true;
                        m_baseTextColor = Color.black;

                        if (backgroundImage)
                            backgroundImage.color = GameColors.Gold;

                        AddEntry(collectibleItem.item.GetName(), null, m_baseTextColor);
                        return;
                    }

                    if (collectibleItem.item.data is ItemJewel)
                    {
                        m_baseTextColor = GameColors.Gold;
                        AddEntry(collectibleItem.item.GetName(), null, m_baseTextColor);
                        if (backgroundImage)
                            backgroundImage.color = defaultBackgroundColor;
                        return;
                    }
                }

                AddEntry(collectibleItem.item.GetName(), null, color);
            }
            else if (collectible is CollectibleMoney multiMoney)
            {
                m_disableHoverBackground = multiMoney.disableHoverHighlight;

                if (multiMoney.solmire > 0)
                    AddEntry(multiMoney.solmire.ToString(), multiMoney.iconSolmire, color);
                if (multiMoney.lunaris > 0)
                    AddEntry(multiMoney.lunaris.ToString(), multiMoney.iconLunaris, color);
                if (multiMoney.amberlings > 0)
                    AddEntry(multiMoney.amberlings.ToString(), multiMoney.iconAmberlings, color);
            }
            else
            {
                AddEntry(collectible.GetName(), null, color);
            }

            if (backgroundImage)
                backgroundImage.color = defaultBackgroundColor;
        }

        public void SetMultiCurrency(List<CollectibleMoney> collectibles, Color color)
        {
            if (collectibles == null || collectibles.Count == 0)
                return;

            m_target = collectibles[0];
            ClearContainer();

            AddEntry("SetMultiCurrency - stary system (nieużywane)", null, color);

            if (backgroundImage)
                backgroundImage.color = defaultBackgroundColor;
        }

        protected virtual void Start()
        {
            InitializeCamera();
            InitializeParent();
        }

        protected virtual void LateUpdate()
        {
            if (!m_target)
            {
                Destroy(this.gameObject);
                return;
            }

            var position = m_target.transform.position + offset;
            var screenPos = m_camera.WorldToScreenPoint(position);
            transform.position = screenPos;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (backgroundImage && !m_disableHoverBackground)
                backgroundImage.color = m_invertHoverColors ? defaultBackgroundColor : m_baseTextColor;

            foreach (var t in m_texts)
                t.color = m_invertHoverColors ? GameColors.Gold :
                          m_disableHoverBackground ? m_baseTextColor :
                          hoverTextColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (backgroundImage && !m_disableHoverBackground)
                backgroundImage.color = m_invertHoverColors ? GameColors.Gold : defaultBackgroundColor;

            foreach (var t in m_texts)
                t.color = m_baseTextColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (m_target && Level.instance && Level.instance.player)
            {
                var player = Level.instance.player;

                if (player != null)
                {
                    player.targetInteractive = m_target;
                    player.MoveTo(m_target.transform.position);
                }
            }
        }
    }
}
