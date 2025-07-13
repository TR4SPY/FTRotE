using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Item Inspector")]
    public class GUIItemInspector : GUIInspector<GUIItemInspector>
    {
        [Header("Containers")]
        [Tooltip("References the parent of the general attributes text.")]
        public GameObject attributesContainer;

        [Tooltip("References the parent of the general elements text.")]
        public GameObject elementsContainer;

        [Tooltip("References the parent of the additional attributes text.")]
        public GameObject additionalAttributesContainer;

        [Tooltip("References the parent of the potion description text.")]
        public GameObject potionDescriptionContainer;

        [Header("Price Tag (dynamic)")]
        public Transform priceContainer;
        public GameObject priceTagPrefab;
        public Sprite solmireIcon;
        public Sprite lunarisIcon;
        public Sprite amberlingsIcon;

        [Header("Texts")]
       // [Tooltip("A reference to the Text component that represents the Item's price.")]
       // public Text itemPriceText;


        [Tooltip("A reference to the Text component that represents the Item's name.")]
        public Text itemName;

        [Tooltip("References the Text component displaying the comparison stats.")]
        public Text comparisonText;

        [Tooltip("A reference to the Text component that represents the Item's potion description.")]
        public Text potionDescription;

        [Tooltip("References the Text component displaying the Item's general attributes.")]
        // public Text attributesText;
        public TextMeshProUGUI attributesText;

        [Tooltip("References the Text component displaying the Item's elements.")]
        public TMPro.TextMeshProUGUI elementsText;

        [Tooltip("References the Text component displaying the Item's additional attributes.")]
        public Text additionalAttributesText;

        [Tooltip("References the Text component displaying the skill instruction.")]
        public Text skillInstructionText;

        [Tooltip("Reference to the Text component displaying class Restrictions.")]
        public Text classRestrictionsText;

        [Tooltip("Displays the skill name granted by the weapon.")]
        public Text itemSkill;

        [Tooltip("Displays the skill's stat requirements (if any).")]
        public Text itemSkillReq;

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
        
        [Tooltip("A reference to the Text component that represents the item's description.")]
        public Text itemDescriptionText;

        [Tooltip("The container for the item description text.")]
        public GameObject itemDescriptionContainer;

        protected CanvasGroup m_group;
        protected ItemInstance m_item;
        protected GUIItem m_guiItem;
        protected bool m_showComparison;

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
            UpdateElementalAttributes();
            UpdateSkillInstruction();
            UpdateItemDescription(); 
            UpdateWeaponSkill();

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
                    classRestrictionsText.text = "";
            }
        }

        protected virtual void UpdatePriceText()
        {
            bool isMerchantOpen = GUIWindowsManager.instance.merchantWindow.isOpen;
            if (priceContainer)
                priceContainer.gameObject.SetActive(isMerchantOpen);

            if (!isMerchantOpen)
            {
                ClearPriceTags();
                return;
            }

            var buying = m_guiItem.onMerchant;
            var price = buying ? m_item.GetPrice() : m_item.GetSellPrice();
            var prefix = buying ? "Buy:" : "Sell:";

            ShowPriceTags(prefix, price);
        }

        /// <summary>
        /// Czyści wszystkie dzieci w priceContainer (usuwamy wszystko, bo nie mamy Title).
        /// </summary>
        private void ClearPriceTags()
        {
            if (!priceContainer) return;
            foreach (Transform child in priceContainer)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Dodaje jeden element "priceTagPrefab" (z liczbą i ikoną) do priceContainer.
        /// </summary>
        private void AddPriceTag(int amount, Sprite icon)
        {
            var go = Instantiate(priceTagPrefab, priceContainer);

            var textObj = go.transform.Find("Name")?.GetComponent<Text>();
            if (textObj) textObj.text = amount.ToString();

            var iconObj = go.transform.Find("Icon")?.GetComponent<Image>();
            if (iconObj && icon)
                iconObj.sprite = icon;
        }

        /// <summary>
        /// Dodaje prefab zawierający sam prefix (np. "Buy:"), ale bez ikony.
        /// </summary>
        private void AddPrefixTag(string prefix)
        {
            var go = Instantiate(priceTagPrefab, priceContainer);

            var textObj = go.transform.Find("Name")?.GetComponent<Text>();
            if (textObj) textObj.text = prefix;

            var iconObj = go.transform.Find("Icon")?.GetComponent<Image>();
            if (iconObj) iconObj.gameObject.SetActive(false); // prefix nie ma ikony
        }

        /// <summary>
        /// Wyświetla prefiks (Buy/Sell) oraz dynamicznie dodaje elementy waluty w priceContainer.
        /// </summary>
        private void ShowPriceTags(string prefix, int cost)
        {
            ClearPriceTags();

            if (!string.IsNullOrEmpty(prefix))
                AddPrefixTag(prefix);

            if (cost <= 0) return;
            var c = new Currency();
            c.SetFromTotalAmberlings(cost);

            if (c.solmire > 0)
                AddPriceTag(c.solmire, solmireIcon);
            if (c.lunaris > 0)
                AddPriceTag(c.lunaris, lunarisIcon);
            if (c.amberlings > 0)
                AddPriceTag(c.amberlings, amberlingsIcon);
        } 
        
        protected virtual void UpdateItemName()
        {
            if (m_item == null || m_item.data == null)
                return;

            var name = m_item.GetName();

            string formattedName;

            if (m_item.data is ItemJewel)
            {
                formattedName = StringUtils.StringWithColorAndStyle(name, GameColors.Gold, bold: true);
            }
            else if (m_item.data.IsQuestSpecific)
            {
                formattedName = StringUtils.StringWithColorAndStyle(name, GameColors.Gold, bold: true);
            }
            else
            {
                var rarityColor = GameColors.RarityColor(m_item.data.rarity);
                formattedName = StringUtils.StringWithColorAndStyle(name, rarityColor, bold: isBold);
            }

            itemName.text = formattedName;
        }

        protected virtual void UpdateAttributes()
        {
            attributesContainer.SetActive(m_item.IsEquippable() || m_item.IsSkill());

            if (attributesContainer.activeSelf)
            {
                attributesText.text = m_item.Inspect(m_entity.stats, attentionColor, invalidColor, specialColor, questItemColor);
                var magicDesc = GetMagicDescription(m_item);
                if (!string.IsNullOrWhiteSpace(magicDesc))
                    attributesText.text += "\n\n" + magicDesc;
            }
        }

        protected virtual void UpdateItemDescription()
        {
            string desc = null;

            if (m_item.data is ItemJewel jewel)
            {
                bool hasDescription = !string.IsNullOrWhiteSpace(jewel.description);
                bool hasSuccessRate = jewel.successRate > 0;

                if (hasDescription)
                {
                    desc = jewel.description;

                    if (hasSuccessRate)
                    {
                        Color color = GetSuccessRateColor(jewel.successRate);
                        string coloredRate = StringUtils.StringWithColor($"{jewel.successRate}%", color);
                        desc += $"\n\nSuccess rate: {coloredRate}";
                    }
                }
            }

            bool shouldShow = !string.IsNullOrWhiteSpace(desc);

            if (itemDescriptionContainer != null)
                itemDescriptionContainer.SetActive(shouldShow);

            if (itemDescriptionText != null && shouldShow)
                itemDescriptionText.text = desc;
        }

        private Color GetSuccessRateColor(int rate)
        {
            if (rate < 40) return GameColors.LightRed;
            if (rate < 60) return GameColors.Orange;
            if (rate < 80) return GameColors.Gold;
            return GameColors.Green;
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

        protected virtual void UpdateWeaponSkill()
        {
            if (!(m_item.data is ItemWeapon weapon) || weapon.skill == null)
            {
                if (itemSkill != null)
                {
                    itemSkill.text = "";
                    SetParentActive(itemSkill, false);
                }

                if (itemSkillReq != null)
                {
                    itemSkillReq.text = "";
                    SetParentActive(itemSkillReq, false);
                }

                return;
            }

            if (!m_item.isSkillEnabled)
            {
                if (itemSkill != null)
                {
                    itemSkill.text = "";
                    SetParentActive(itemSkill, false);
                }

                if (itemSkillReq != null)
                {
                    itemSkillReq.text = "";
                    SetParentActive(itemSkillReq, false);
                }

                return;
            }

            string name = StringUtils.StringWithColorAndStyle(weapon.skill.name + " Skill", GameColors.Purple, bold: true);
            if (itemSkill != null)
            {
                itemSkill.text = name;
                SetParentActive(itemSkill, true);
            }

            if (weapon.skillSource != null)
            {
                if (weapon.skillSource.skill != weapon.skill)
                {
                    return;
                }

                var stats = m_entity.stats;
                var itemSkillSource = weapon.skillSource;

                int missingLvl = Mathf.Max(0, itemSkillSource.requiredLevel - stats.level);
                int missingStr = Mathf.Max(0, itemSkillSource.requiredStrength - stats.strength);
                int missingEne = Mathf.Max(0, itemSkillSource.requiredEnergy - stats.energy);

                string reqText = "";

                if (missingLvl > 0)
                    reqText += StringUtils.StringWithColor($"Skill requires +{missingLvl} Level", invalidColor) + "\n";

                if (missingStr > 0)
                    reqText += StringUtils.StringWithColor($"Skill requires +{missingStr} Strength", invalidColor) + "\n";

                if (missingEne > 0)
                    reqText += StringUtils.StringWithColor($"Skill requires +{missingEne} Energy", invalidColor) + "\n";

                if (itemSkillReq != null)
                {
                    itemSkillReq.text = reqText.TrimEnd('\n');
                    SetParentActive(itemSkillReq, !string.IsNullOrEmpty(itemSkillReq.text));
                }
            }
            else
            {
                if (itemSkillReq != null)
                {
                    itemSkillReq.text = "";
                    SetParentActive(itemSkillReq, false);
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

        private string GetMagicDescription(ItemInstance item)
        {
            var lines = new List<string>();

            if (item.data is ItemSkill skillBook && skillBook.skill != null)
            {
                var skill = skillBook.skill;

                if (skill.school != MagicSchool.None)
                    lines.Add($"Magic School: {skill.school}");

                if (skill.form != MagicForm.None)
                    lines.Add($"Form of Spell: {skill.form}");

                if (skill.type != MagicType.None)
                    lines.Add($"Type of Spell: {skill.type}");

                if (skill.element != MagicElement.None)
                    lines.Add($"Magic Element: {skill.element}");
            }

            return string.Join("\n", lines);
        }

        protected virtual void UpdateAdditionalAttributes()
        {
            if (m_item.attributes != null)
            {
                string attrText = m_item.attributes.Inspect();
                additionalAttributesText.text = attrText;
                additionalAttributesContainer.SetActive(!string.IsNullOrWhiteSpace(attrText));
            }
            else
            {
                additionalAttributesContainer.SetActive(false);
            }
        }

        protected virtual void UpdateElementalAttributes()
        {
            if (m_item.elements != null)
            {
                string elemText = m_item.elements.Inspect();
                elementsText.text = elemText;
                elementsContainer.SetActive(!string.IsNullOrWhiteSpace(elemText));
            }
            else
            {
                elementsContainer.SetActive(false);
            }
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

        protected virtual ItemInstance GetEquippedItem(ItemInstance item)
        {
            var items = Level.instance.player?.items;
            if (items == null || item == null)
                return null;

            if (item.IsArmor())
            {
                switch (item.GetArmor().slot)
                {
                    case ItemSlots.Helm:
                        return items.GetHelm();
                    case ItemSlots.Chest:
                        return items.GetChest();
                    case ItemSlots.Pants:
                        return items.GetPants();
                    case ItemSlots.Gloves:
                        return items.GetGloves();
                    case ItemSlots.Boots:
                        return items.GetBoots();
                }
            }
            else if (item.IsShield())
                return items.GetLeftHand();
            else if (item.IsBow())
                return items.GetRightHand();
            else if (item.IsWeapon())
            {
                var right = items.GetRightHand();
                if (right != null && right.IsWeapon())
                    return right;

                var left = items.GetLeftHand();
                if (left != null && left.IsWeapon())
                    return left;
            }

            return null;
        }

        protected virtual string FormatDelta(float value)
        {
            if (Mathf.Approximately(value, 0))
                return "0";

            var color = value > 0 ? GameColors.LightBlue : GameColors.LightRed;
            var sign = value > 0 ? "+" : string.Empty;
            return StringUtils.StringWithColor($"{sign}{value}", color);
        }

        protected virtual string BuildComparisonText(ItemInstance equipped)
        {
            if (equipped == null)
                return string.Empty;

            var text = string.Empty;

            if (m_item.IsWeapon() && equipped.IsWeapon())
            {
                var min = m_item.GetWeapon().minDamage - equipped.GetWeapon().minDamage;
                var max = m_item.GetWeapon().maxDamage - equipped.GetWeapon().maxDamage;
                text += $"Damage: {FormatDelta(min)} ~ {FormatDelta(max)}";
                var atk = m_item.GetWeapon().attackSpeed - equipped.GetWeapon().attackSpeed;
                text += $"\nAttack Speed: {FormatDelta(atk)}";
            }
            else if ((m_item.IsArmor() || m_item.IsShield()) && (equipped.IsArmor() || equipped.IsShield()))
            {
                int def = 0;
                if (m_item.IsArmor())
                    def += m_item.GetArmor().defense;
                if (m_item.IsShield())
                    def += m_item.GetShield().defense;

                int eqDef = 0;
                if (equipped.IsArmor())
                    eqDef += equipped.GetArmor().defense;
                if (equipped.IsShield())
                    eqDef += equipped.GetShield().defense;

                text += $"Defense: {FormatDelta(def - eqDef)}";
            }

            int health = m_item.GetAdditionalHealth() - equipped.GetAdditionalHealth();
            int mana = m_item.GetAdditionalMana() - equipped.GetAdditionalMana();
            int damage = m_item.GetAdditionalDamage() - equipped.GetAdditionalDamage();
            int speed = m_item.GetAttackSpeed() - equipped.GetAttackSpeed();
            int defense = m_item.GetAdditionalDefense() - equipped.GetAdditionalDefense();

            if (damage != 0)
                text += $"\nAdditional Damage: {FormatDelta(damage)}";
            if (speed != 0)
                text += $"\nAdditional Attack Speed: {FormatDelta(speed)}";
            if (defense != 0)
                text += $"\nAdditional Defense: {FormatDelta(defense)}";
            if (mana != 0)
                text += $"\nAdditional Mana: {FormatDelta(mana)}";
            if (health != 0)
                text += $"\nAdditional Health: {FormatDelta(health)}";

            return text;
        }

        protected virtual void Update()
        {
            if (comparisonText == null || m_item == null)
                return;

            var pressed = Keyboard.current != null &&
                (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);

            if (pressed && !m_showComparison && gameObject.activeSelf && m_item.IsEquippable())
            {
                var equipped = GetEquippedItem(m_item);
                comparisonText.text = BuildComparisonText(equipped);
                comparisonText.gameObject.SetActive(true);
                m_showComparison = true;
            }
            else if (!pressed && m_showComparison)
            {
                comparisonText.gameObject.SetActive(false);
                comparisonText.text = string.Empty;
                m_showComparison = false;
            }
        }

        protected virtual void Start()
        {
            InitializeEntity();
            InitializeCanvasGroup();
            InitializeInstance();
        }
    }
}