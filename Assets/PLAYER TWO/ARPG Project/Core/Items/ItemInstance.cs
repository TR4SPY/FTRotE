using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [System.Serializable]
    public partial class ItemInstance
    {
        /// <summary>
        /// Invoked when the durability changed.
        /// </summary>
        public System.Action onChanged;

        /// <summary>
        /// Invoked when the stack size changed.
        /// </summary>
        public System.Action onStackChanged;

        /// <summary>
        /// Invoked when the durability points reach zero.
        /// </summary>
        public System.Action onBreak;

        /// <summary>
        /// The Item data that represents this Item Instance.
        /// </summary>
        public Item data;

        /// <summary>
        /// The additional attributes of this Item Instance.
        /// </summary>
        public ItemAttributes attributes;

        protected int m_stack;

        /// <summary>
        /// The current durability points of this Item Instance.
        /// </summary>
        public int durability { get; protected set; }

        /// <summary>
        /// The size of the item stack.
        /// </summary>
        public int stack
        {
            get { return m_stack; }

            set
            {
                if (!IsStackable()) return;

                m_stack = Mathf.Clamp(value, 0, data.stackCapacity);
                onStackChanged?.Invoke();
            }
        }

        /// <summary>
        /// The amount of rows this Item Instance takes on the Inventory.
        /// </summary>
        public int rows => data.rows;

        /// <summary>
        /// The amount of columns this Item Instance takes on the Inventory.
        /// </summary>
        public int columns => data.columns;

        public GameObject ModelPrefab => data.prefab;
        
        public ItemInstance(Item data, bool generateAttributes = true,
            int minAttributes = 0, int maxAttributes = 0)
        {
            SetDefaultData(data);

            if (generateAttributes)
                GenerateAdditionalAttributes(minAttributes, maxAttributes);
        }

        public ItemInstance(Item data, ItemAttributes attributes)
        {
            this.data = data;
            this.attributes = attributes;

            if (IsEquippable())
                durability = GetEquippable().maxDurability;

            if (IsStackable())
                stack = 1;
        }

        public ItemInstance(Item data, int durability, int stack, bool generateAttributes = true)
        {
            this.data = data;
            this.durability = durability;
            this.stack = stack;

            if (generateAttributes)
                GenerateAdditionalAttributes();
        }

        public ItemInstance(Item data, ItemAttributes attributes, int durability, int stack)
        {
            this.data = data;
            this.attributes = attributes;
            this.durability = durability;
            this.stack = stack;
        }

        /// <summary>
        /// Tries to stack another item on the stack.
        /// </summary>
        /// <param name="other">The Item Instance you want to try stack.</param>
        /// <returns>Returns true if it was able to stack the item.</returns>
        public bool TryStack(ItemInstance other)
        {
            if (other == null)
            {
                // Debug.LogWarning("[STACK] TryStack failed: Other item is null.");
                return false;
            }

            if (!CanStack(other))
            {
                // Debug.LogWarning($"[STACK] TryStack failed: Cannot stack {other.GetName()} with {GetName()}.");
                return false;
            }

            int maxStack = data.stackCapacity;
            if (stack >= maxStack)
            {
                // Debug.LogWarning($"[STACK] TryStack failed: Stack is already full ({stack}/{maxStack}).");
                return false;
            }

            int amountToAdd = Mathf.Min(maxStack - stack, other.stack);
            stack += amountToAdd;
            other.stack -= amountToAdd;

            // Debug.Log($"[STACK] Stacking successful! {GetName()} stack is now {stack}/{maxStack}. Other stack: {other.stack}");

            if (other.stack == 0)
            {
                // Debug.Log($"[STACK] {other.GetName()} stack in inventory is now 0. Removing from inventory.");
                Level.instance.player.inventory.instance.TryRemoveItem(other);
                GUI.instance.GetComponentInChildren<GUIInventory>()?.UpdateSlots();
            }

            return true;
        }

        public string GetName()
        {
            return data != null ? data.GetName() : "Unknown Item";
        }

        /// <summary>
        /// Returns the required minimum level to equip this Item.
        /// </summary>
        public virtual int GetRequiredLevel()
        {
            if (IsEquippable()) return GetEquippable().requiredLevel;
            if (IsSkill()) return GetSkill().requiredLevel;

            return 0;
        }

        public bool IsClassAllowed(CharacterClassRestrictions playerClass)
        {
            if (data == null) return true;

            var allowed = data.allowedClasses;

            if (allowed == CharacterClassRestrictions.None)
                return true;

            return (allowed & playerClass) != 0;
        }

        /// <summary>
        /// Returns the required minimum strength to equip this Item.
        /// </summary>
        public virtual int GetRequiredStrength()
        {
            if (IsEquippable()) return GetEquippable().requiredStrength;
            if (IsSkill()) return GetSkill().requiredStrength;

            return 0;
        }

        /// <summary>
        /// Returns the required minimum dexterity to equip this Item.
        /// </summary>
        public virtual int GetRequiredDexterity()
        {
            if (IsEquippable()) return GetEquippable().requiredDexterity;

            return 0;
        }

        /// <summary>
        /// Returns the required minimum energy to equip this Item.
        /// </summary>
        public virtual int GetRequiredEnergy()
        {
            if (IsSkill()) return GetSkill().requiredEnergy;

            return 0;
        }

        /// <summary>
        /// Returns true if this Item Instance can stack another given Item Instance.
        /// </summary>
        /// <param name="other">The Item Instance you want to check.</param>
        public virtual bool CanStack(ItemInstance other) =>
            IsStackable() && other.data == data && stack + other.stack <= data.stackCapacity;

        /// <summary>
        /// Returns true if the durability points of this Item Instance is zero.
        /// </summary>
        public virtual bool IsBroken() => durability == 0;

        /// <summary>
        /// Returns true if the durability of this Item Instance is at half.
        /// </summary>
        public virtual bool IsAboutToBreak()
        {
            if (!IsEquippable()) return false;

            return durability <= GetEquippable().maxDurability / 2f;
        }

        /// <summary>
        /// Returns true if this Item Instance has additional attributes.
        /// </summary>
        public virtual bool ContainAttributes() => IsEquippable() && attributes != null;

        /// <summary>
        /// Returns true if it's allowed to read the additional attributes from this Item Instance.
        /// </summary>
        public virtual bool UseAttributes() => ContainAttributes() && !IsBroken();

        /// <summary>
        /// Reduces the durability of this Item Instance by a given amount.
        /// </summary>
        /// <param name="amount">The amount of points to decrease from the durability.</param>
        public virtual void ApplyDamage(int amount)
        {
            if (!IsEquippable()) return;

            var maxDurability = GetEquippable().maxDurability;
            durability = Mathf.Clamp(durability - amount, 0, maxDurability);

            if (durability <= 0)
                onBreak?.Invoke();

            onChanged?.Invoke();
        }

        /// <summary>
        /// Returns the minimum and maximum damage of this Item Instance. If the Item is broken or if its
        /// not a Weapon, the damage will always be zero. If it's about to break, the damage is reduced by half.
        /// </summary>
        public virtual MinMax GetDamage()
        {
            if (!IsWeapon() || IsBroken()) return MinMax.Zero;

            var minDamage = GetWeapon().minDamage;
            var maxDamage = GetWeapon().maxDamage;

            if (IsAboutToBreak())
                return new((int)(minDamage / 2f), (int)(maxDamage / 2f));

            return new(minDamage, maxDamage);
        }

        /// <summary>
        /// Returns the magic damage of this item instance.
        /// </summary>
        public virtual MinMax GetMagicDamage()
        {
            if (!IsWeapon() || IsBroken()) return MinMax.Zero;

            var minMagic = GetWeapon().minMagicDamage;
            var maxMagic = GetWeapon().maxMagicDamage;

            if (IsAboutToBreak())
                return new MinMax((int)(minMagic / 2f), (int)(maxMagic / 2f));

            return new MinMax(minMagic, maxMagic);
        }

        public int GetAdditionalMagicDamage()
        {
            if (data is ItemWeapon weapon)
                return weapon.minMagicDamage; // Pobiera magiczne obrażenia z broni

            return 0;
        }

        public float GetMagicDamageMultiplier()
        {
            return attributes != null ? attributes.magicDamagePercent / 100f : 1f;
        }

        public int GetAdditionalMagicResistance()
        {
            return attributes != null ? attributes.magicResistance : 0;
        }

        /// <summary>
        /// Returns true if this weapon has magic damage.
        /// </summary>
        public bool HasMagicDamage() => IsWeapon() && (GetWeapon().minMagicDamage > 0 || GetWeapon().maxMagicDamage > 0);

        /// <summary>
        /// Returns the defense points of this Item Instance. If it's broken, the defense is zero.
        /// If the Item Instance is about to break, the defense is reduced by half.
        /// </summary>
        public virtual int GetDefense()
        {
            if (IsBroken()) return 0;

            var defense = 0;

            if (IsArmor())
                defense = GetArmor().defense;
            else if (IsShield())
                defense = GetShield().defense;

            return IsAboutToBreak() ? (int)(defense / 2f) : defense;
        }

        /// <summary>
        /// Sets this Item Instance durability to its maximum points.
        /// </summary>
        public virtual void Repair()
        {
            if (!IsEquippable()) return;

            durability = GetEquippable().maxDurability;
            onChanged?.Invoke();
        }

        /// <summary>
        /// Returns the current durability in a rate of zero to one.
        /// </summary>
        public virtual float GetDurabilityRate()
        {
            if (!IsEquippable()) return 1;

            return durability / (float)GetEquippable().maxDurability;
        }

        protected virtual void SetDefaultData(Item data)
        {
            this.data = data;

            if (IsEquippable())
                durability = GetEquippable().maxDurability;

            if (IsStackable())
                stack = 1;
        }

        /// <summary>
        /// Randomly generates additional attributes for this Item Instance if it's a equipment.
        /// </summary>
        /// <param name="minAttributes">The minimum amount of additional attributes.</param>
        /// <param name="maxAttributes">The maximum amount of additional attributes.</param>
        protected virtual void GenerateAdditionalAttributes(int minAttributes = 0, int maxAttributes = 0)
        {
            if (!IsEquippable()) return;

            if (IsWeapon())
                attributes = new WeaponAttributes(minAttributes, maxAttributes);
            else if (IsArmor() || IsShield())
                attributes = new ArmorAttributes(minAttributes, maxAttributes);
        }

        /// <summary>
        /// Returns the selling price of this Item Instance.
        /// </summary>
        public virtual int GetSellPrice() => (int)(GetPrice() / 2f);

        /// <summary>
        /// Returns the price of this Item Instance. If it's a stack, the price is multiplier
        /// by the stack size. The durability rate of the Item Instance is multiplied by its final price.
        /// </summary>
        public virtual int GetPrice()
        {
            var price = data.price;

            if (IsStackable()) price *= stack;

            if (IsEquippable())
            {
                if (attributes != null)
                {
                    var totalAttr = attributes.GetAttributesCount();
                    price += totalAttr * Game.instance.pricePerAttribute;
                }

                price = (int)(price * GetDurabilityRate());
            }

            return price;
        }

        protected string InspectRequired(string name, int required, int current, Color error, bool breakLine)
        {
            var lineBreak = breakLine ? "\n" : "";
            var attr = $"Required {name}: {required}";

            if (current < required)
                return lineBreak + StringUtils.StringWithColor(attr, error);

            return lineBreak + attr;
        }

        /// <summary>
        /// Returns a string with the Item's general attributes.
        /// </summary>
        /// <param name="stats">The Entity Stats to compare against.</param>
        /// <param name="warning">The color of warning texts.</param>
        /// <param name="error">The color of the error texts.</param>
        public virtual string Inspect(EntityStatsManager stats, Color warning, Color error, Color special, Color quest)
        {
            var text = "";

            if (IsArmor())
            {
                text += $"Defense: {GetArmor().defense}";

                if (GetArmor().magicResistance > 0)
                    text += $"\nMagic Resistance: {GetArmor().magicResistance}";
            }
            else if (IsShield())
            {
                text += $"Defense: {GetShield().defense}";
                text += $"\nChance To Block: {GetShield().chanceToBlock}%";

                if (GetShield().magicResistance > 0)
                    text += $"\nMagic Resistance: {GetShield().magicResistance}";
            }
            else if (IsWeapon())
            {
                var blade = GetBlade();
                if (blade != null)
                {
                    var weaponType = blade.IsTwoHanded() ? "Two-Handed Weapon\n" : "One-Handed Weapon\n";
                    text += $"{StringUtils.StringWithColor(weaponType, special)}";
                }
                else
                {
                    text += $"{StringUtils.StringWithColor("Two-Handed Weapon\n", special)}";
                }

                text += $"Damage: {GetWeapon().minDamage} ~ {GetWeapon().maxDamage}";
                text += $"\nAttack Speed: {GetWeapon().attackSpeed}";

                if (GetWeapon().minMagicDamage > 0 || GetWeapon().maxMagicDamage > 0)
                    text += $"\nMagic Damage: {GetWeapon().minMagicDamage} ~ {GetWeapon().maxMagicDamage}";
            }  

            if (IsEquippable())
            {
                var lineBreak = text.Length > 0 ? "\n" : "";
                var attr = $"Durability: {durability} of {GetEquippable().maxDurability}";

                if (IsAboutToBreak())
                    text += lineBreak + StringUtils.StringWithColor(attr, warning);
                else if (IsBroken())
                    text += lineBreak + StringUtils.StringWithColor(attr, error);
                else
                    text += lineBreak + attr;
            }

            if (data.allowedClasses != CharacterClassRestrictions.None)
            {
                if (GetRequiredLevel() > 1)
                    text += InspectRequired("Level", GetRequiredLevel(), stats.level, error, text.Length > 0);

                if (GetRequiredStrength() > 0)
                    text += InspectRequired("Strength", GetRequiredStrength(), stats.strength, error, text.Length > 0);

                if (GetRequiredDexterity() > 0)
                    text += InspectRequired("Dexterity", GetRequiredDexterity(), stats.dexterity, error, text.Length > 0);

                if (GetRequiredEnergy() > 0)
                    text += InspectRequired("Energy", GetRequiredEnergy(), stats.energy, error, text.Length > 0);
            }

            if (data != null)
            {
                if (data.isQuestSpecific && data is not ItemJewel)
                {
                    text += $"\n{StringUtils.StringWithColorAndStyle("\nThis is a Quest Item", quest, bold: true)}";
                }

                if (data.cannotBeDropped)
                {
                    text += $"\n{StringUtils.StringWithColor("Cannot be dropped", error)}";
                }

                if (data.cannotBeSold)
                {
                    text += $"\n{StringUtils.StringWithColor("Cannot be sold", error)}";
                }
            }

            return text;
        }

        /// <summary>
        /// Returns a new Item Instance from the Item Serializer.
        /// </summary>
        /// <param name="serializer">The Item Serializer to create the Item Instance from.</param>
        public static ItemInstance CreateFromSerializer(ItemSerializer serializer)
        {
            if (serializer == null || serializer.itemId < 0) return null;

            var item = GameDatabase.instance.FindElementById<Item>(serializer.itemId);
            var attributes = ItemAttributes.CreateFromSerializer(serializer.attributes);

            return new ItemInstance(item, attributes, serializer.durability, serializer.stack);
        }
    }
}
