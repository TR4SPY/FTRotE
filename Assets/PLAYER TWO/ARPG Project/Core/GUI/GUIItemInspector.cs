using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Item Inspector")]
    public class GUIItemInspector : GUIInspector<GUIItemInspector>
    {
        [Header("Containers")]
        [Tooltip("References the parent of the general attributes text.")]
        public GameObject attributesContainer;

        [Tooltip("References the parent of the additional attributes text.")]
        public GameObject additionalAttributesContainer;

        [Tooltip("References the parent of the potion description text.")]
        public GameObject potionDescriptionContainer;

        [Header("Texts")]
        [Tooltip("A reference to the Text component that represents the Item's price.")]
        public Text itemPriceText;

        [Tooltip("A reference to the Text component that represents the Item's name.")]
        public Text itemName;

        [Tooltip("A reference to the Text component that represents the Item's potion description.")]
        public Text potionDescription;

        [Tooltip("References the Text component displaying the Item's general attributes.")]
        public Text attributesText;

        [Tooltip("References the Text component displaying the Item's additional attributes.")]
        public Text additionalAttributesText;

        [Tooltip("References the Text component displaying the skill instruction.")]
        public Text skillInstructionText;

        [Tooltip("Reference to the Text component displaying class Restrictions.")]
        public Text classRestrictionsText;

        [Header("Color Settings")]
        [Tooltip("Regular text colors.")]
        public Color regularColor = new(1, 1, 1, 1);

        [Tooltip("Rare text colors.")]
        public Color rareItemColor = new(1, 1, 1, 1);

        [Tooltip("Quest item name colors.")]
        public Color questItemColor = GameColors.Gold;

        [Tooltip("Is quest item name bold?")]
        public bool isBold = true; // Opcja do ustawienia bold w inspektorze

        [Tooltip("Invalid text colors.")]
        public Color invalidColor = new(1, 0, 0, 1);

        [Tooltip("Attention text colors.")]
        public Color attentionColor = new(1, 1, 0, 1);

        [Tooltip("Special text colors.")]
        public Color specialColor = GameColors.LightBlue;

        [Header("Skill Instructions")]
        [Tooltip("The instructions to show when inspecting a skill.")]
        public string skillPcInstruction = "Press 'Right-Click' to learn";

        [Tooltip("The instructions to show when inspecting a skill on mobile.")]
        public string skillMobileInstruction = "Double Tap to learn";

        protected CanvasGroup m_group;
        protected ItemInstance m_item;
        protected GUIItem m_guiItem;

        protected Entity m_entity;

        protected System.Action updateHandler;

        protected virtual void InitializeEntity() => m_entity = Level.instance.player;

        protected virtual void InitializeCanvasGroup()
        {
            m_group = GetComponent<CanvasGroup>();
            m_group.blocksRaycasts = false;
        }

        protected virtual void InitializeInstance()
        {
            updateHandler = () => UpdateAll();
            Hide();
        }

        /// <summary>
        /// Shows the inspector with information from a given GUI Item.
        /// </summary>
        /// <param name="item">The item you want to inspect.</param>
        public virtual void Show(GUIItem item)
        {
            if (item == null || gameObject.activeSelf)
                return;

            m_guiItem = item;
            m_item = item.item;
            m_item.onChanged += updateHandler;
            gameObject.SetActive(true);
            m_rect.SetAsLastSibling();
            UpdateAll();
            FadIn();
        }

        /// <summary>
        /// Hides the inspector.
        /// </summary>
        public virtual void Hide()
        {
            if (m_item != null)
                m_item.onChanged -= updateHandler;

            gameObject.SetActive(false);
        }

        protected virtual void UpdateAll()
        {
            UpdatePriceText();
            UpdateItemName();
            UpdatePotionDescription();
            UpdateAttributes();
            UpdateAdditionalAttributes();
            UpdateSkillInstruction();

            if (m_item.IsEquippable())
            {
                ShowClassRestrictions(m_item.GetEquippable().allowedClasses, "equipped");
            }
            else if (m_item.data.allowedClasses != CharacterClassRestrictions.None)
            {
                ShowClassRestrictions(m_item.data.allowedClasses, "used");
            }
            else
            {
                if (classRestrictionsText != null)
                    classRestrictionsText.gameObject.SetActive(false);
            }
        }

        protected virtual void UpdatePriceText()
        {
            itemPriceText.gameObject.SetActive(GUIWindowsManager.instance.merchantWindow.isOpen);

            if (itemPriceText.gameObject.activeSelf)
            {
                var buying = m_guiItem.onMerchant;
                var price = buying ? m_item.GetPrice() : m_item.GetSellPrice();
                var prefix = buying ? "Buy" : "Sell";
                itemPriceText.text = $"{prefix}:  {price.ToString()}";
            }
        }

        protected virtual void UpdateItemName()
        {
            itemName.text = m_item.data.name;
            itemName.color = regularColor;

            if (m_item.IsSkill() || m_item.GetAttributesCount() > 0)
                itemName.color = specialColor;
            
            if (m_item.data is ItemQuest questItem && questItem.IsQuestSpecific)
            {
                string colorHex = ColorUtility.ToHtmlStringRGB(questItemColor);

                string formattedText = isBold 
                    ? $"<b><color=#{colorHex}>{m_item.data.name}</color></b>" 
                    : $"<color=#{colorHex}>{m_item.data.name}</color>";

                itemName.text = formattedText;
            }
        }

        protected virtual void UpdateAttributes()
        {
            attributesContainer.SetActive(m_item.IsEquippable() || m_item.IsSkill());

            if (attributesContainer.activeSelf)
                attributesText.text = m_item.Inspect(m_entity.stats, attentionColor, invalidColor, specialColor, questItemColor);
        }

        protected virtual void UpdatePotionDescription()
        {
            potionDescriptionContainer.SetActive(m_item.IsPotion());

            if (potionDescriptionContainer.activeSelf)
            {
                potionDescription.text = "";

                if (m_item.GetPotion().healthAmount > 0)
                    potionDescription.text +=
                        $"Increases Health Points by {m_item.GetPotion().healthAmount}.";

                if (m_item.GetPotion().manaAmount > 0)
                {
                    if (potionDescription.text.Length > 0)
                        potionDescription.text += "\n";

                    potionDescription.text +=
                        $"Increases Mana Points by {m_item.GetPotion().manaAmount}.";
                }
            }
        }

        private void ShowClassRestrictions(CharacterClassRestrictions allowed, string verb)
        {
            if (classRestrictionsText == null)
                return;

            var allPossible = Enum.GetValues(typeof(CharacterClassRestrictions))
                .Cast<CharacterClassRestrictions>()
                .Where(c => c != CharacterClassRestrictions.None)
                .Aggregate((a, b) => a | b);

            if ((allowed & allPossible) == allPossible)
            {
                classRestrictionsText.gameObject.SetActive(false);
                return;
            }

            var player = Game.instance?.currentCharacter;
            CharacterClassRestrictions playerClass = CharacterClassRestrictions.None;

            if (player?.Entity != null)
            {
                string className = player.Entity.name.Replace("(Clone)", "").Trim();
                ClassHierarchy.NameToBits.TryGetValue(className, out playerClass);
            }

            classRestrictionsText.gameObject.SetActive(true);
            string result = "";
            bool anyoneCanUse = false;

            foreach (var family in ClassHierarchy.Families)
            {
                var allowedTiers = new List<CharacterClassRestrictions>();
                foreach (var tier in family.Tiers)
                {
                    if ((allowed & tier) != 0)
                        allowedTiers.Add(tier);
                }

                if (allowedTiers.Count == 0)
                    continue;

                anyoneCanUse = true;
                bool playerMatches = allowedTiers.Contains(playerClass);
                string displayName = allowedTiers[0].ToString();

                if (playerMatches)
                {
                    if (allowedTiers.Count == 1)
                        result += StringUtils.StringWithColor($"Can only be {verb} by {displayName}", regularColor) + "\n";
                    else
                        result += StringUtils.StringWithColor($"Can be {verb} by {displayName}", regularColor) + "\n";
                }
                else
                {
                    if (allowedTiers.Count == 1)
                        result += StringUtils.StringWithColor($"Can only be {verb} by {displayName}", invalidColor) + "\n";
                    else
                        result += StringUtils.StringWithColor($"Can be {verb} by {displayName}", invalidColor) + "\n";
                }
            }

            if (!anyoneCanUse)
            {
                string line = StringUtils.StringWithColor($"Cannot be {verb}", invalidColor);
                classRestrictionsText.text = line;
            }
            else
            {
                classRestrictionsText.text = result;
            }
        }

        protected virtual void UpdateAdditionalAttributes()
        {
            var text = m_item.attributes?.Inspect();

            if (text == null || text.Length == 0)
            {
                additionalAttributesContainer.SetActive(false);
                return;
            }

            additionalAttributesContainer.SetActive(true);
            additionalAttributesText.text = text;
        }

        protected virtual void UpdateSkillInstruction()
        {
            SetParentActive(skillInstructionText, m_item.IsSkill() && !m_guiItem.onMerchant);

            if (skillInstructionText.gameObject.activeSelf)
            {
#if UNITY_STANDALONE || UNITY_WEBGL
                skillInstructionText.text = skillPcInstruction;
#else
                skillInstructionText.text = skillMobileInstruction;
#endif
            }
        }

        protected virtual void SetParentActive(Text element, bool value)
        {
            if (element == null || element.transform.parent == null)
                return;

            element.transform.parent.gameObject.SetActive(value);
        }

        protected virtual void Start()
        {
            InitializeEntity();
            InitializeCanvasGroup();
            InitializeInstance();
        }
    }
}