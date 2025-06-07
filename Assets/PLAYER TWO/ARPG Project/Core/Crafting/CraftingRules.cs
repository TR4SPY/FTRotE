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

        public virtual void ConsumeUsedItems(List<ItemInstance> originalItems, List<ItemInstance> usedItems)
        {
            foreach (var used in usedItems)
            {
                var original = originalItems.FirstOrDefault(i => i.data == used.data);
                if (original != null)
                {
                    int consumed = Mathf.Min(original.stack, used.stack);
                    original.stack -= consumed;
                    if (original.stack <= 0)
                        originalItems.Remove(original);
                }
            }
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
            var original = items.FirstOrDefault(i => i.IsEquippable());
            var embers = items.FirstOrDefault(i => i.data.name == "Jewel of Embers" && i.stack > 0);
            if (original == null || embers == null)
                return null;

            var newItem = new ItemInstance(original.data, original.attributes?.Clone());
            newItem.SetItemLevel(original.itemLevel + 1);
            newItem.isSkillEnabled = original.isSkillEnabled;

            return newItem;
        }

        public override void OnFail(ItemInstance item)
        {
            item.SetItemLevel(Mathf.Max(0, item.itemLevel - 1));
        }

        public override void ConsumeUsedItems(List<ItemInstance> originalItems, List<ItemInstance> usedItems)
        {
            var item = originalItems.FirstOrDefault(i => i.IsEquippable());
            if (item != null)
                originalItems.Remove(item);

            var jewel = originalItems.FirstOrDefault(i => i.data.name == "Jewel of Embers");
            if (jewel != null)
            {
                if (jewel.stack > 1)
                    jewel.stack--;
                else
                    originalItems.Remove(jewel);
            }
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
            var item = items.FirstOrDefault(i => i.IsEquippable());
            var jewel = items.FirstOrDefault(i => i.data.name == "Jewel of Light");

            return item is { data: ItemWeapon weapon } &&
                weapon.skill != null &&
                !item.isSkillEnabled &&
                jewel != null;
        }

        public override ItemInstance Craft(List<ItemInstance> items)
        {
            var item = items.FirstOrDefault(i => i.IsEquippable());
            if (item == null || item.data is not ItemWeapon weapon || weapon.skill == null || item.isSkillEnabled)
                return null;

            var newItem = new ItemInstance(item.data, item.attributes?.Clone());
            newItem.SetItemLevel(item.itemLevel);
            newItem.isSkillEnabled = true;

            return newItem;
        }

        public override void OnFail(ItemInstance item)
        {
            // Nie modyfikujemy itemu – po porażce zostaje w oryginalnym stanie.
        }
        public override void ConsumeUsedItems(List<ItemInstance> originalItems, List<ItemInstance> usedItems)
        {
            var item = originalItems.FirstOrDefault(i => i.IsEquippable());
            if (item != null)
                originalItems.Remove(item);

            var jewel = originalItems.FirstOrDefault(i => i.data.name == "Jewel of Light");
            if (jewel != null)
            {
                if (jewel.stack > 1)
                    jewel.stack--;
                else
                    originalItems.Remove(jewel);
            }

            int barsUsed = Mathf.Clamp(originalItems.Count(i => i.data.name == "Bar of Solmire"), 0, 10);
            for (int i = 0; i < barsUsed; i++)
            {
                var bar = originalItems.FirstOrDefault(i => i.data.name == "Bar of Solmire");
                if (bar != null)
                {
                    if (bar.stack > 1)
                        bar.stack--;
                    else
                        originalItems.Remove(bar);
                }
            }
        }
        public override string GetPreview(List<ItemInstance> items)
        {
            var item = items.FirstOrDefault(i => i.IsEquippable());
            if (item == null || item.data is not ItemWeapon weapon)
                return "";

            if (weapon.skill != null && !item.isSkillEnabled)
                return $"Imbue: {item.GetName()} with skill: {weapon.skill.name}";

            return "No skills available, try different combination.";
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
            if (eq == null || clarity == null)
                return null;

            var newItem = new ItemInstance(eq.data, false);
            newItem.SetItemLevel(eq.itemLevel);
            newItem.isSkillEnabled = eq.isSkillEnabled;

            newItem.attributes = eq.attributes?.Clone();
            if (newItem.attributes == null && eq.data is ItemEquippable equip)
                newItem.attributes = equip.CreateDefaultAttributes();

            if (!newItem.attributes.MaxAttributes())
                newItem.attributes.AddRandom();

            return newItem;
        }

        public override void OnFail(ItemInstance item)
        {
            // No downgrade – player just loses the jewel
        }

        public override void ConsumeUsedItems(List<ItemInstance> originalItems, List<ItemInstance> usedItems)
        {
            var item = originalItems.FirstOrDefault(i => i.IsEquippable());
            if (item != null)
                originalItems.Remove(item);

            var jewel = originalItems.FirstOrDefault(i => i.data.name == "Jewel of Clarity");
            if (jewel != null)
            {
                if (jewel.stack > 1)
                    jewel.stack--;
                else
                    originalItems.Remove(jewel);
            }
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
            if (eq == null || ocean == null)
                return null;

            var newItem = new ItemInstance(eq.data, false);
            newItem.SetItemLevel(eq.itemLevel);
            newItem.isSkillEnabled = eq.isSkillEnabled;

            newItem.attributes = eq.attributes?.Clone();
            if (newItem.attributes == null && eq.data is ItemEquippable equip)
                newItem.attributes = equip.CreateDefaultAttributes();

            newItem.RerollAttributes();

            return newItem;
        }

        public override void OnFail(ItemInstance item)
        {
            // No downgrade – player just loses the jewel
        }
        public override void ConsumeUsedItems(List<ItemInstance> originalItems, List<ItemInstance> usedItems)
        {
            var item = originalItems.FirstOrDefault(i => i.IsEquippable());
            if (item != null)
                originalItems.Remove(item);

            var crystal = originalItems.FirstOrDefault(i => i.data.name == "Ocean Crystal");
            if (crystal != null)
            {
                if (crystal.stack > 1)
                    crystal.stack--;
                else
                    originalItems.Remove(crystal);
            }
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
        private int lastItemLevel = 0; 
        private bool lastCraftSuccess = false;
        public override bool Matches(List<ItemInstance> items)
        {
            var item = items.FirstOrDefault(i => i.IsEquippable());
            var crystalsList = items.Where(i => i.data.name == "Crystal of Refraction").ToList();
            int totalCrystals = crystalsList.Sum(i => i.stack);

            if (item == null || totalCrystals <= 0)
                return false;

            if (item.itemLevel < 9 || item.itemLevel >= 25)
                return false;

            int requiredCrystals = item.itemLevel - 8;
            if (totalCrystals < requiredCrystals)
                return false;

            return true;
        }

        public override ItemInstance Craft(List<ItemInstance> items)
        {
            var item = items.FirstOrDefault(i => i.IsEquippable());
            var crystals = items.Where(i => i.data.name == "Crystal of Refraction").Sum(i => i.stack);

            if (item == null || crystals <= 0)
            {
                lastCraftSuccess = false;
                return null;
            }

            lastItemLevel = item.itemLevel;

            int required = lastItemLevel - 8;
            float baseSuccessRate = (float)crystals / required * 0.5f;
            baseSuccessRate = Mathf.Clamp01(baseSuccessRate);

            if (Random.value > baseSuccessRate)
            {
                lastCraftSuccess = false;
                return null;
            }

            lastCraftSuccess = true;

            var newItem = new ItemInstance(item.data, item.attributes?.Clone());
            newItem.SetItemLevel(item.itemLevel + 1);
            newItem.isSkillEnabled = item.isSkillEnabled;
            newItem.ForceStack(1);

            return newItem;
        }

        public override void OnFail(ItemInstance item)
        {
            lastItemLevel = item.itemLevel;
            item.SetItemLevel(0);
            item.ForceStack(1);
        }
        public override void ConsumeUsedItems(List<ItemInstance> originalItems, List<ItemInstance> usedItems)
        {
            var eqItem = originalItems.FirstOrDefault(i => i.IsEquippable());
            if (eqItem != null)
                originalItems.Remove(eqItem);

            int remaining = lastItemLevel - 8;
            if (remaining <= 0) return;

            for (int i = originalItems.Count - 1; i >= 0 && remaining > 0; i--)
            {
                var inst = originalItems[i];
                if (inst.data.name != "Crystal of Refraction")
                    continue;

                if (inst.stack <= remaining)
                {
                    remaining -= inst.stack;
                    originalItems.RemoveAt(i);
                }
                else
                {
                    inst.ForceStack(inst.stack - remaining);
                    remaining = 0;
                    originalItems[i] = inst;
                }
            }
        }
        public override string GetPreview(List<ItemInstance> items)
        {
            var item = items.FirstOrDefault(i => i.IsEquippable());
            if (item == null)
                return "";

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
            if (eq == null || dusk == null)
                return null;

            var newItem = new ItemInstance(eq.data, false);
            newItem.SetItemLevel(eq.itemLevel);
            newItem.isSkillEnabled = eq.isSkillEnabled;

            newItem.attributes = eq.attributes?.Clone();
            if (newItem.attributes == null && eq.data is ItemEquippable equip)
                newItem.attributes = equip.CreateDefaultAttributes();

            var possibleEffects = new List<int>();

            if (eq.itemLevel < 9) possibleEffects.Add(0);
            if (newItem.attributes != null && !newItem.attributes.MaxAttributes()) possibleEffects.Add(1);
            if (eq.data is ItemWeapon weapon && weapon.skill != null && !eq.isSkillEnabled) possibleEffects.Add(2);

            if (possibleEffects.Count == 0)
                return newItem;

            int effect = Random.Range(0, possibleEffects.Count);

            switch (possibleEffects[effect])
            {
                case 0:
                    newItem.SetItemLevel(eq.itemLevel + 1);
                    break;
                case 1:
                    newItem.attributes.AddRandom();
                    break;
                case 2:
                    newItem.isSkillEnabled = true;
                    break;
            }

            return newItem;
        }

        public override void OnFail(ItemInstance item)
        {
            // No downgrade – player just loses the crystal
        }

        public override void ConsumeUsedItems(List<ItemInstance> originalItems, List<ItemInstance> usedItems)
        {
            var item = originalItems.FirstOrDefault(i => i.IsEquippable());
            if (item != null)
                originalItems.Remove(item);

            var crystal = originalItems.FirstOrDefault(i => i.data.name == "Dusk Crystal");
            if (crystal != null)
            {
                if (crystal.stack > 1)
                    crystal.stack--;
                else
                    originalItems.Remove(crystal);
            }
        }

        public override string GetPreview(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            if (eq == null) return "";
            return $"Dusk Effect: +1 level, +1 attribute or skill activation for {eq.GetName()}";
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

            var newItem = new ItemInstance(eq.data, eq.attributes?.Clone());
            newItem.SetItemLevel(eq.itemLevel);

            // TODO: Add passive HP regeneration effect

            return newItem;
        }

        public override void OnFail(ItemInstance item)
        {
            // No downgrade – player just loses the jewel
        }

        public override void ConsumeUsedItems(List<ItemInstance> originalItems, List<ItemInstance> usedItems)
        {
            var item = originalItems.FirstOrDefault(i => i.IsEquippable());
            if (item != null)
                originalItems.Remove(item);

            var crystal = originalItems.FirstOrDefault(i => i.data.name == "Heartly Crystal");
            if (crystal != null)
            {
                if (crystal.stack > 1)
                    crystal.stack--;
                else
                    originalItems.Remove(crystal);
            }
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

            var newItem = new ItemInstance(eq.data, eq.attributes?.Clone());
            newItem.SetItemLevel(eq.itemLevel);

            // TODO: Add bonus XP passive effect

            return newItem;
        }

        public override void OnFail(ItemInstance item)
        {
            // No downgrade – player just loses the jewel
        }

        public override void ConsumeUsedItems(List<ItemInstance> originalItems, List<ItemInstance> usedItems)
        {
            var item = originalItems.FirstOrDefault(i => i.IsEquippable());
            if (item != null)
                originalItems.Remove(item);

            var jewel = originalItems.FirstOrDefault(i => i.data.name == "Jewel of Verdancy");
            if (jewel != null)
            {
                if (jewel.stack > 1)
                    jewel.stack--;
                else
                    originalItems.Remove(jewel);
            }
        }

        public override string GetPreview(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            if (eq == null) return "";
            return $"Effect: {eq.GetName()} gains bonus XP (TODO)";
        }
    }
    public class ElementStoneRule : CraftingRules
    {
        protected readonly MagicElement element;
        protected readonly string stoneName;

        public ElementStoneRule(MagicElement element, string stoneName)
        {
            this.element = element;
            this.stoneName = stoneName;
        }

        public override bool Matches(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            var stone = items.FirstOrDefault(i => i.data.name == stoneName);
            return eq != null && stone != null;
        }

        public override ItemInstance Craft(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            var stone = items.FirstOrDefault(i => i.data.name == stoneName && i.stack > 0);
            if (eq == null || stone == null) return null;

            var newItem = new ItemInstance(eq.data, eq.attributes?.Clone(), eq.elements?.Clone());
            newItem.SetItemLevel(eq.itemLevel);
            newItem.isSkillEnabled = eq.isSkillEnabled;
            if (newItem.elements == null)
                newItem.elements = new ItemElements();

            newItem.elements.ModifyResistance(element, 2);

            return newItem;
        }

        public override void OnFail(ItemInstance item)
        {
            item?.elements?.ModifyResistance(element, -2);
        }

        public override void ConsumeUsedItems(List<ItemInstance> originalItems, List<ItemInstance> usedItems)
        {
            var eq = originalItems.FirstOrDefault(i => i.IsEquippable());
            if (eq != null)
                originalItems.Remove(eq);

            var stone = originalItems.FirstOrDefault(i => i.data.name == stoneName);
            if (stone != null)
            {
                if (stone.stack > 1)
                    stone.stack--;
                else
                    originalItems.Remove(stone);
            }
        }

        public override string GetPreview(List<ItemInstance> items)
        {
            var eq = items.FirstOrDefault(i => i.IsEquippable());
            if (eq == null) return "";
            int current = eq.elements?.GetResistance(element) ?? 0;
            int next = Mathf.Clamp(current + 2, 0, 12);
            return $"Imbue: {eq.GetName()} {element} Res {current} → {next}";
        }
    }

    public class FireStoneRule : ElementStoneRule
    {
        public FireStoneRule() : base(MagicElement.Fire, "Fire Stone") { }
    }

    public class WaterStoneRule : ElementStoneRule
    {
        public WaterStoneRule() : base(MagicElement.Water, "Water Stone") { }
    }

    public class IceStoneRule : ElementStoneRule
    {
        public IceStoneRule() : base(MagicElement.Ice, "Ice Stone") { }
    }

    public class EarthStoneRule : ElementStoneRule
    {
        public EarthStoneRule() : base(MagicElement.Earth, "Earth Stone") { }
    }

    public class AirStoneRule : ElementStoneRule
    {
        public AirStoneRule() : base(MagicElement.Air, "Air Stone") { }
    }

    public class LightningStoneRule : ElementStoneRule
    {
        public LightningStoneRule() : base(MagicElement.Lightning, "Lightning Stone") { }
    }

    public class ShadowStoneRule : ElementStoneRule
    {
        public ShadowStoneRule() : base(MagicElement.Shadow, "Shadow Stone") { }
    }

    public class StoneOfLightRule : ElementStoneRule
    {
        public StoneOfLightRule() : base(MagicElement.Light, "Stone of Light") { }
    }

    public class ArcaneStoneRule : ElementStoneRule
    {
        public ArcaneStoneRule() : base(MagicElement.Arcane, "Arcane Stone") { }
    }

    public class StoneOfElementsRule : CraftingRules
    {
        private readonly string[] requiredStones = new[]
        {
            "Fire Stone",
            "Water Stone",
            "Ice Stone",
            "Earth Stone",
            "Air Stone",
            "Lightning Stone",
            "Shadow Stone",
            "Stone of Light",
            "Arcane Stone"
        };

        public override bool Matches(List<ItemInstance> items)
        {
            foreach (var s in requiredStones)
            {
                if (items.Where(i => i.data.name == s).Sum(i => i.stack) < 1)
                    return false;
            }
            return true;
        }

        public override ItemInstance Craft(List<ItemInstance> items)
        {
            var stoneData = GameDatabase.instance.items.FirstOrDefault(i => i.name == "Stone of Elements");
            if (stoneData == null) return null;
            return new ItemInstance(stoneData, false);
        }

        public override void OnFail(ItemInstance item)
        {
            // no item to modify
        }

        public override void ConsumeUsedItems(List<ItemInstance> originalItems, List<ItemInstance> usedItems)
        {
            foreach (var s in requiredStones)
            {
                var stone = originalItems.FirstOrDefault(i => i.data.name == s);
                if (stone != null)
                {
                    if (stone.stack > 1)
                        stone.stack--;
                    else
                        originalItems.Remove(stone);
                }
            }
        }

        public override string GetPreview(List<ItemInstance> items)
        {
            return "Combine stones into Stone of Elements";
        }
    }
}