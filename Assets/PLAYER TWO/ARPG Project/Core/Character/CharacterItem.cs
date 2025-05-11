using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [System.Serializable]
    public class CharacterItem
    {
        public Item data;
        public int stack;
        public CharacterItemAttributes attributes;

        [HideInInspector]
        public int durability;
        [HideInInspector]
        public int itemLevel = 0;
        public bool skillEnabled;


        public CharacterItem(Item data,
        CharacterItemAttributes attributes,
        int durability, int stack)
        {
            this.data = data;
            this.attributes = attributes;
            this.durability = durability;

            if (data != null && !data.canStack && stack < 1)
            {
                this.stack = 1;
            }
            else if (data != null && data.canStack)
            {
                this.stack = Mathf.Clamp(stack, 1, data.stackCapacity);
            }
            else
            {
                this.stack = 1;
            }
        }

        public ItemInstance ToItemInstance(bool withDefaultDurability = false)
        {
            var a = new ItemAttributes()
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
                healthPercent = attributes.healthPercent
            };

            if (withDefaultDurability)
            {
                var durability = data is ItemEquippable ? (data as ItemEquippable).maxDurability : 0;
                return new ItemInstance(data, a, durability, stack);
            }

            int finalStack = stack;
            if (data.canStack)
                finalStack = Mathf.Clamp(finalStack, 1, data.stackCapacity);
            else
                finalStack = 1;

            var instance = new ItemInstance(data, a, durability, finalStack);
            instance.SetItemLevel(itemLevel);
            instance.isSkillEnabled = this.skillEnabled;

            return instance;
        }

        public static CharacterItem CreateFromSerializer(ItemSerializer serializer)
        {
            var data = GameDatabase.instance.FindElementById<Item>(serializer.itemId);
            var attributes = CharacterItemAttributes.CreateFromSerializer(serializer.attributes);
            var characterItem = new CharacterItem(data, attributes, serializer.durability, serializer.stack);
            characterItem.itemLevel = serializer.itemLevel;
            characterItem.skillEnabled = serializer.skillEnabled;

            return characterItem;
        }
    }
}
