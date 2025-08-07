using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Stats Extended Manager")]
    public class GUIExtendedStatsManager : GUIStatsManager
    {
        [Header("Resistance Texts")]
        public Text fireResistanceText;
        public Text waterResistanceText;
        public Text iceResistanceText;
        public Text earthResistanceText;
        public Text airResistanceText;
        public Text lightningResistanceText;
        public Text shadowResistanceText;
        public Text lightResistanceText;
        public Text arcaneResistanceText;

        [Header("Immunity Texts")]
        public Text magicImmunityText;
        public Text fireImmunityText;
        public Text waterImmunityText;
        public Text iceImmunityText;
        public Text earthImmunityText;
        public Text airImmunityText;
        public Text lightningImmunityText;
        public Text shadowImmunityText;
        public Text lightImmunityText;
        public Text arcaneImmunityText;

        [Header("Regeneration Texts")]
        public Text manaRegenPerSecondText;
        public Text manaRegenPer5SecondsText;
        public Text manaRegenPer30SecondsText;
        public Text healthRegenPerSecondText;
        public Text healthRegenPer5SecondsText;
        public Text healthRegenPer30SecondsText;
        public Text experiencePerSecondPercentText;
        public Text experiencePer5SecondsPercentText;
        public Text experiencePer30SecondsPercentText;

        [Header("Block Texts")]
        public Text chanceToBlockText;
        public Text blockSpeedText;

        [Header("Combo Texts")]
        public Text maxCombosText;
        public Text timeToStopComboText;
        public Text nextComboDelayText;

        [Header("Bonus Texts")]
        public Text additionalManaText;
        public Text additionalManaPercentText;
        public Text additionalHealthText;
        public Text additionalHealthPercentText;
        public Text increaseAttackSpeedPercentText;
        public Text increaseAttackSpeedValueText;
        public Text increaseDamagePercentText;
        public Text increaseDamageValueText;
        public Text increaseMagicalDamagePercentText;
        public Text increaseMagicalDamageValueText;
        public Text additionalExperienceRewardPercentText;
        public Text additionalMoneyRewardPercentText;
        public Text additionalAmberlingsPerMinuteText;
        public Text additionalLunarisPerMinuteText;
        public Text additionalSolmiresPerMinuteText;
        public Text itemPricePercentText;

        public override void Refresh()
        {
            base.Refresh();

            if (m_entity == null || m_entity.stats == null)
                return;

            var s = m_entity.stats;

            if (fireResistanceText) fireResistanceText.text = s.fireResistance.ToString();
            if (waterResistanceText) waterResistanceText.text = s.waterResistance.ToString();
            if (iceResistanceText) iceResistanceText.text = s.iceResistance.ToString();
            if (earthResistanceText) earthResistanceText.text = s.earthResistance.ToString();
            if (airResistanceText) airResistanceText.text = s.airResistance.ToString();
            if (lightningResistanceText) lightningResistanceText.text = s.lightningResistance.ToString();
            if (shadowResistanceText) shadowResistanceText.text = s.shadowResistance.ToString();
            if (lightResistanceText) lightResistanceText.text = s.lightResistance.ToString();
            if (arcaneResistanceText) arcaneResistanceText.text = s.arcaneResistance.ToString();

            if (magicImmunityText) magicImmunityText.text = s.magicImmunity ? "Yes" : "No";
            if (fireImmunityText) fireImmunityText.text = s.fireImmunity ? "Yes" : "No";
            if (waterImmunityText) waterImmunityText.text = s.waterImmunity ? "Yes" : "No";
            if (iceImmunityText) iceImmunityText.text = s.iceImmunity ? "Yes" : "No";
            if (earthImmunityText) earthImmunityText.text = s.earthImmunity ? "Yes" : "No";
            if (airImmunityText) airImmunityText.text = s.airImmunity ? "Yes" : "No";
            if (lightningImmunityText) lightningImmunityText.text = s.lightningImmunity ? "Yes" : "No";
            if (shadowImmunityText) shadowImmunityText.text = s.shadowImmunity ? "Yes" : "No";
            if (lightImmunityText) lightImmunityText.text = s.lightImmunity ? "Yes" : "No";
            if (arcaneImmunityText) arcaneImmunityText.text = s.arcaneImmunity ? "Yes" : "No";

            if (manaRegenPerSecondText) manaRegenPerSecondText.text = s.manaRegenPerSecond.ToString();
            if (manaRegenPer5SecondsText) manaRegenPer5SecondsText.text = s.manaRegenPer5Seconds.ToString();
            if (manaRegenPer30SecondsText) manaRegenPer30SecondsText.text = s.manaRegenPer30Seconds.ToString();
            if (healthRegenPerSecondText) healthRegenPerSecondText.text = s.healthRegenPerSecond.ToString();
            if (healthRegenPer5SecondsText) healthRegenPer5SecondsText.text = s.healthRegenPer5Seconds.ToString();
            if (healthRegenPer30SecondsText) healthRegenPer30SecondsText.text = s.healthRegenPer30Seconds.ToString();
            if (experiencePerSecondPercentText) experiencePerSecondPercentText.text = s.experiencePerSecondPercent.ToString();
            if (experiencePer5SecondsPercentText) experiencePer5SecondsPercentText.text = s.experiencePer5SecondsPercent.ToString();
            if (experiencePer30SecondsPercentText) experiencePer30SecondsPercentText.text = s.experiencePer30SecondsPercent.ToString();

            if (chanceToBlockText) chanceToBlockText.text = $"{s.chanceToBlock * 100f:F2}%";
            if (blockSpeedText) blockSpeedText.text = s.blockSpeed.ToString();

            if (maxCombosText) maxCombosText.text = s.maxCombos.ToString();
            if (timeToStopComboText) timeToStopComboText.text = s.timeToStopCombo.ToString("F2");
            if (nextComboDelayText) nextComboDelayText.text = s.nextComboDelay.ToString("F2");

            if (additionalManaText) additionalManaText.text = s.additionalMana.ToString();
            if (additionalManaPercentText) additionalManaPercentText.text = s.additionalManaPercent.ToString();
            if (additionalHealthText) additionalHealthText.text = s.additionalHealth.ToString();
            if (additionalHealthPercentText) additionalHealthPercentText.text = s.additionalHealthPercent.ToString();
            if (increaseAttackSpeedPercentText) increaseAttackSpeedPercentText.text = s.increaseAttackSpeedPercent.ToString();
            if (increaseAttackSpeedValueText) increaseAttackSpeedValueText.text = s.increaseAttackSpeedValue.ToString();
            if (increaseDamagePercentText) increaseDamagePercentText.text = s.increaseDamagePercent.ToString();
            if (increaseDamageValueText) increaseDamageValueText.text = s.increaseDamageValue.ToString();
            if (increaseMagicalDamagePercentText) increaseMagicalDamagePercentText.text = s.increaseMagicalDamagePercent.ToString();
            if (increaseMagicalDamageValueText) increaseMagicalDamageValueText.text = s.increaseMagicalDamageValue.ToString();
            if (additionalExperienceRewardPercentText) additionalExperienceRewardPercentText.text = s.additionalExperienceRewardPercent.ToString();
            if (additionalMoneyRewardPercentText) additionalMoneyRewardPercentText.text = s.additionalMoneyRewardPercent.ToString();
            if (additionalAmberlingsPerMinuteText) additionalAmberlingsPerMinuteText.text = s.additionalAmberlingsPerMinute.ToString();
            if (additionalLunarisPerMinuteText) additionalLunarisPerMinuteText.text = s.additionalLunarisPerMinute.ToString();
            if (additionalSolmiresPerMinuteText) additionalSolmiresPerMinuteText.text = s.additionalSolmiresPerMinute.ToString();
            if (itemPricePercentText) itemPricePercentText.text = s.itemPricePercent.ToString();
        }
    }
}