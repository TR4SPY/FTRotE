using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public abstract class CraftingRules
    {
        public abstract bool Matches(List<ItemInstance> items);
        public abstract ItemInstance Craft(List<ItemInstance> items);
        public abstract string GetPreview(List<ItemInstance> items);
        public virtual void OnFail(ItemInstance item)
        {
            // DEFAULT: Nothing will happen in default
        }
    }

    public class JewelOfEmbersRule : CraftingRules
    {
        public override bool Matches(List<ItemInstance> items)
        {
            var weapon = items.FirstOrDefault(i => i.IsEquippable());
            var embers = items.Where(i => i.data.name == "Jewel of Embers").Sum(i => i.stack);
            return weapon != null && embers >= 1 && weapon.itemLevel < 9;
        }

        public override ItemInstance Craft(List<ItemInstance> items)
        {
            var weapon = items.FirstOrDefault(i => i.IsEquippable());
            var embers = items.FirstOrDefault(i => i.data.name == "Jewel of Embers" && i.stack > 0);
            if (weapon == null || embers == null) return null;

            var newItem = new ItemInstance(weapon.data, false);
            newItem.attributes = weapon.attributes?.Clone();
            newItem.SetItemLevel(weapon.itemLevel + 1);

            return newItem;
        }

        public override void OnFail(ItemInstance item)
        {
            item.SetItemLevel(Mathf.Max(0, item.itemLevel - 1));
        }

        public override string GetPreview(List<ItemInstance> items)
        {
            var weapon = items.FirstOrDefault(i => i.IsEquippable());
            if (weapon == null) return "";

            return $"Upgrade: {weapon.GetName()} → +{weapon.itemLevel + 1}";
        }
    }

    public class JewelOfLightRule : CraftingRules
    {
        public override bool Matches(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            var light = items.FirstOrDefault(i => i.data.name == "Jewel of Light");
            return eq != null && light != null;
        }

        public override ItemInstance Craft(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            var light = items.FirstOrDefault(i => i.data.name == "Jewel of Light" && i.stack > 0);
            if (eq == null || light == null) return null;

            var newItem = new ItemInstance(eq.data, false);
            newItem.attributes = eq.attributes?.Clone();
            newItem.SetItemLevel(eq.itemLevel);

            // TODO: Add random skill implementation

            return newItem;
        }

        public override void OnFail(ItemInstance item)
        {
            // No downgrade – player just loses the jewel
        }

        public override string GetPreview(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            if (eq == null) return "";

            return $"Imbue: {eq.GetName()} with random skill";
        }
    }

    public class JewelOfClarityRule : CraftingRules
    {
        public override bool Matches(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            var clarity = items.FirstOrDefault(i => i.data.name == "Jewel of Clarity");
            return eq != null && clarity != null;
        }

        public override ItemInstance Craft(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            var clarity = items.FirstOrDefault(i => i.data.name == "Jewel of Clarity" && i.stack > 0);
            if (eq == null || clarity == null) return null;

            var newItem = new ItemInstance(eq.data, false);
            newItem.attributes = eq.attributes?.Clone();
            newItem.SetItemLevel(eq.itemLevel);
            newItem.attributes?.AddRandom();

            return newItem;
        }

        public override void OnFail(ItemInstance item)
        {
            // No downgrade – player just loses the jewel
        }

        public override string GetPreview(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            if (eq == null) return "";
            return $"Imbue: {eq.GetName()} with a random attribute";
        }
    }

    public class OceanCrystalRule : CraftingRules
    {
        public override bool Matches(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            var ocean = items.FirstOrDefault(i => i.data.name == "Ocean Crystal");
            return eq != null && ocean != null;
        }

        public override ItemInstance Craft(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            var ocean = items.FirstOrDefault(i => i.data.name == "Ocean Crystal" && i.stack > 0);
            if (eq == null || ocean == null) return null;

            var newItem = new ItemInstance(eq.data, false);
            newItem.SetItemLevel(eq.itemLevel);
            newItem.RerollAttributes();

            return newItem;
        }

        public override void OnFail(ItemInstance item)
        {
            // No downgrade – player just loses the jewel
        }

        public override string GetPreview(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            if (eq == null) return "";
            return $"Reroll: {eq.GetName()} attributes";
        }
    }

    public class CrystalOfRefractionRule : CraftingRules
    {
        public override bool Matches(List<ItemInstance> items)
        {
            var item = items.FirstOrDefault(i => i.IsEquippable());
            var crystals = items.FirstOrDefault(i => i.data.name == "Crystal of Refraction");

            if (item == null || crystals == null) return false;
            if (item.itemLevel < 9 || item.itemLevel >= 25) return false;

            int requiredCrystals = item.itemLevel - 8;
            return crystals.stack >= requiredCrystals;
        }

        public override ItemInstance Craft(List<ItemInstance> items)
        {
            var item = items.FirstOrDefault(i => i.IsEquippable());
            var crystals = items.FirstOrDefault(i => i.data.name == "Crystal of Refraction");

            if (item == null || crystals == null)
                return null;

            int required = item.itemLevel - 8;
            float baseSuccessRate = (float)crystals.stack / required * 0.5f;
            baseSuccessRate = Mathf.Clamp01(baseSuccessRate);

            if (Random.value > baseSuccessRate)
            {
                return null;
            }

            var newItem = new ItemInstance(item.data, false);
            newItem.SetItemLevel(item.itemLevel + 1);
            newItem.attributes = item.attributes;
            crystals.stack--;

            return newItem;
        }

        public override void OnFail(ItemInstance item)
        {
            item.SetItemLevel(0);
        }

        public override string GetPreview(List<ItemInstance> items)
        {
            var item = items.FirstOrDefault(i => i.IsEquippable());
            if (item == null) return "";

            int nextLevel = Mathf.Min(item.itemLevel + 1, 25);
            int requiredCrystals = item.itemLevel - 8;
            return $"Enhance: {item.GetName()} → +{nextLevel} (Needs {requiredCrystals} Crystal(s))";
        }
    }

    public class DuskCrystalRule : CraftingRules
    {
        public override bool Matches(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            var dusk = items.FirstOrDefault(i => i.data.name == "Dusk Crystal");
            return eq != null && dusk != null;
        }

        public override ItemInstance Craft(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            var dusk = items.FirstOrDefault(i => i.data.name == "Dusk Crystal" && i.stack > 0);
            if (eq == null || dusk == null) return null;

            var newItem = new ItemInstance(eq.data, false);
            newItem.attributes = eq.attributes?.Clone();
            newItem.SetItemLevel(eq.itemLevel);

            int random = Random.Range(0, 3);
            switch (random)
            {
                case 0:
                    newItem.SetItemLevel(eq.itemLevel + 1);
                    break;
                case 1:
                    newItem.attributes?.AddRandom();
                    break;
                case 2:
                    // TODO: Add random skill support
                    break;
            }

            return newItem;
        }

        public override void OnFail(ItemInstance item)
        {
            // No downgrade – player just loses the jewel
        }

        public override string GetPreview(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            if (eq == null) return "";
            return $"Dusk Effect: Random upgrade, attribute or skill for {eq.GetName()}";
        }
    }

    public class HeartlyCrystalRule : CraftingRules
    {
        public override bool Matches(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            var heart = items.FirstOrDefault(i => i.data.name == "Heartly Crystal");
            return eq != null && heart != null;
        }

        public override ItemInstance Craft(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            var heart = items.FirstOrDefault(i => i.data.name == "Heartly Crystal" && i.stack > 0);
            if (eq == null || heart == null) return null;

            var newItem = new ItemInstance(eq.data, false);
            newItem.attributes = eq.attributes?.Clone();
            newItem.SetItemLevel(eq.itemLevel);

            // TODO: Add passive HP regeneration effect

            return newItem;
        }

        public override void OnFail(ItemInstance item)
        {
            // No downgrade – player just loses the jewel
        }

        public override string GetPreview(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            if (eq == null) return "";
            return $"Effect: {eq.GetName()} gains health regen (TODO)";
        }
    }

    public class JewelOfVerdancyRule : CraftingRules
    {
        public override bool Matches(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            var verdancy = items.FirstOrDefault(i => i.data.name == "Jewel of Verdancy");
            return eq != null && verdancy != null;
        }

        public override ItemInstance Craft(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            var verdancy = items.FirstOrDefault(i => i.data.name == "Jewel of Verdancy" && i.stack > 0);
            if (eq == null || verdancy == null) return null;

            var newItem = new ItemInstance(eq.data, false);
            newItem.attributes = eq.attributes?.Clone();
            newItem.SetItemLevel(eq.itemLevel);

            // TODO: Add bonus XP passive effect

            return newItem;
        }

        public override void OnFail(ItemInstance item)
        {
            // No downgrade – player just loses the jewel
        }

        public override string GetPreview(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            if (eq == null) return "";
            return $"Effect: {eq.GetName()} gains bonus XP (TODO)";
        }
    }
}