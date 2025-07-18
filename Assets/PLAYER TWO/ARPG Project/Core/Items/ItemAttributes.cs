using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    public class ItemAttributes
    {
        public int damage;
        public int damagePercent;
        public int magicDamage;
        public int magicDamagePercent;
        public int magicResistance;
        public int attackSpeed;
        public int critical;
        public int defense;
        public int defensePercent;
        public int mana;
        public int manaPercent;
        public int health;
        public int healthPercent;

        protected int[] m_points = new int[] { 2, 4, 6, 8, 10, 12 };
        protected int[] m_percentages = new int[] { 3, 5, 8, 13, 21 };

        protected FieldInfo[] m_fields;

        protected string m_tempName;
        protected object m_tempValue;

        protected virtual int GetRandomPoint() =>
            m_points[Random.Range(0, m_points.Length)];

        protected virtual int GetRandomPercentage() =>
            m_percentages[Random.Range(0, m_percentages.Length)];

        protected virtual bool CanAddAttribute(int iteration,
            int attributesAmount, int minAttributes, int maxAttributes)
        {
            if (maxAttributes > 0 && iteration > maxAttributes - 1) return false;
            if (minAttributes > 0 && iteration < minAttributes) return true;

            return CanAddAttribute(iteration, attributesAmount);
        }

        protected virtual bool CanAddAttribute(int iteration, int attributesAmount) =>
            Random.value <= Mathf.Lerp(Game.instance.maxAttributeChance,
                Game.instance.minAttributeChance, iteration / (float)attributesAmount);

        protected virtual List<T> Shuffle<T>(List<T> dictionary)
        {
            var random = new System.Random();
            return dictionary.OrderBy(e => random.Next()).ToList();
        }

        protected virtual string GetAttributeText(string name, int value)
        {
            if (name.Contains("Percent"))
            {
                name = name.Replace("Percent", "");
                name = StringUtils.ConvertToTitleCase(name);
                return $"Increases {name} by +{value}%";
            }

            name = StringUtils.ConvertToTitleCase(name);
            return $"+{value} of Additional {name}";
        }

        public virtual FieldInfo[] GetFields()
        {
            if (m_fields == null)
                m_fields = typeof(ItemAttributes).GetFields();

            return m_fields;
        }

        public virtual float GetDamageMultiplier() => damagePercent / 100f;
        public virtual float GetMagicDamageMultiplier() => magicDamagePercent / 100f;
        public virtual float GetCriticalMultiplier() => critical / 100f;
        public virtual float GetDefenseMultiplier() => defensePercent / 100f;
        public virtual float GetManaMultiplier() => manaPercent / 100f;
        public virtual float GetHealthMultiplier() => healthPercent / 100f;

        public virtual int GetAttributesCount()
        {
            var count = 0;

            foreach (var field in GetFields())
            {
                m_tempValue = field.GetValue(this);

                if (m_tempValue is int && (int)m_tempValue > 0)
                    count++;
            }

            return count;
        }

        public virtual string Inspect()
        {
            var text = "";

            foreach (var field in GetFields())
            {
                m_tempName = field.Name;
                m_tempValue = field.GetValue(this);

                if (!(m_tempValue is int) || (int)m_tempValue <= 0)
                    continue;

                if (text.Length > 0)
                    text += "\n";

                text += GetAttributeText(m_tempName, (int)m_tempValue);
            }

            return text;
        }

        public virtual void AddRandom()
        {
            var fields = GetFields().Where(f => f.FieldType == typeof(int)).ToList();
            if (fields.Count == 0) return;

            var randomField = fields[Random.Range(0, fields.Count)];
            int value = (randomField.Name.Contains("Percent")) ? GetRandomPercentage() : GetRandomPoint();
            int current = (int)randomField.GetValue(this);
            randomField.SetValue(this, current + value);
        }

        public virtual ItemAttributes Clone()
        {
            return new ItemAttributes()
            {
                damage = this.damage,
                damagePercent = this.damagePercent,
                magicDamage = this.magicDamage,
                magicDamagePercent = this.magicDamagePercent,
                magicResistance = this.magicResistance,
                attackSpeed = this.attackSpeed,
                critical = this.critical,
                defense = this.defense,
                defensePercent = this.defensePercent,
                mana = this.mana,
                manaPercent = this.manaPercent,
                health = this.health,
                healthPercent = this.healthPercent
            };
        }

        public bool MaxAttributesOptions(FieldInfo field)
        {
            int value = (int)field.GetValue(this);

            if (field.Name.Contains("Percent"))
                return value >= m_percentages.Max();

            return value >= m_points.Max();
        }

        public virtual bool MaxAttributes(int maxAttributes = 4)
        {
                var fields = GetFields().Where(f => f.FieldType == typeof(int)).ToList();

                int activeCount = 0;
                foreach (var field in fields)
                {
                    int val = (int)field.GetValue(this);
                    if (val > 0 && !MaxAttributesOptions(field))
                        return false;

                    if (val > 0) activeCount++;
                }

                return activeCount >= maxAttributes;
        }

        public static ItemAttributes CreateFromSerializer(ItemSerializer.Attributes attributes)
        {
            return new ItemAttributes()
            {
                damage = attributes.damage,
                damagePercent = attributes.damagePercent,
                attackSpeed = attributes.attackSpeed,
                critical = attributes.critical,
                defense = attributes.defense,
                defensePercent = attributes.defensePercent,
                mana = attributes.mana,
                manaPercent = attributes.manaPercent,
                health = attributes.health,
                healthPercent = attributes.healthPercent,
            };
        }
    }
}
