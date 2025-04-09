using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Collectible Name")]
    public class GUICollectibleName : MonoBehaviour
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

        protected virtual void InitializeCamera() => m_camera = Camera.main;

        protected virtual void InitializeParent()
        {
            if (GUI.instance && GUI.instance.collectiblesContainer)
                transform.SetParent(GUI.instance.collectiblesContainer);
        }

        private void ClearContainer()
        {
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }

        private void AddEntry(string text, Sprite icon, Color color)
        {
            var entry = Instantiate(entryPrefab, container);
            var amountText = entry.transform.Find("Name").GetComponent<Text>();
            var currencyIcon = entry.transform.Find("Icon").GetComponent<Image>();

            amountText.text = text;
            amountText.color = color;

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

            if (collectible is CollectibleItem collectibleItem && collectibleItem.item != null)
            {
                AddEntry(collectibleItem.item.GetName(), null, color);
            }
            else if (collectible is CollectibleMoney multiMoney)
            {
                if (multiMoney.solmire > 0)
                {
                    AddEntry(multiMoney.solmire.ToString(), multiMoney.iconSolmire, color);
                }
                if (multiMoney.lunaris > 0)
                {
                    AddEntry(multiMoney.lunaris.ToString(), multiMoney.iconLunaris, color);
                }
                if (multiMoney.amberlings > 0)
                {
                    AddEntry(multiMoney.amberlings.ToString(), multiMoney.iconAmberlings, color);
                }
            }
            // Fallback
            else
            {
                AddEntry(collectible.GetName(), null, color);
            }
        }

        public void SetMultiCurrency(List<CollectibleMoney> collectibles, Color color)
        {
            if (collectibles == null || collectibles.Count == 0)
                return;

            m_target = collectibles[0];
            ClearContainer();

            AddEntry("SetMultiCurrency - stary system (nieużywane)", null, color);
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
    }
}
