using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [System.Serializable]
    public class ItemSerializer
    {
        [System.Serializable]
        public class Attributes
        {
            public int damage;
            public int damagePercent;
            public int attackSpeed;
            public int critical;
            public int defense;
            public int defensePercent;
            public int mana;
            public int manaPercent;
            public int health;
            public int healthPercent;
        }

        [System.Serializable]
        public class Elements
        {
            public int fireResistance;
            public int waterResistance;
            public int iceResistance;
            public int earthResistance;
            public int airResistance;
            public int lightningResistance;
            public int shadowResistance;
            public int lightResistance;
            public int arcaneResistance;
        }

        public int itemId = -1;
        public int durability;
        public int stack;
        public int itemLevel = 0;
        public int sealType;
        public float effectiveness;
        public bool skillEnabled = false;
        public Attributes attributes;
        public Elements elements;

        public ItemSerializer() { }

        public ItemSerializer(ItemInstance item)
        {
            this.itemId = GameDatabase.instance.GetElementId<Item>(item.data);
            this.durability = item.durability;

            if (item.data != null)
            {
                if (item.data.canStack)
                {
                    this.stack = Mathf.Clamp(item.stack, 1, item.data.stackCapacity);
                }
                else
                {
                    this.stack = 1;
                }
            }
            else
            {
                this.stack = 1;
            }

            this.itemLevel = item.itemLevel;
            this.skillEnabled = item.isSkillEnabled;
            this.sealType = item.sealType;
            this.effectiveness = item.effectiveness;
            this.attributes = new Attributes();
            this.elements = new Elements();

            if (item.ContainAttributes())
            {
                var a = item.attributes;

                this.attributes.damage = a.damage;
                this.attributes.damagePercent = a.damagePercent;
                this.attributes.attackSpeed = a.attackSpeed;
                this.attributes.critical = a.critical;
                this.attributes.defense = a.defense;
                this.attributes.defensePercent = a.defensePercent;
                this.attributes.mana = a.mana;
                this.attributes.manaPercent = a.manaPercent;
                this.attributes.health = a.health;
                this.attributes.healthPercent = a.healthPercent;
            }

            if (item.elements != null)
            {
                var e = item.elements;

                this.elements.fireResistance = e.fireResistance;
                this.elements.waterResistance = e.waterResistance;
                this.elements.iceResistance = e.iceResistance;
                this.elements.earthResistance = e.earthResistance;
                this.elements.airResistance = e.airResistance;
                this.elements.lightningResistance = e.lightningResistance;
                this.elements.shadowResistance = e.shadowResistance;
                this.elements.lightResistance = e.lightResistance;
                this.elements.arcaneResistance = e.arcaneResistance;
            }
        }

        public virtual string ToJson() => JsonUtility.ToJson(this);

        public static ItemSerializer FromJson(string json) =>
            JsonUtility.FromJson<ItemSerializer>(json);
    }
}
