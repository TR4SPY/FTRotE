using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    public class GlyphAttributes : ItemAttributes
    {
        public enum Attribute
        {
            FireResistance,
            WaterResistance,
            IceResistance,
            EarthResistance,
            AirResistance,
            LightningResistance,
            ShadowResistance,
            LightResistance,
            ArcaneResistance,
            CastingSpeed,
        }

        public int fireResistance;
        public int waterResistance;
        public int iceResistance;
        public int earthResistance;
        public int airResistance;
        public int lightningResistance;
        public int shadowResistance;
        public int lightResistance;
        public int arcaneResistance;
        public int castingSpeed;

        public GlyphAttributes(int minAttributes = 0, int maxAttributes = 0)
        {
            var attributes = GetAttributes();

            for (int i = 0; i < attributes.Count; i++)
            {
                if (!CanAddAttribute(i, attributes.Count, minAttributes, maxAttributes))
                    break;

                switch (attributes[i].type)
                {
                    case Attribute.FireResistance:
                        fireResistance = attributes[i].points;
                        break;
                    case Attribute.WaterResistance:
                        waterResistance = attributes[i].points;
                        break;
                    case Attribute.IceResistance:
                        iceResistance = attributes[i].points;
                        break;
                    case Attribute.EarthResistance:
                        earthResistance = attributes[i].points;
                        break;
                    case Attribute.AirResistance:
                        airResistance = attributes[i].points;
                        break;
                    case Attribute.LightningResistance:
                        lightningResistance = attributes[i].points;
                        break;
                    case Attribute.ShadowResistance:
                        shadowResistance = attributes[i].points;
                        break;
                    case Attribute.LightResistance:
                        lightResistance = attributes[i].points;
                        break;
                    case Attribute.ArcaneResistance:
                        arcaneResistance = attributes[i].points;
                        break;
                    case Attribute.CastingSpeed:
                        castingSpeed = attributes[i].points;
                        break;
                    default:
                        break;
                }
            }
        }

        public GlyphAttributes() { }

        protected virtual List<(Attribute type, int points)> GetAttributes()
        {
            var attributes = new List<(Attribute, int)>();
            attributes.Add((Attribute.FireResistance, GetRandomPoint()));
            attributes.Add((Attribute.WaterResistance, GetRandomPoint()));
            attributes.Add((Attribute.IceResistance, GetRandomPoint()));
            attributes.Add((Attribute.EarthResistance, GetRandomPoint()));
            attributes.Add((Attribute.AirResistance, GetRandomPoint()));
            attributes.Add((Attribute.LightningResistance, GetRandomPoint()));
            attributes.Add((Attribute.ShadowResistance, GetRandomPoint()));
            attributes.Add((Attribute.LightResistance, GetRandomPoint()));
            attributes.Add((Attribute.ArcaneResistance, GetRandomPoint()));
            attributes.Add((Attribute.CastingSpeed, GetRandomPoint()));
            return Shuffle<(Attribute, int)>(attributes);
        }
    }
}
