using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using AI_DDA.Assets.Scripts;
using System;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Entity/Entity Stats Manager")]
    public partial class EntityStatsManager : MonoBehaviour
    {
        [HideInInspector] public int baseStrength;
        [HideInInspector] public int baseDexterity;
        [HideInInspector] public int baseVitality;
        [HideInInspector] public int baseEnergy;

        public bool wasBoosted = false;

        public UnityEvent onLevelUp;
        public UnityEvent onRecalculate;
        public UnityEvent onHealthChanged;
        public UnityEvent onManaChanged;
        public UnityEvent onExperienceChanged;

        [Header("General Settings")]
        [Tooltip("The initial level of this Entity.")]
        public int level = 1;

        [Tooltip("The initial strength of this Entity.")]
        public int strength = 20;
        
        [Tooltip("The initial dexterity of this Entity.")]
        public int dexterity = 15;

        [Tooltip("The initial vitality of this Entity.")]
        public int vitality = 15;

        [Tooltip("The initial energy of this Entity.")]
        public int energy = 10;

        [Header("Combo Settings")]
        [Tooltip("If true, the Entity will always use the base combo stats.")]
        public bool alwaysUseBaseComboStats;

        [Tooltip("The base maximum number of combos.")]
        public int baseMaxCombos = 1;

        [Tooltip("The base time it takes to stop a combo if no attack is performed.")]
        public float baseTimeToStopCombo = 1f;

        [Tooltip("The delay before performing the next combo when an attack is triggered.")]
        public float baseNextComboDelay = 0.1f;

        [Header("Bot Settings")]
        [Tooltip("If true, the Entity will be able to gain experience.")]
        public bool canGainExperience = true;

        [Tooltip("If true, the current health will never decrease.")]
        public bool infiniteHealth;

        [Tooltip("If true, the current mana will never decrease.")]
        public bool infiniteMana;

        [Tooltip("If true, the Entity will be immune to stun.")]
        public bool immuneToStun;

        public bool isNewlySpawned { get; set; } = true;

        protected int m_health;
        protected int m_mana;
        protected int m_experience;

        /// <summary>
        /// Returns true if the component was initialized.
        /// </summary>
        public bool initialized { get; protected set; }

        /// <summary>
        /// Returns the amount of experience to get to the next level.
        /// </summary>
        public int nextLevelExp { get; protected set; }

        /// <summary>
        /// Returns the amount of available distribution points.
        /// </summary>
        public int availablePoints { get; protected set; }

        /// <summary>
        /// Returns the maximum health points.
        /// </summary>
        public int maxHealth { get; protected set; }

        /// <summary>
        /// Returns the maximum mana points.
        /// </summary>
        public int maxMana { get; protected set; }

        /// <summary>
        /// Returns the minimum normal attack damage.
        /// </summary>
        public int minDamage { get; protected set; }

        /// <summary>
        /// Returns the minimum normal attack damage.
        /// </summary>
        public int maxDamage { get; protected set; }

        /// <summary>
        /// Returns the minimum magic damage.
        /// </summary>
        public int minMagicDamage { get; protected set; }

        /// <summary>
        /// Returns the maximum magic damage.
        /// </summary>
        public int maxMagicDamage { get; protected set; }
        public int magicResistance { get; protected set; }
        public int fireResistance { get; protected set; }
        public int waterResistance { get; protected set; }
        public int iceResistance { get; protected set; }
        public int earthResistance { get; protected set; }
        public int airResistance { get; protected set; }
        public int lightningResistance { get; protected set; }
        public int shadowResistance { get; protected set; }
        public int lightResistance { get; protected set; }
        public int arcaneResistance { get; protected set; }
        public int manaRegenPerSecond { get; protected set; }
        public int manaRegenPer5Seconds { get; protected set; }
        public int manaRegenPer30Seconds { get; protected set; }
        public int healthRegenPerSecond { get; protected set; }
        public int healthRegenPer5Seconds { get; protected set; }
        public int healthRegenPer30Seconds { get; protected set; }
        public int experiencePerSecondPercent { get; protected set; }
        public int experiencePer5SecondsPercent { get; protected set; }
        public int experiencePer30SecondsPercent { get; protected set; }
        public int additionalMana { get; protected set; }
        public int additionalManaPercent { get; protected set; }
        public int additionalHealth { get; protected set; }
        public int additionalHealthPercent { get; protected set; }
        public int increaseAttackSpeedPercent { get; protected set; }
        public int increaseAttackSpeedValue { get; protected set; }
        public int increaseDamagePercent { get; protected set; }
        public int increaseDamageValue { get; protected set; }
        public int increaseMagicalDamagePercent { get; protected set; }
        public int increaseMagicalDamageValue { get; protected set; }
        public int additionalExperienceRewardPercent { get; protected set; }
        public int additionalMoneyRewardPercent { get; protected set; }
        public bool magicImmunity { get; protected set; }
        public bool fireImmunity { get; protected set; }
        public bool waterImmunity { get; protected set; }
        public bool iceImmunity { get; protected set; }
        public bool earthImmunity { get; protected set; }
        public bool airImmunity { get; protected set; }
        public bool lightningImmunity { get; protected set; }
        public bool shadowImmunity { get; protected set; }
        public bool lightImmunity { get; protected set; }
        public bool arcaneImmunity { get; protected set; }
        public int additionalAmberlingsPerMinute { get; protected set; }
        public int additionalLunarisPerMinute { get; protected set; }
        public int additionalSolmiresPerMinute { get; protected set; }
        public int itemPricePercent { get; protected set; }

        /// <summary>
        /// Returns the maximum number of combos.
        /// </summary>
        public int maxCombos { get; protected set; }

        /// <summary>
        /// Returns the time it takes to stop a combo if no attack is performed.
        /// </summary>
        public float timeToStopCombo { get; protected set; }

        /// <summary>
        /// Returns the time it takes to perform the next combo.
        /// </summary>
        public float nextComboDelay { get; protected set; }

        /// <summary>
        /// Returns the current defense points.
        /// </summary>
        public int defense { get; protected set; }

        /// <summary>
        /// Returns the current attack speed.
        /// </summary>
        public int attackSpeed { get; protected set; }

        /// <summary>
        /// Returns the current critical chance in percentage.
        /// </summary>
        public float criticalChance { get; protected set; }

        /// <summary>
        /// Returns the current chance to block incoming attacks.
        /// </summary>
        public float chanceToBlock { get; protected set; }

        /// <summary>
        /// Returns the current block recover speed.
        /// </summary>
        public int blockSpeed { get; protected set; }

        /// <summary>
        /// Returns the current chance to stun an enemy when attacking.
        /// </summary>
        public float stunChance { get; protected set; }

        /// <summary>
        /// Returns the current stun recover speed.
        /// </summary>
        public int stunSpeed { get; protected set; }
        
        protected virtual void Awake()
        {
            Initialize();
        }

        protected virtual void Start()
        {
            var entity = GetComponent<Entity>();
            m_inventory = entity != null ? entity.inventory : null;
            Initialize();
        }

        /// <summary>
        /// Get or set the experience points.
        /// </summary>
        public int experience
        {
            get { return m_experience; }
            protected set
            {
                m_experience = value;
                onExperienceChanged?.Invoke();
            }
        }

        /// <summary>
        /// Get or set the health points.
        /// </summary>
        public int health
        {
            get
            {
                if (infiniteHealth)
                    return maxHealth;
                return m_health;
            }
            set
            {
                m_health = Mathf.Clamp(value, 0, maxHealth);
                onHealthChanged?.Invoke();
            }
        }

        /// <summary>
        /// Get or set the mana points.
        /// </summary>
        public int mana
        {
            get
            {
                if (infiniteMana)
                    return maxMana;

                return m_mana;
            }
            set
            {
                m_mana = Mathf.Clamp(value, 0, maxMana);
                onManaChanged?.Invoke();
            }
        }

        protected EntityItemManager m_items;
        protected EntitySkillManager m_skills;

        protected ItemFinalAttributes m_additionalAttributes;
        protected List<Entity> m_defeatedEntities = new();

        protected EntityInventory m_inventory;

        protected float m_manaRegenTimer1;
        protected float m_manaRegenRemainder;
        protected float m_manaRegenTimer5;
        protected float m_manaRegenRemainder5;
        protected float m_manaRegenTimer30;
        protected float m_manaRegenRemainder30;

        protected float m_healthRegenTimer1;
        protected float m_healthRegenRemainder;
        protected float m_healthRegenTimer5;
        protected float m_healthRegenRemainder5;
        protected float m_healthRegenTimer30;
        protected float m_healthRegenRemainder30;

        protected float m_experienceTimer1;
        protected float m_experienceRemainder;
        protected float m_experienceTimer5;
        protected float m_experienceRemainder5;
        protected float m_experienceTimer30;
        protected float m_experienceRemainder30;

        protected float m_moneyTimer;

        /// <summary>
        /// Returns true if the Entity is using a weapon.
        /// </summary>
        protected bool m_isUsingWeapon => m_items && m_items.IsUsingWeapon();

        /// <summary>
        /// Returns true if the Entity reached the maximum level.
        /// </summary>
        protected bool m_reachedMaxLevel => level >= Game.instance.maxLevel;

        /// <summary>
        /// Initializes the Stats Manager.
        /// </summary>
        /// 
        public virtual void Initialize()
        {
            if (initialized)
                return;

            // Debug.Log($"Initializing stats for {gameObject.name}");

            if (DifficultyManager.Instance == null)
            {
                Debug.LogError("DifficultyManager.Instance is null! Skipping stats adjustment.");
                return;
            }

            if (baseStrength <= 0)  baseStrength = strength;
            if (baseDexterity <= 0) baseDexterity = dexterity;
            if (baseVitality <= 0)  baseVitality  = vitality;
            if (baseEnergy <= 0)    baseEnergy    = energy;

            InitializeItems();
            InitializeSkills();
            Recalculate();
            Revitalize();

            initialized = true;

            /*
            if (gameObject.CompareTag("Entity/Enemy"))
            {
                ApplyDifficultyModifiers();
            }
            */

            // ValidateStatsDebug();    -   Validate function for enemy stats (bug already fixed)

            var entity = GetComponent<Entity>();
            if (entity && entity.nametag)
            {
                onLevelUp.AddListener(() =>
                {
                    entity.nametag.SetNametag(entity.name, level);
                });
            }
        }

        protected virtual void InitializeItems()
        {
            m_items = GetComponent<EntityItemManager>();

            if (m_items)
                m_items.onChanged.AddListener(Recalculate);
        }

        protected virtual void InitializeSkills() => m_skills = GetComponent<EntitySkillManager>();

        public void RecheckWeaponSkill()
        {
            if (m_items == null || m_skills == null)
                return;

            var right = m_items.GetRightHand();
            var left = m_items.GetLeftHand();

            foreach (var item in new[] { right, left })
            {
                if (item?.data is ItemWeapon weapon)
                {
                    var skill = weapon.skill;
                    var source = weapon.skillSource;

                    if (item.isSkillEnabled && skill != null && source != null)
                    {
                        m_skills.TryLearnSkill(source);
                    }
                }
            }
        }

        /// <summary>
        /// Bulk update all stats points and recalculate the stats.
        /// </summary>
        public virtual void BulkUpdate(
            int level,
            int strength,
            int dexterity,
            int vitality,
            int energy,
            int availablePoints,
            int experience
        )
        {
            this.level = level;
            this.strength = strength;
            this.dexterity = dexterity;
            this.vitality = vitality;
            this.energy = energy;
            this.availablePoints = availablePoints;
            this.experience = experience;
            Recalculate();
        }

        private int _lastKnownStrength;

        private void Update()
        {
            if (strength != _lastKnownStrength)
            {
                // Debug.LogWarning($"[EntityStatsManager - Update] {name} ID={GetInstanceID()} changed from {_lastKnownStrength} to {strength}");
                _lastKnownStrength = strength;
            }

            float dt = Time.deltaTime;

            ApplyOverTime(ref m_manaRegenTimer1, ref m_manaRegenRemainder, dt, 1f, manaRegenPerSecond, v => mana += v);
            ApplyOverTime(ref m_manaRegenTimer5, ref m_manaRegenRemainder5, dt, 5f, manaRegenPer5Seconds, v => mana += v);
            ApplyOverTime(ref m_manaRegenTimer30, ref m_manaRegenRemainder30, dt, 30f, manaRegenPer30Seconds, v => mana += v);

            ApplyOverTime(ref m_healthRegenTimer1, ref m_healthRegenRemainder, dt, 1f, healthRegenPerSecond, v => health += v);
            ApplyOverTime(ref m_healthRegenTimer5, ref m_healthRegenRemainder5, dt, 5f, healthRegenPer5Seconds, v => health += v);
            ApplyOverTime(ref m_healthRegenTimer30, ref m_healthRegenRemainder30, dt, 30f, healthRegenPer30Seconds, v => health += v);

            ApplyExperienceOverTime(ref m_experienceTimer1, ref m_experienceRemainder, dt, 1f, experiencePerSecondPercent);
            ApplyExperienceOverTime(ref m_experienceTimer5, ref m_experienceRemainder5, dt, 5f, experiencePer5SecondsPercent);
            ApplyExperienceOverTime(ref m_experienceTimer30, ref m_experienceRemainder30, dt, 30f, experiencePer30SecondsPercent);

            ApplyCurrencyOverTime(dt);
        }

        protected void ApplyOverTime(ref float timer, ref float remainder, float dt, float interval, float amount, System.Action<int> apply)
        {
            if (amount == 0f && remainder == 0f)
            {
                timer = 0f;
                return;
            }

            timer += dt;
            if (timer >= interval)
            {
                int ticks = Mathf.FloorToInt(timer / interval);
                timer -= interval * ticks;
                float total = amount * ticks + remainder;
                int delta = Mathf.FloorToInt(total);
                remainder = total - delta;
                if (delta != 0)
                    apply(delta);
            }
        }

        protected void ApplyExperienceOverTime(ref float timer, ref float remainder, float dt, float interval, float percent)
        {
            if (percent == 0f && remainder == 0f)
            {
                timer = 0f;
                return;
            }

            timer += dt;
            if (timer >= interval)
            {
                int ticks = Mathf.FloorToInt(timer / interval);
                timer -= interval * ticks;
                float total = nextLevelExp * percent / 100f * ticks + remainder;
                int amount = Mathf.FloorToInt(total);
                remainder = total - amount;
                if (amount != 0)
                    AddExperience(amount);
            }
        }

        protected void ApplyCurrencyOverTime(float dt)
        {
            if ((additionalAmberlingsPerMinute == 0 && additionalLunarisPerMinute == 0 && additionalSolmiresPerMinute == 0) || m_inventory == null)
            {
                m_moneyTimer = 0f;
                return;
            }

            m_moneyTimer += dt;
            if (m_moneyTimer >= 60f)
            {
                int ticks = Mathf.FloorToInt(m_moneyTimer / 60f);
                m_moneyTimer -= 60f * ticks;

                var inv = m_inventory.instance;
                if (additionalAmberlingsPerMinute != 0)
                    inv.currency.AddSpecificCurrency(CurrencyType.Amberlings, additionalAmberlingsPerMinute * ticks);
                if (additionalLunarisPerMinute != 0)
                    inv.currency.AddSpecificCurrency(CurrencyType.Lunaris, additionalLunarisPerMinute * ticks);
                if (additionalSolmiresPerMinute != 0)
                    inv.currency.AddSpecificCurrency(CurrencyType.Solmire, additionalSolmiresPerMinute * ticks);

                inv.onMoneyChanged?.Invoke();
            }
        }

        /// <summary>
        /// Bulk distribute the available distribution points and recalculate the stats.
        /// </summary>
        public virtual void BulkDistribute(int strength, int dexterity, int vitality, int energy)
        {
            this.strength += strength;
            this.dexterity += dexterity;
            this.vitality += vitality;
            this.energy += energy;
            this.availablePoints -= strength + dexterity + vitality + energy;
            Recalculate();
        }

        public ItemWeapon GetCurrentWeapon() => m_items?.GetWeapon();

        /// <summary>
        /// Gets the current health in a 0 to 1 range.
        /// </summary>
        public virtual float GetHealthPercent() => health / (float)maxHealth;

        /// <summary>
        /// Gets the current mana in a 0 to 1 range.
        /// </summary>
        public virtual float GetManaPercent() => mana / (float)maxMana;

        /// <summary>
        /// Returns the current experience in a 0 to 1 range.
        /// </summary>
        public virtual float GetExperiencePercent() => experience / (float)nextLevelExp;

        /// <summary>
        /// Calculates the normal attack damage points with the critical multiplier.
        /// </summary>
        /// <param name="critical">If true, the damage is critical.</param>
        public virtual int GetDamage(out bool critical) =>
            (int)(GetCriticalMultiplier(out critical) * GetFinalDamage());

        /// <summary>
        /// Calculates the magic attack damage points using a given skill with the critical multiplier.
        /// </summary>
        /// <param name="skill">The skill you want to calculate damage for.</param>
        /// <param name="critical">If true, the damage is critical.</param>
/*
        public virtual int GetSkillDamage(Skill skill, out bool critical) =>
           (int)(GetCriticalMultiplier(out critical) * GetSkillDamage(skill));
*/
        public virtual int GetSkillDamage(Skill skill, MagicElement element, out bool critical)
        {
            int minDamage = GetSkillDamage(skill, element, out bool minCritical, true);
            int maxDamage = GetSkillDamage(skill, element, out bool maxCritical, false);

            int finalDamage = UnityEngine.Random.Range(minDamage, maxDamage + 1);

            critical = minCritical || maxCritical;
            
            return finalDamage;
        }

        public int GetElementalBonus(MagicElement element)
        {
            if (element == MagicElement.None)
                return 0;

            var weapon = GetCurrentWeapon();

            int weaponBonus = weapon != null && weapon.magicElement == element
                ? weapon.minMagicDamage
                : 0;

            int passiveBonus = 0;

            return weaponBonus + passiveBonus;
        }

        /// <summary>
        /// Return the attack animation speed multiplier based on the attack speed stat.
        /// </summary>
        public virtual float GetAnimationAttackSpeed() =>
            attackSpeed / (float)Game.instance.maxAttackSpeed;

        /// <summary>
        /// Return the block animation speed multiplier based on the block speed stat.
        /// </summary>
        public virtual float GetAnimationBlockSpeed() =>
            blockSpeed / (float)Game.instance.maxBlockSpeed;

        /// <summary>
        /// Returns the stun speed multiplier based on the stun speed stat.
        /// </summary>
        public virtual float GetStunAnimationSpeed() =>
            stunSpeed / (float)Game.instance.maxStunSpeed;

        /// <summary>
        /// Returns the final normal damage points.
        /// </summary>
        protected virtual int GetFinalDamage() =>
            (int)(UnityEngine.Random.Range(minDamage, maxDamage) * m_additionalAttributes.damageMultiplier);

        /// <summary>
        /// Returns the final magical damage points.
        /// </summary>
        protected virtual int GetFinalMagicDamage() =>
            (int)(
                UnityEngine.Random.Range(minMagicDamage, maxMagicDamage)
                * m_additionalAttributes.damageMultiplier
            );

        public void RecalculateMagicResistance()
        {
            magicResistance = CalculateMagicResistance();
        }

        protected virtual int GetMagicResistance()
        {
            if (m_items == null)
                return 0;

            return m_items.GetMagicResistance();
        }

        protected virtual int GetElementalResistance(MagicElement element)
        {
            if (m_items == null)
                return 0;

            return m_items.GetElementalResistance(element);
        }

        /// <summary>
        /// Returns the magic damage points given a skill.
        /// </summary>
        /// <param name="skill">The skill you want to calculate magic damage from.</param>
        public virtual int GetSkillDamage(Skill skill, MagicElement element, out bool isCritical, bool calculateMin)
        {
            if (!skill || !skill.IsAttack())
            {
                isCritical = false;
                return 0;
            }

            var attack = skill.AsAttack();
            var entity = GetComponent<Entity>();

            if (entity == null)
            {
                isCritical = false;
                return calculateMin ? attack.minDamage : attack.maxDamage;
            }

            int damage = calculateMin ? attack.minDamage : attack.maxDamage;

            switch (attack.damageMode)
            {
                default:
                case SkillAttack.DamageMode.Regular:
                    damage += minDamage;
                    break;
                case SkillAttack.DamageMode.Magic:
                    damage += minMagicDamage;
                    break;
            }

            isCritical = UnityEngine.Random.value < entity.stats.criticalChance;
            if (isCritical)
            {
                damage = Mathf.RoundToInt(damage * Game.instance.criticalMultiplier);
            }

            var resistance = CalculateElementResistance(element);
            var final = Mathf.Max(0, damage - resistance);
            if (resistance > 0)
            {
                // Debug.Log($"[GetSkillDamage] Base:{damage} Resistance:{resistance} Final:{final} Element:{element}");
            }

            return final;
        }

        /// <summary>
        /// Returns the maximum number of combos based on the equipped weapon.
        /// </summary>
        protected virtual int GetItemsMaxCombos()
        {
            if (alwaysUseBaseComboStats || !m_isUsingWeapon)
                return baseMaxCombos;

            return m_items.GetWeapon().maxCombos;
        }

        /// <summary>
        /// Returns the time it takes to stop a combo based on the equipped weapon.
        /// </summary>
        protected float GetTimeToStopCombo()
        {
            if (alwaysUseBaseComboStats || !m_isUsingWeapon)
                return baseTimeToStopCombo;

            return m_items.GetWeapon().timeToStopCombo;
        }

        /// <summary>
        /// Returns the time it takes to perform the next combo based on the equipped weapon.
        /// </summary>
        protected float GetNextComboDelay()
        {
            if (alwaysUseBaseComboStats || !m_isUsingWeapon)
                return baseNextComboDelay;

            return m_items.GetWeapon().nextComboDelay;
        }

        /// <summary>
        /// Returns the minimum and maximum damage calculated from the equipped items.
        /// </summary>
        protected virtual MinMax GetItemsDamage()
        {
            if (!m_items)
                return MinMax.Zero;

            return m_items.GetDamage();
        }

        protected virtual MinMax GetMagicDamage()
        {
            if (!m_items)
                return MinMax.Zero;

            return m_items.GetMagicDamage();
        }

        /// <summary>
        /// Returns the defense points calculated from the equipped items.
        /// </summary>
        protected virtual int GetItemsDefense()
        {
            if (!m_items)
                return 0;

            return m_items.GetDefense();
        }

        /// <summary>
        /// Returns the attack speed points calculated from the equipped items.
        /// </summary>
        protected virtual int GetItemsAttackSpeed()
        {
            if (!m_items)
                return 0;

            return m_items.GetAttackSpeed();
        }

        /// <summary>
        /// Returns the chance to block from the equipped items.
        /// </summary>
        protected virtual float GetItemsChanceToBlock()
        {
            if (!m_items)
                return 0;

            return m_items.GetChanceToBlock();
        }

        /// <summary>
        /// Sets all the attribute points from the equipped items.
        /// </summary>
        protected virtual void SetAdditionalAttributes()
        {
            m_additionalAttributes = m_items ? m_items.GetFinalAttributes() : new();
        }

        /// <summary>
        /// Calculates and return the critical multiplier in percentage.
        /// </summary>
        /// <param name="success">If true, the critical is successful.</param>
        protected virtual float GetCriticalMultiplier(out bool success)
        {
            success = UnityEngine.Random.value > 1 - criticalChance;
            return success ? Game.instance.criticalMultiplier : 1;
        }

        public virtual int CalculateSkillDamage(Skill skill, bool isMax)
        {
            if (!skill || !skill.IsAttack())
                return 0;

            var attack = skill.AsAttack();
            int baseDamage = isMax ? attack.maxDamage : attack.minDamage;

            switch (attack.damageMode)
            {
                case SkillAttack.DamageMode.Regular:
                    baseDamage += maxDamage;
                    break;
                case SkillAttack.DamageMode.Magic:
                    baseDamage += maxMagicDamage;
                    break;
            }

            return Mathf.RoundToInt(baseDamage);
        }

        /// <summary>
        /// Metody do pobierania bazowych wartości
        /// </summary>
        public int GetBaseStrength()
        {
            return baseStrength;
        }

        public int GetBaseDexterity()
        {
            return baseDexterity;
        }

        public int GetBaseVitality()
        {
            return baseVitality;
        }

        public int GetBaseEnergy()
        {
            return baseEnergy;
        }

        /// <summary>
        /// Recalculates the points for all the Entity's dynamic stats.
        /// </summary>
        public virtual void Recalculate()
        {
            SetAdditionalAttributes();

            var magicDamage = CalculateMagicDamage();
            var damage = CalculateDamage();

            nextLevelExp = CalculateNextLevelExperience();
            maxHealth = CalculateMaxHealth();
            maxMana = CalculateMaxMana();
            minDamage = damage.min;
            maxDamage = damage.max;
            maxCombos = GetItemsMaxCombos();
            timeToStopCombo = GetTimeToStopCombo();
            nextComboDelay = GetNextComboDelay();
            attackSpeed = CalculateAttackSpeed();
            minMagicDamage = magicDamage.min;
            maxMagicDamage = magicDamage.max;
            magicResistance = CalculateMagicResistance();
            criticalChance = CalculateCriticalChance();
            chanceToBlock = CalculateChanceToBlock();
            blockSpeed = CalculateBlockSpeed();
            defense = CalculateDefense();
            stunChance = CalculateStunChance();
            stunSpeed = CalculateStunSpeed();
            health = Mathf.Min(health, maxHealth);
            mana = Mathf.Min(mana, maxMana);
            
            onRecalculate?.Invoke();

            onRecalculate.RemoveListener(RecheckWeaponSkill);
            onRecalculate.AddListener(RecheckWeaponSkill);
        }

        /// <summary>
        /// Restores the current health and mana points to its maximum values.
        /// </summary>
        public virtual void Revitalize()
        {
            ResetHealth();
            ResetMana();
        }

        /// <summary>
        /// Sets the health points to its maximum value.
        /// </summary>
        public virtual void ResetHealth() => health = maxHealth;

        /// <summary>
        /// Sets the mana points to its maximum value.
        /// </summary>
        public virtual void ResetMana() => mana = maxMana;

        /// <summary>
        /// Level Up the Entity, consuming the experience points, and recalculating all its dynamic stats.
        /// </summary>
        public virtual void LevelUp()
        {
            if (m_reachedMaxLevel)
                return;

            while (experience >= nextLevelExp)
            {
                level++;
                experience -= nextLevelExp;
                availablePoints += Game.instance.levelUpPoints;
                nextLevelExp = CalculateNextLevelExperience();
            }

            Recalculate();
            Revitalize();

            onLevelUp?.Invoke();
        }

        /// <summary>
        /// Add experience points to the Entity.
        /// </summary>
        /// <param name="amount">The amount of experience points you want to add.</param>
        public virtual void AddExperience(int amount)
        {
            if (!canGainExperience || m_reachedMaxLevel)
                return;

            if (additionalExperienceRewardPercent != 0)
                amount = Mathf.RoundToInt(amount * (1f + additionalExperienceRewardPercent / 100f));

            experience += amount;

            if (experience >= nextLevelExp)
                LevelUp();

            // PlayerBehaviorLogger.Instance.CheckPlayerLevel();
            PlayerBehaviorLogger.Instance.achievementManager?.CheckAchievements(PlayerBehaviorLogger.Instance);
        }

        /// <summary>
        /// Set the current experience points to zero.
        /// </summary>
        public virtual void ResetExperience() => experience = 0;

        /// <summary>
        /// Calculates and sets the experience points acquired by defeating a given Entity.
        /// </summary>
        /// <param name="other">The Entity you want to use as a base to the calculation.</param>
        public virtual void OnDefeatEntity(Entity other)
        {
            if (m_defeatedEntities.Contains(other))
                return;

            m_defeatedEntities.Add(other);

            int gainedExp = CalculateEnemyExperience(other);
            
            AddExperience(gainedExp);
            // Debug.Log($"[EXP] Pokonano {other.name}, zdobyto {gainedExp} EXP");
        }

        public void ForceLevelUp(int amount = 1)
        {
            if (m_reachedMaxLevel)
                return;

            level = Mathf.Min(level + amount, Game.instance.maxLevel);
            availablePoints += Game.instance.levelUpPoints * amount;

            Recalculate();
            Revitalize();

            onLevelUp?.Invoke();
        }

        public bool IsMaxLevel()
        {
            return m_reachedMaxLevel;
        }
    }
}
