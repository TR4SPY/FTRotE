using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    public class JewelryAttributes : ItemAttributes
    {
        public enum Attribute
        {
            Defense,
            Mana,
            ManaPercent,
            Health,
            HealthPercent,
            MagicResistance
        }

        public JewelryAttributes(int minAttributes = 0, int maxAttributes = 0)
        {
            var attributes = GetAttributes();

            for (int i = 0; i < attributes.Count; i++)
            {
                if (!CanAddAttribute(i, attributes.Count, minAttributes, maxAttributes))
                    break;

                switch (attributes[i].type)
                {
                    default:
                    case Attribute.Defense:
                        defense = attributes[i].points;
                        break;
                    case Attribute.Mana:
                        mana = attributes[i].points;
                        break;
                    case Attribute.ManaPercent:
                        manaPercent = attributes[i].points;
                        break;
                    case Attribute.Health:
                        health = attributes[i].points;
                        break;
                    case Attribute.HealthPercent:
                        healthPercent = attributes[i].points;
                        break;
                    case Attribute.MagicResistance:
                        magicResistance = attributes[i].points;
                        break;
                }
            }
        }

        public JewelryAttributes() { }

        protected virtual List<(Attribute type, int points)> GetAttributes()
        {
            var attributes = new List<(Attribute, int)>();
            attributes.Add((Attribute.Defense, GetRandomPoint()));
            attributes.Add((Attribute.Mana, GetRandomPoint()));
            attributes.Add((Attribute.ManaPercent, GetRandomPercentage()));
            attributes.Add((Attribute.Health, GetRandomPoint()));
            attributes.Add((Attribute.HealthPercent, GetRandomPercentage()));
            attributes.Add((Attribute.MagicResistance, GetRandomPoint()));
            return Shuffle<(Attribute, int)>(attributes);
        }
    }
}
