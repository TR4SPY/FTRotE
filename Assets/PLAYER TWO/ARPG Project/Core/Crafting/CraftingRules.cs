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
            var weapon = items.First(i => i.IsEquippable());
            var embers = items.First(i => i.data.name == "Jewel of Embers");

            var newItem = new ItemInstance(weapon.data, false);
            newItem.attributes = weapon.attributes.Clone();
            newItem.SetItemLevel(weapon.itemLevel + 1);

            embers.stack--;

            return newItem;
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
            var eq = items.First(i => i.IsEquippable());
            var light = items.First(i => i.data.name == "Jewel of Light");

            var newItem = new ItemInstance(eq.data, false);
            newItem.attributes = eq.attributes.Clone();
            newItem.SetItemLevel(eq.itemLevel);

            // TODO: Add random skill here when implemented
            // newItem.AddRandomSkill();

            light.stack--;

            return newItem;
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
            var eq = items.First(i => i.IsEquippable());
            var clarity = items.First(i => i.data.name == "Jewel of Clarity");
            var newItem = new ItemInstance(eq.data, false);
            newItem.attributes = eq.attributes.Clone();
            newItem.SetItemLevel(eq.itemLevel);
            newItem.attributes.AddRandom();
            clarity.stack--;
            return newItem;
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
            var eq = items.First(i => i.IsEquippable());
            var ocean = items.First(i => i.data.name == "Ocean Crystal");
            var newItem = new ItemInstance(eq.data, false);
            newItem.SetItemLevel(eq.itemLevel);
            newItem.RerollAttributes();
            ocean.stack--;
            return newItem;
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
            var item = items.First(i => i.IsEquippable());
            var crystal = items.First(i => i.data.name == "Crystal of Refraction");

            int requiredCrystals = item.itemLevel - 8;
            crystal.stack -= requiredCrystals;

            var newItem = new ItemInstance(item.data, false);
            newItem.attributes = item.attributes.Clone();
            newItem.SetItemLevel(Mathf.Min(item.itemLevel + 1, 25));

            return newItem;
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
            var eq = items.First(i => i.IsEquippable());
            var dusk = items.First(i => i.data.name == "Dusk Crystal");
            var newItem = new ItemInstance(eq.data, false);
            newItem.attributes = eq.attributes.Clone();
            newItem.SetItemLevel(eq.itemLevel);

            int random = Random.Range(0, 3);
            switch (random)
            {
                case 0:
                    newItem.SetItemLevel(eq.itemLevel + 1);
                    break;
                case 1:
                    newItem.attributes.AddRandom();
                    break;
                case 2:
                    // TODO: Add random skill support
                    break;
            }

            dusk.stack--;
            return newItem;
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
            var eq = items.First(i => i.IsEquippable());
            var heart = items.First(i => i.data.name == "Heartly Crystal");

            var newItem = new ItemInstance(eq.data, false);
            newItem.attributes = eq.attributes.Clone();
            newItem.SetItemLevel(eq.itemLevel);

            // TODO: Add passive HP regeneration effect

            heart.stack--;
            return newItem;
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
            var eq = items.First(i => i.IsEquippable());
            var verdancy = items.First(i => i.data.name == "Jewel of Verdancy");

            var newItem = new ItemInstance(eq.data, false);
            newItem.attributes = eq.attributes.Clone();
            newItem.SetItemLevel(eq.itemLevel);

            // TODO: Add bonus XP passive effect

            verdancy.stack--;
            return newItem;
        }

        public override string GetPreview(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            if (eq == null) return "";
            return $"Effect: {eq.GetName()} gains bonus XP (TODO)";
        }
    }
}