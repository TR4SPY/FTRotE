using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Stats Manager")]
    public class GUIStatsManager : MonoBehaviour
    {
        private readonly Dictionary<Text, Color> m_defaultTextColors = new();

        private Color GetDefaultColor(Text text)
        {
            if (!text)
                return Color.white;

            if (!m_defaultTextColors.TryGetValue(text, out var color))
            {
                color = text.color;
                m_defaultTextColors[text] = color;
            }

            return color;
        }

        private void UpdateTextColor(Text text, float current, float baseline)
        {
            if (!text)
                return;

            var defaultColor = GetDefaultColor(text);

            if (current > baseline)
                text.color = GameColors.Green;
            else if (current < baseline)
                text.color = GameColors.LightRed;
            else
                text.color = defaultColor;
        }
        
        [Header("GUI Texts")]
        [Tooltip("A reference to the Text component that represents the player's name.")]
        public Text characterNameText;

        [Tooltip("A reference to the Text component that represents the player's class.")]
        public Text classText;

        [Tooltip("A reference to the Text component that represents the Stats level.")]
        public Text levelText;

        [Tooltip("A reference to the Text component that represents the Stats available points.")]
        public Text availablePointsText;

        [Tooltip("A reference to the Text component that represents the Stats experience points.")]
        public Text experienceText;

        [Tooltip("A reference to the Text component that represents the Stats damage points.")]
        public Text damageText;

        [Tooltip("A reference to the Text component that represents the Stats defense points.")]
        public Text defenseText;

        [Tooltip("A reference to the Text component that represents the Stats attack speed. points")]
        public Text attackSpeedText;

        [Tooltip("A reference to the Text component that represents the Stats maximum health points.")]
        public Text maxHealthText;

        [Tooltip("A reference to the Text component that represents the Stats maximum mana points.")]
        public Text maxManaText;

        [Tooltip("A reference to the Text component that represents the Stats magic damage points.")]
        public Text magicDamageText;

        [Tooltip("A reference to the Text component that represents the Stats magic resistance points.")]
        public Text magicResistanceText;

        [Tooltip("A reference to the Text component that represents the Stats stun chance points.")]
        public Text stunChanceText;

        [Tooltip("A reference to the Text component that represents the Stats stun speed points.")]
        public Text stunSpeedText;
        
        [Tooltip("A reference to the Text component that represents the Stats critical chance.")]
        public Text criticalChanceText;

        [Tooltip("A reference to the Text component that represents the player's Bartle Type.")]
        public Text playerTypeText;

        [Header("GUI Attributes")]
        [Tooltip("A reference to the GUI Stats Attributes representing strength points.")]
        public GUIStatsAttribute strength;

        [Tooltip("A reference to the GUI Stats Attributes representing dexterity points.")]
        public GUIStatsAttribute dexterity;

        [Tooltip("A reference to the GUI Stats Attributes representing vitality points.")]
        public GUIStatsAttribute vitality;

        [Tooltip("A reference to the GUI Stats Attributes representing energy points.")]
        public GUIStatsAttribute energy;

        [Header("Main Buttons")]
        [Tooltip("A reference to the Apply button.")]
        public Button applyButton;

        [Tooltip("A reference to the Cancel button.")]
        public Button cancelButton;

        [Tooltip("A reference to the button that toggles the Extended Stats Window.")]
        public Button extendedStatsButton;

        [Tooltip("Button to reopen the Master Skill Tree.")]
        public Button masterSkillTreeButton;

        [Header("Sprites")]
        public Sprite[] playerTypeSprites;
        [SerializeField] private Image playerTypeIcon;

        public CharacterInstance characterInstance;

        protected Entity m_entity;
        private GUIWindow m_extendedStatsWindow;

        [Tooltip("A reference to the GUI Extended Stats Manager.")]
        public GUIExtendedStatsManager extendedStats;

        public event System.Action<int> onPointsChanged;

        protected int m_availablePoints;

        /// <summary>
        /// Returns the current available points to distribute.
        /// </summary>
        public int availablePoints
        {
            get
            {
                return m_availablePoints;
            }

            set
            {
                m_availablePoints = Mathf.Max(0, value);
                onPointsChanged?.Invoke(m_availablePoints);
                availablePointsText.text = m_availablePoints.ToString();
            }
        }

        /// <summary>
        /// Update Cancel & Apply buttons via checking if there are any points distributed.
        /// </summary>
        public void UpdateApplyCancelButtons()
        {
            bool shouldShow = strength.distributedPoints > 0 ||
                            dexterity.distributedPoints > 0 ||
                            vitality.distributedPoints > 0 ||
                            energy.distributedPoints > 0;

            applyButton.gameObject.SetActive(shouldShow);
            cancelButton.gameObject.SetActive(shouldShow);
        }

        public void UpdateMasterSkillTreeButton()
        {
            if (!masterSkillTreeButton)
                return;

            int currentTier = CharacterSpecializations.currentTier;
            var selected = CharacterSpecializations.GetSelected(currentTier);
            masterSkillTreeButton.gameObject.SetActive(selected != null);
        }

        private void ToggleExtendedStats()
        {
            if (!extendedStats && GUIWindowsManager.Instance != null)
                extendedStats = GUIWindowsManager.Instance.extendedStats;

            if (!extendedStats)
            {
                Debug.LogWarning("[GUIStatsManager] Extended Stats Manager reference is missing.");
                return;
            }

            if (!m_extendedStatsWindow)
                m_extendedStatsWindow = extendedStats.GetComponent<GUIWindow>();

            m_extendedStatsWindow?.Toggle();
        }

        //protected virtual void InitializeEntity() => m_entity = Level.instance.player;
        protected virtual void InitializeEntity()
        {
            if (Level.instance?.player == null)
            {
                Debug.LogError("Player instance in Level is null. Cannot initialize entity.");
                return;
            }

            m_entity = Level.instance.player;

            if (m_entity == null)
            {
                Debug.LogError("Entity instance is null. Cannot initialize entity.");
                return;
            }

            if (Game.instance.currentCharacter == null)
            {
                Debug.LogError("Current character in entity is null. Cannot initialize characterInstance.");
                return;
            }

            characterInstance = Game.instance.currentCharacter;
            Debug.Log($"CharacterInstance successfully initialized: {characterInstance.name}");
        }

        public void GetCharacterName(CharacterInstance character)
        {
            if (characterInstance == null)
            {
                Debug.LogError("CharacterInstance is null. Cannot load logs.");
                return;
            }
        }

        protected virtual void InitializeCallbacks()
        {
            m_entity.stats.onLevelUp.AddListener(Refresh);
            m_entity.stats.onRecalculate.AddListener(Refresh);
            m_entity.stats.onExperienceChanged.AddListener(() =>
                experienceText.text = $"{m_entity.stats.experience} / {m_entity.stats.nextLevelExp}");

            var buffManager = m_entity.GetComponent<EntityBuffManager>();
            if (buffManager != null)
            {
                buffManager.onBuffAdded.AddListener(_ => Refresh());
                buffManager.onBuffRemoved.AddListener(_ => Refresh());
            }
        }
        protected virtual void InitializeTexts()
        {
            if (characterInstance == null)
            {
                Debug.LogError("CharacterInstance is null. Cannot initialize texts.");
                return;
            }

            if (characterNameText != null)
            {
                characterNameText.text = characterInstance.name;
            }
            else
            {
                Debug.LogWarning("CharacterNameText is not assigned in the inspector.");
            }

            if (levelText != null)
            {
                levelText.text = m_entity?.stats?.level.ToString();
            }
            else
            {
                Debug.LogWarning("LevelText is not assigned in the inspector.");
            }

            availablePointsText.text = m_availablePoints.ToString();
        }


        /// <summary>
        /// Applies all distributed points to the Player's Stats Manager.
        /// </summary>
        public void Apply()
        {
            m_entity.stats.BulkDistribute(strength.distributedPoints, dexterity.distributedPoints, vitality.distributedPoints, energy.distributedPoints);
            Refresh();
            UpdateApplyCancelButtons();
        }

       /// <summary>
        /// Cancel all the changes, gives back all the distributed points.
        /// </summary>
        public void Cancel()
        {
            strength.Reset(m_entity.stats.strength, m_entity.stats.GetBaseStrength());
            dexterity.Reset(m_entity.stats.dexterity, m_entity.stats.GetBaseDexterity());
            vitality.Reset(m_entity.stats.vitality, m_entity.stats.GetBaseVitality());
            energy.Reset(m_entity.stats.energy, m_entity.stats.GetBaseEnergy());

            UpdateApplyCancelButtons();
        }

        /// <summary>
        /// Refreshes all attributes to display the current values.
        /// </summary>
        public virtual void Refresh()
        {
            if (m_entity == null || m_entity.stats == null)
		{
    			Debug.LogError("[StatsManager] Entity or stats are null during Refresh. Aborting.");
			return;
		}

        /*	
        string rawClassName = Game.instance.currentCharacter?.Entity != null
                        ? Game.instance.currentCharacter.Entity.name.Replace("(Clone)", "").Trim()
                        : "Unknown";
        */
            if (classText != null)
            {
                string classDisplayName = Game.instance.currentCharacter?.GetName() ?? "Unknown";
                classText.text = classDisplayName;
            }

            levelText.text = m_entity.stats.level.ToString();
            experienceText.text = $"{m_entity.stats.experience} / {m_entity.stats.nextLevelExp}";
            characterNameText.text = characterInstance.name;
            damageText.text = $"{m_entity.stats.minDamage} - {m_entity.stats.maxDamage}";
            defenseText.text = m_entity.stats.defense.ToString();
            attackSpeedText.text = $"{m_entity.stats.attackSpeed} / {Game.instance.maxAttackSpeed}";
            maxHealthText.text = m_entity.stats.maxHealth.ToString();
            maxManaText.text = m_entity.stats.maxMana.ToString();
            magicDamageText.text = $"{m_entity.stats.minMagicDamage} - {m_entity.stats.maxMagicDamage}";
            magicResistanceText.text = m_entity.stats.magicResistance.ToString();
            stunChanceText.text = $"{(m_entity.stats.stunChance * 100):F2}%";
            stunSpeedText.text = m_entity.stats.stunSpeed.ToString();
            criticalChanceText.text = $"{(m_entity.stats.criticalChance * 100):F2}%";

            availablePoints = m_entity.stats.availablePoints;

            var buffManager = m_entity.GetComponent<EntityBuffManager>();

            int baseStrength = m_entity.stats.strength;
            int baseDexterity = m_entity.stats.dexterity;
            int baseVitality = m_entity.stats.vitality;
            int baseEnergy = m_entity.stats.energy;
            int baseDefense = m_entity.stats.defense;
            int baseAttackSpeed = m_entity.stats.attackSpeed;
            int baseMaxHealth = m_entity.stats.maxHealth;
            int baseMaxMana = m_entity.stats.maxMana;
            int baseMinDamage = m_entity.stats.minDamage;
            int baseMaxDamage = m_entity.stats.maxDamage;
            int baseMinMagic = m_entity.stats.minMagicDamage;
            int baseMaxMagic = m_entity.stats.maxMagicDamage;
            int baseMagicResistance = m_entity.stats.magicResistance;

            if (buffManager)
            {
                foreach (var value in buffManager.GetStatModifiers(nameof(Buff.strength)).Values)
                    baseStrength -= value;

                foreach (var value in buffManager.GetStatModifiers(nameof(Buff.dexterity)).Values)
                    baseDexterity -= value;

                foreach (var value in buffManager.GetStatModifiers(nameof(Buff.vitality)).Values)
                    baseVitality -= value;

                foreach (var value in buffManager.GetStatModifiers(nameof(Buff.energy)).Values)
                    baseEnergy -= value;

                foreach (var value in buffManager.GetStatModifiers(nameof(Buff.defense)).Values)
                    baseDefense -= value;

                foreach (var value in buffManager.GetStatModifiers(nameof(EntityStatsManager.attackSpeed)).Values)
                    baseAttackSpeed -= value;

                int addHealth = 0;
                foreach (var value in buffManager.GetStatModifiers(nameof(Buff.additionalHealth)).Values)
                    addHealth += value;
                int addHealthPercent = 0;
                foreach (var value in buffManager.GetStatModifiers(nameof(Buff.additionalHealthPercent)).Values)
                    addHealthPercent += value;
                baseMaxHealth = Mathf.RoundToInt((baseMaxHealth - addHealth) / (1f + addHealthPercent / 100f));

                int addMana = 0;
                foreach (var value in buffManager.GetStatModifiers(nameof(Buff.additionalMana)).Values)
                    addMana += value;
                int addManaPercent = 0;
                foreach (var value in buffManager.GetStatModifiers(nameof(Buff.additionalManaPercent)).Values)
                    addManaPercent += value;
                baseMaxMana = Mathf.RoundToInt((baseMaxMana - addMana) / (1f + addManaPercent / 100f));

                foreach (var value in buffManager.GetStatModifiers(nameof(EntityStatsManager.minDamage)).Values)
                    baseMinDamage -= value;
                foreach (var value in buffManager.GetStatModifiers(nameof(EntityStatsManager.maxDamage)).Values)
                    baseMaxDamage -= value;

                foreach (var value in buffManager.GetStatModifiers(nameof(EntityStatsManager.minMagicDamage)).Values)
                    baseMinMagic -= value;
                foreach (var value in buffManager.GetStatModifiers(nameof(EntityStatsManager.maxMagicDamage)).Values)
                    baseMaxMagic -= value;

                foreach (var value in buffManager.GetStatModifiers(nameof(Buff.magicResistance)).Values)
                    baseMagicResistance -= value;
            }

            strength.Reset(m_entity.stats.strength, baseStrength);
            dexterity.Reset(m_entity.stats.dexterity, baseDexterity);
            vitality.Reset(m_entity.stats.vitality, baseVitality);
            energy.Reset(m_entity.stats.energy, baseEnergy);

            UpdateTextColor(defenseText, m_entity.stats.defense, baseDefense);
            UpdateTextColor(attackSpeedText, m_entity.stats.attackSpeed, baseAttackSpeed);
            UpdateTextColor(maxHealthText, m_entity.stats.maxHealth, baseMaxHealth);
            UpdateTextColor(maxManaText, m_entity.stats.maxMana, baseMaxMana);
            UpdateTextColor(magicResistanceText, m_entity.stats.magicResistance, baseMagicResistance);

            if (damageText)
            {
                if (m_entity.stats.minDamage > baseMinDamage || m_entity.stats.maxDamage > baseMaxDamage)
                    damageText.color = GameColors.Green;
                else if (m_entity.stats.minDamage < baseMinDamage || m_entity.stats.maxDamage < baseMaxDamage)
                    damageText.color = GameColors.LightRed;
                else
                    damageText.color = GetDefaultColor(damageText);
            }

            if (magicDamageText)
            {
                if (m_entity.stats.minMagicDamage > baseMinMagic || m_entity.stats.maxMagicDamage > baseMaxMagic)
                    magicDamageText.color = GameColors.Green;
                else if (m_entity.stats.minMagicDamage < baseMinMagic || m_entity.stats.maxMagicDamage < baseMaxMagic)
                    magicDamageText.color = GameColors.LightRed;
                else
                    magicDamageText.color = GetDefaultColor(magicDamageText);
            }

            string staticType = characterInstance.playerType;
            string dynamicType = PlayerBehaviorLogger.Instance?.currentDynamicPlayerType ?? "Unknown";
            string displayedType = (dynamicType == "Unknown" || dynamicType == "Undefined") ? staticType : dynamicType;

            if (playerTypeText != null)
            {
                playerTypeText.text = $"{displayedType}";
            }

                        if (playerTypeIcon != null)
            {
                if (!string.IsNullOrEmpty(displayedType) && displayedType != "Unknown" && displayedType != "Undefined")
                {
                    SetPlayerTypeIcon(displayedType, playerTypeIcon);
                    playerTypeIcon.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"Player type is undefined: {displayedType}");
                    playerTypeIcon.gameObject.SetActive(false);
                }
            }

            extendedStats?.Refresh();
            UpdateMasterSkillTreeButton();
        }
        
        private void SetPlayerTypeIcon(string playerType, Image iconImage)
        {
            iconImage.gameObject.SetActive(false);

            if (playerTypeSprites == null || playerTypeSprites.Length < 4)
            {
                Debug.LogWarning("PlayerTypeSprites array is not properly configured!");
                return;
            }

            switch (playerType)
            {
                case "Achiever":
                    iconImage.sprite = playerTypeSprites[0];
                    iconImage.gameObject.SetActive(true);
                    break;
                case "Killer":
                    iconImage.sprite = playerTypeSprites[1];
                    iconImage.gameObject.SetActive(true);
                    break;
                case "Explorer":
                    iconImage.sprite = playerTypeSprites[2];
                    iconImage.gameObject.SetActive(true);
                    break;
                case "Socializer":
                    iconImage.sprite = playerTypeSprites[3];
                    iconImage.gameObject.SetActive(true);
                    break;
                default:
                    Debug.LogWarning($"Unknown player type: {playerType}");
                    break;
            }
        }

        protected virtual void OnEnable()
        {
            if (characterInstance != null)
                characterInstance.onBuffsRestored -= Refresh;

            characterInstance = Game.instance?.currentCharacter;

            if (characterInstance != null)
                characterInstance.onBuffsRestored += Refresh;

            if (m_entity != null)
                Refresh();
        }

        protected virtual void OnDisable()
        {
            if (characterInstance != null)
                characterInstance.onBuffsRestored -= Refresh;
        }

        protected virtual void Start()
        {
            InitializeEntity();
            InitializeCallbacks();
            InitializeTexts();
            InitializeTooltips();
            Refresh();
            UpdateApplyCancelButtons();

            if (!extendedStats && GUIWindowsManager.Instance != null)
                extendedStats = GUIWindowsManager.Instance.extendedStats;

            if (!extendedStatsButton && cancelButton)
            {
                extendedStatsButton = Instantiate(cancelButton, cancelButton.transform.parent);
                extendedStatsButton.gameObject.SetActive(true);
                extendedStatsButton.name = "Extended";
                var txt = extendedStatsButton.GetComponentInChildren<Text>();
                if (txt) txt.text = "EXTENDED";
                extendedStatsButton.onClick.RemoveAllListeners();
            }

            if (extendedStatsButton)
                extendedStatsButton.onClick.AddListener(ToggleExtendedStats);
            
            if (masterSkillTreeButton)
            {
                masterSkillTreeButton.onClick.RemoveAllListeners();
                masterSkillTreeButton.onClick.AddListener(() =>
                    GUIWindowsManager.Instance.specializationsWindow?.GetComponent<GUIWindow>()?.Show());
            }

            UpdateMasterSkillTreeButton();
        }

        protected virtual void InitializeTooltips()
        {
            AttachTooltip(strength?.gameObject, "strength");
            AttachTooltip(strength?.pointsText?.gameObject, "strength");

            AttachTooltip(dexterity?.gameObject, "dexterity");
            AttachTooltip(dexterity?.pointsText?.gameObject, "dexterity");

            AttachTooltip(vitality?.gameObject, "vitality");
            AttachTooltip(vitality?.pointsText?.gameObject, "vitality");

            AttachTooltip(energy?.gameObject, "energy");
            AttachTooltip(energy?.pointsText?.gameObject, "energy");

            AttachTooltip(defenseText?.gameObject, "defense");
            AttachTooltip(magicResistanceText?.gameObject, "magicResistance");
            AttachTooltip(maxHealthText?.gameObject, "maxHealth");
            AttachTooltip(maxManaText?.gameObject, "maxMana");
            AttachTooltip(attackSpeedText?.gameObject, "attackSpeed");
            AttachTooltip(stunSpeedText?.gameObject, "stunSpeed");
            AttachTooltip(stunChanceText?.gameObject, "stunChance");
            AttachTooltip(criticalChanceText?.gameObject, "criticalChance");
            AttachTooltip(damageText?.gameObject, "minDamage");
            AttachTooltip(magicDamageText?.gameObject, "minMagicDamage");
        }


        protected virtual void AttachTooltip(GameObject go, string stat)
        {
            if (!go)
                return;

            var tooltip = go.GetComponent<GUIStatTooltip>() ?? go.AddComponent<GUIStatTooltip>();
            tooltip.statName = stat;
        }
    }
}
