using UnityEngine;
using UnityEngine.UI;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Stats Manager")]
    public class GUIStatsManager : MonoBehaviour
    {
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


        [Header("Sprites")]
        public Sprite[] playerTypeSprites; 
        [SerializeField] private Image playerTypeIcon;

        public CharacterInstance characterInstance;

        protected Entity m_entity;

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
            strength.Reset(m_entity.stats.strength);
            dexterity.Reset(m_entity.stats.dexterity);
            vitality.Reset(m_entity.stats.vitality);
            energy.Reset(m_entity.stats.energy);

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

	string rawClassName = Game.instance.currentCharacter?.Entity != null
                ? Game.instance.currentCharacter.Entity.name.Replace("(Clone)", "").Trim()
                : "Unknown";

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
            strength.Reset(m_entity.stats.strength);
            dexterity.Reset(m_entity.stats.dexterity);
            vitality.Reset(m_entity.stats.vitality);
            energy.Reset(m_entity.stats.energy);

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

        protected virtual void Start()
        {
            InitializeEntity();
            InitializeCallbacks();
            InitializeTexts();
            Refresh();
            UpdateApplyCancelButtons();
        }
    }
}
