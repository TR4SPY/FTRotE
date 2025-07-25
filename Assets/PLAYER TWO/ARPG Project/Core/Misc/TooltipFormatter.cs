using UnityEngine;
using PLAYERTWO.ARPGProject;

namespace PLAYERTWO.ARPGProject
{
    public static class TooltipFormatter
    {
        public static string FormatRewardText(string label, int baseValue, int finalValue, float multiplier)
        {
            if (baseValue == 0 && finalValue == 0)
                return "";

            Color bonusColor = GameColors.MultiplierColor(multiplier);

            string changeText = multiplier != 1f
                ? $"{StringUtils.StringWithColor(FormatMultiplierText(multiplier), bonusColor)}"
                : "";

            return $"{label}: {baseValue} ({changeText})";
        }

        public static string FormatObjectiveTooltip(string objectiveText, int currentProgress, int targetProgress, int baseTargetProgress)
        {
            if (targetProgress == 0)
            {
                return $"{objectiveText}: ";
            }

            string adjustmentText = "";
            if (targetProgress != baseTargetProgress)
            {
                int difference = targetProgress - baseTargetProgress;
                float percentage = ((float)difference / baseTargetProgress) * 100f;

                Color adjustmentColor = GameColors.LightBlue; // 🔵 Domyślnie niebieski (bez zmian)
                if (percentage > 0) adjustmentColor = GameColors.LightRed; // 🔴 Trudniejsze zadanie
                else if (percentage < 0) adjustmentColor = GameColors.Green; // 🟢 Łatwiejsze zadanie

                string percentageText = percentage != 0 ? $"{(percentage > 0 ? "+" : "")}{percentage:F0}%" : "-/-";
                adjustmentText = $" ({baseTargetProgress} {StringUtils.StringWithColor(percentageText, adjustmentColor)})";
            }

            string tooltipText = $"{objectiveText}\n{currentProgress} / {targetProgress}{adjustmentText}";

            if (targetProgress != baseTargetProgress)
            {
                tooltipText += $"\n\n{GetObjectiveMessage()}";
            }

            return tooltipText;
        }

        public static string GetRewardMessage()
        {
            return $"Rewards may be adjusted\nbased on player type, buffs or debuffs";
        }

        public static string GetObjectiveMessage()
        {
            return $"Objectives may be adjusted\nbased on player type, curses or blessings";
        }

        public static string FormatExperienceTooltip(int currentExp, int nextLevelExp, bool isMaxLevel)
        {
            if (isMaxLevel)
            {
                return $"Current Experience: {currentExp}\nMax Level Reached";
            }

            return $"Current Experience: {currentExp}\nExperience Needed: {nextLevelExp}";
        }

        public static string FormatMultiplierText(float multiplier)
        {
            if (multiplier > 1f)
                return $"+{Mathf.RoundToInt((multiplier - 1) * 100)}%";
            if (multiplier < 1f)
                return $"-{Mathf.RoundToInt((1 - multiplier) * 100)}%";
            return "0%";
        }

        public static string FormatColoredMultiplier(float multiplier)
        {
            Color bonusColor = GameColors.MultiplierColor(multiplier);
            return StringUtils.StringWithColor(FormatMultiplierText(multiplier), bonusColor);
        }

        public static string FormatCurrencyBreakdown(int totalAmberlings)
        {
            if (totalAmberlings <= 0)
                return "";

            var c = new Currency();
            c.SetFromTotalAmberlings(totalAmberlings);

            string result = "";
            if (c.solmire > 0) result += $"{c.solmire} Solmire ";
            if (c.lunaris > 0) result += $"{c.lunaris} Lunaris ";
            if (c.amberlings > 0) result += $"{c.amberlings} Amberlings";

            return result.Trim();
        }

        public static string GenerateItemRewardsTooltip(QuestItemReward[] items)
        {
            if (items == null || items.Length == 0)
                return "";

            string result = "\nItem Rewards:\n";
            foreach (var item in items)
            {
                if (item.data)
                {
                    Color rarityColor = GameColors.RarityColor(item.data.rarity);
                    string itemName = StringUtils.StringWithColor($"[{item.data.name}]", rarityColor);
                    string attributes = item.attributes > 0 ? $" (+{item.attributes} Attributes)" : "";

                    result += $"   🔹 {itemName}{attributes}\n";
                }
            }
            return result;
        }
    }
}