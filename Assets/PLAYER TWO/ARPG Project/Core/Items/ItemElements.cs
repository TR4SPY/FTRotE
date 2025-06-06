using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;

namespace PLAYERTWO.ARPGProject
{
    [System.Serializable]
    public class ItemElements
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

        protected static FieldInfo[] m_fields;
        protected int[] m_points = new int[] { 2, 4, 6, 8, 10, 12 };

        public ItemElements(int minElements = 0, int maxElements = 0)
        {
            var fields = GetElementalFields();
            fields = Shuffle(fields);

            for (int i = 0; i < fields.Length; i++)
            {
                if (!CanAddAttribute(i, fields.Length, minElements, maxElements))
                    break;

                int value = m_points[UnityEngine.Random.Range(0, m_points.Length)];
                fields[i].SetValue(this, value);
            }
        }

        public ItemElements() { }

        protected FieldInfo[] GetElementalFields()
        {
            if (m_fields == null)
            {
                m_fields = typeof(ItemElements).GetFields(BindingFlags.Public | BindingFlags.Instance);
            }

            return Array.FindAll(m_fields, field =>
                field.Name.EndsWith("Resistance", StringComparison.OrdinalIgnoreCase));
        }

        protected FieldInfo[] Shuffle(FieldInfo[] input)
        {
            var list = new List<FieldInfo>(input);
            for (int i = 0; i < list.Count; i++)
            {
                int j = UnityEngine.Random.Range(i, list.Count);
                (list[i], list[j]) = (list[j], list[i]);
            }
            return list.ToArray();
        }

        protected bool CanAddAttribute(int index, int total, int min, int max)
        {
            if (max > 0 && index >= max) return false;
            if (index < min) return true;
            return UnityEngine.Random.value > 0.5f;
        }

        public virtual string Inspect()
        {
            var list = new List<string>();

            foreach (MagicElement element in Enum.GetValues(typeof(MagicElement)))
            {
                if (element == MagicElement.None)
                    continue;

                int value = GetResistance(element);
                if (value != 0)
                {
                    string label = $"{element} Resistance: {value}";
                    string icon = $"<sprite name=\"{StringUtils.GetTMPElementSpriteName(element)}\" tint>";
                    list.Add($"{label} {icon}");
                }
            }

            return string.Join("\n", list);
        }

        public int GetResistance(MagicElement element)
        {
            return element switch
            {
                MagicElement.Fire => fireResistance,
                MagicElement.Water => waterResistance,
                MagicElement.Ice => iceResistance,
                MagicElement.Earth => earthResistance,
                MagicElement.Air => airResistance,
                MagicElement.Lightning => lightningResistance,
                MagicElement.Shadow => shadowResistance,
                MagicElement.Light => lightResistance,
                MagicElement.Arcane => arcaneResistance,
                _ => 0
            };
        }

        public static ItemElements CreateFromSerializer(ItemSerializer.Elements elements)
        {
            if (elements == null) return new ItemElements();

            return new ItemElements()
            {
                fireResistance = elements.fireResistance,
                waterResistance = elements.waterResistance,
                iceResistance = elements.iceResistance,
                earthResistance = elements.earthResistance,
                airResistance = elements.airResistance,
                lightningResistance = elements.lightningResistance,
                shadowResistance = elements.shadowResistance,
                lightResistance = elements.lightResistance,
                arcaneResistance = elements.arcaneResistance
            };
        }
    }
}
