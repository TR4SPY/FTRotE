using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Extended Stats Manager")]
    [RequireComponent(typeof(LayoutGroup))]
    public class GUIExtendedStatsManager : MonoBehaviour
    {
        protected Entity m_entity;
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
        public Text healthRegenPerSecondText;
        public Text experiencePerSecondPercentText;

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

        protected virtual void Start()
        {
            InitializeEntity();
            Refresh();
        }

        protected virtual void InitializeEntity()
        {
            if (Level.instance?.player == null)
            {
                Debug.LogError("Player instance in Level is null. Cannot initialize entity.");
                return;
            }

            m_entity = Level.instance.player;
        }

        protected void SetNumericStat(Text text, float value, string formattedValue = null)
        {
            if (!text)
                return;

            var parent = text.transform.parent.gameObject;
            bool active = Mathf.Abs(value) > Mathf.Epsilon;
            parent.SetActive(active);

            if (active)
                text.text = formattedValue ?? value.ToString();
        }

        protected void SetBoolStat(Text text, bool value)
        {
            if (!text)
                return;

            var parent = text.transform.parent.gameObject;
            parent.SetActive(value);

            if (value)
                text.text = "Yes";
        }

        public void Refresh()
        {
            if (m_entity == null || m_entity.stats == null)
                return;

            var s = m_entity.stats;

            SetNumericStat(fireResistanceText, s.fireResistance);
            SetNumericStat(waterResistanceText, s.waterResistance);
            SetNumericStat(iceResistanceText, s.iceResistance);
            SetNumericStat(earthResistanceText, s.earthResistance);
            SetNumericStat(airResistanceText, s.airResistance);
            SetNumericStat(lightningResistanceText, s.lightningResistance);
            SetNumericStat(shadowResistanceText, s.shadowResistance);
            SetNumericStat(lightResistanceText, s.lightResistance);
            SetNumericStat(arcaneResistanceText, s.arcaneResistance);

            SetBoolStat(magicImmunityText, s.magicImmunity);
            SetBoolStat(fireImmunityText, s.fireImmunity);
            SetBoolStat(waterImmunityText, s.waterImmunity);
            SetBoolStat(iceImmunityText, s.iceImmunity);
            SetBoolStat(earthImmunityText, s.earthImmunity);
            SetBoolStat(airImmunityText, s.airImmunity);
            SetBoolStat(lightningImmunityText, s.lightningImmunity);
            SetBoolStat(shadowImmunityText, s.shadowImmunity);
            SetBoolStat(lightImmunityText, s.lightImmunity);
            SetBoolStat(arcaneImmunityText, s.arcaneImmunity);

            float manaTotal = s.manaRegenPerSecond + s.manaRegenPer5Seconds / 5f + s.manaRegenPer30Seconds / 30f;
            SetNumericStat(manaRegenPerSecondText, manaTotal, manaTotal.ToString("F2"));
            
            float healthTotal = s.healthRegenPerSecond + s.healthRegenPer5Seconds / 5f + s.healthRegenPer30Seconds / 30f;
            SetNumericStat(healthRegenPerSecondText, healthTotal, healthTotal.ToString("F2"));

            float experienceTotal = s.experiencePerSecondPercent + s.experiencePer5SecondsPercent / 5f + s.experiencePer30SecondsPercent / 30f;
            SetNumericStat(experiencePerSecondPercentText, experienceTotal, experienceTotal.ToString("F2"));

            SetNumericStat(chanceToBlockText, s.chanceToBlock, $"{s.chanceToBlock * 100f:F2}%");
            SetNumericStat(blockSpeedText, s.blockSpeed);

            SetNumericStat(maxCombosText, s.maxCombos);
            SetNumericStat(timeToStopComboText, s.timeToStopCombo, s.timeToStopCombo.ToString("F2"));
            SetNumericStat(nextComboDelayText, s.nextComboDelay, s.nextComboDelay.ToString("F2"));

            SetNumericStat(additionalManaText, s.additionalMana);
            SetNumericStat(additionalManaPercentText, s.additionalManaPercent);
            SetNumericStat(additionalHealthText, s.additionalHealth);
            SetNumericStat(additionalHealthPercentText, s.additionalHealthPercent);
            SetNumericStat(increaseAttackSpeedPercentText, s.increaseAttackSpeedPercent);
            SetNumericStat(increaseAttackSpeedValueText, s.increaseAttackSpeedValue);
            SetNumericStat(increaseDamagePercentText, s.increaseDamagePercent);
            SetNumericStat(increaseDamageValueText, s.increaseDamageValue);
            SetNumericStat(increaseMagicalDamagePercentText, s.increaseMagicalDamagePercent);
            SetNumericStat(increaseMagicalDamageValueText, s.increaseMagicalDamageValue);
            SetNumericStat(additionalExperienceRewardPercentText, s.additionalExperienceRewardPercent);
            SetNumericStat(additionalMoneyRewardPercentText, s.additionalMoneyRewardPercent);
            SetNumericStat(additionalAmberlingsPerMinuteText, s.additionalAmberlingsPerMinute);
            SetNumericStat(additionalLunarisPerMinuteText, s.additionalLunarisPerMinute);
            SetNumericStat(additionalSolmiresPerMinuteText, s.additionalSolmiresPerMinute);
            SetNumericStat(itemPricePercentText, s.itemPricePercent);

            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }
    }
}