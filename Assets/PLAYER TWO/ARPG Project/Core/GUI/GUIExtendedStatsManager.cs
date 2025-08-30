using System;
using UnityEngine;
using UnityEngine.UI;
using AI_DDA.Assets.Scripts;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Extended Stats Manager")]
    [RequireComponent(typeof(LayoutGroup))]
    public class GUIExtendedStatsManager : MonoBehaviour
    {
        protected Entity m_entity;
        protected EntityBuffManager m_buffManager;
        protected EntityItemManager m_itemManager;

        [Header("Player Type")]
        public Text playerTypeDescriptionText;

        private static readonly Dictionary<string, string> s_playerTypeDescriptions = new()
        {
            {"Achiever", "You are an Achiever. Focusing on goals, rewards, and completion."},
            {"Explorer", "You are an Explorer. Loving discovering new areas and secrets."},
            {"Socializer", "You are a Socializer. Enjoying interacting and forming connections."},
            {"Killer", "You are a Killer! Seeking competition and dominating opponents."}
        };

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
        public Text additionalHealthText;
        public Text increaseAttackSpeedPercentText;
        public Text increaseDamageValueText;
        public Text increaseMagicalDamageValueText;
        public Text additionalExperienceRewardPercentText;
        public Text additionalMoneyRewardPercentText;
        public Text additionalAmberlingsPerMinuteText;
        public Text additionalLunarisPerMinuteText;
        public Text additionalSolmiresPerMinuteText;
        public Text itemPricePercentText;

        [Header("Progress Texts")]
        public Text zonesDiscoveredText;
        public Text achievementsUnlockedText;
        public Text questsCompletedText;
        public Text storylineCompletedText;

        [Header("Difficulty Texts")]
        public Text currentDifficultyText;
        public Text difficultyMultiplierText;


        GameObject m_resistancesHeader;
        GameObject m_immunitiesHeader;
        GameObject m_regenerationsHeader;
        GameObject m_blocksHeader;
        GameObject m_combosHeader;
        GameObject m_bonusesHeader;
        GameObject m_progressHeader;
        GameObject m_difficultyHeader;

        protected virtual void Start()
        {
            InitializeEntity();
            CacheHeaders();
            
            if (m_entity != null)
            {
                if (m_entity.stats != null)
                {
                    m_entity.stats.onLevelUp.AddListener(Refresh);
                    m_entity.stats.onRecalculate.AddListener(Refresh);
                }

                m_buffManager = m_entity.GetComponent<EntityBuffManager>();
                if (m_buffManager != null)
                {
                    m_buffManager.onBuffAdded.AddListener(HandleBuffChanged);
                    m_buffManager.onBuffRemoved.AddListener(HandleBuffChanged);
                }

                m_itemManager = m_entity.items;
                if (m_itemManager != null)
                {
                    m_itemManager.onChanged.AddListener(Refresh);
                }
            }

            Refresh();
        }

        protected void HandleBuffChanged(BuffInstance _)
        {
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

        void CacheHeaders()
        {
            Transform[] all = GetComponentsInChildren<Transform>(true);

            GameObject Find(string n)
            {
                var t = Array.Find(all, tr => tr.name == n);
                return t ? t.gameObject : null;
            }

            m_resistancesHeader = Find("Header Resistances");
            m_immunitiesHeader = Find("Header Immunities");
            m_regenerationsHeader = Find("Header Regenerations");
            m_blocksHeader = Find("Header Blocks");
            m_combosHeader = Find("Header Combos");
            m_bonusesHeader = Find("Header Bonuses");
            m_progressHeader = Find("Header Progress");
            m_difficultyHeader = Find("Header Difficulty");
        }

        void ToggleHeader(GameObject header, params Text[] stats)
        {
            if (!header)
                return;

            bool any = false;
            foreach (var text in stats)
            {
                if (text && text.transform.parent.gameObject.activeSelf)
                {
                    any = true;
                    break;
                }
            }

            header.SetActive(any);
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
        
        protected string GetDifficultyLabel(float value)
        {
            if (value >= 1f && value < 2f)
                return "I'm scared";
            if (value >= 2f && value < 3f)
                return "Is this even on?";
            if (value >= 3f && value < 4f)
                return "Baby mode";
            if (value >= 4f && value < 5f)
                return "Casual stroll";
            if (value >= 5f && value < 6f)
                return "Now we’re talking";
            if (value >= 6f && value < 7f)
                return "Bring it on!";
            if (value >= 7f && value < 8f)
                return "Why do I hear boss music?";
            if (value >= 8f && value < 9f)
                return "Send help!!";
            if (value >= 9f && value < 9.01f)
                return "Nightmare fuel";
            if (value >= 9.01f && value <= 10f)
                return "Abandon all hope";
            return value.ToString("F2");
        }

        int CalculateStorylineProgress(CharacterInstance character, PlayerBehaviorLogger logger)
        {
            if (character == null || logger == null)
                return 0;

            float questsRatio = 0f;
            var db = GameDatabase.instance;
            if (db != null)
            {
                var playerClass = CharacterInstance.GetClassBitFromName(character.data.name);
                string pType = character.currentDynamicPlayerType;
                int total = 0;

                foreach (var q in db.quests)
                {
                    if (q == null)
                        continue;

                    if (q.requiredClass != CharacterClassRestrictions.None &&
                        (q.requiredClass & playerClass) == 0)
                        continue;

                    if (q.questType == QuestType.Exclusive)
                    {
                        if (pType == "Killer" && !q.forKiller) continue;
                        if (pType == "Achiever" && !q.forAchiever) continue;
                        if (pType == "Explorer" && !q.forExplorer) continue;
                        if (pType == "Socializer" && !q.forSocializer) continue;
                    }

                    total++;
                }

                questsRatio = total > 0 ? logger.questsCompleted / (float)total : 0f;
            }

            float zonesRatio = 0f;
            if (LevelZones.instance != null)
            {
                int totalZones = LevelZones.instance.zones.Count;
                zonesRatio = totalZones > 0 ? logger.zonesDiscovered / (float)totalZones : 0f;
            }

            float waypointsRatio = 0f;
            if (LevelWaypoints.instance != null && LevelWaypoints.instance.waypoints != null)
            {
                int totalWaypoints = LevelWaypoints.instance.waypoints.Count;
                waypointsRatio = totalWaypoints > 0 ? logger.waypointsDiscovered / (float)totalWaypoints : 0f;
            }

            float levelRatio = 0f;
            if (Game.instance != null && character.stats != null && Game.instance.maxLevel > 0)
                levelRatio = character.stats.currentLevel / (float)Game.instance.maxLevel;

            var classBit = CharacterInstance.GetClassBitFromName(character.data.name);
            int tierIndex = ClassHierarchy.GetTier(classBit);
            float classRatio = (tierIndex + 1) / 3f;

            float avg = (questsRatio + zonesRatio + waypointsRatio + levelRatio + classRatio) / 5f;
            int percent = Mathf.RoundToInt(avg * 100f);
            return Mathf.Clamp(percent, 0, 100);
        }

        public void Refresh()
        {
            if (m_entity == null || m_entity.stats == null)
                return;

            var s = m_entity.stats;

            string playerType = PlayerBehaviorLogger.Instance?.currentDynamicPlayerType;
            if (string.IsNullOrEmpty(playerType) || playerType == "Unknown" || playerType == "Undefined")
                playerType = Game.instance?.currentCharacter?.playerType;

            if (playerTypeDescriptionText)
            {
                if (playerType != null && s_playerTypeDescriptions.TryGetValue(playerType, out var desc))
                    playerTypeDescriptionText.text = desc;
                else
                    playerTypeDescriptionText.text = string.Empty;
            }

            SetNumericStat(fireResistanceText, s.fireResistance, $"{s.fireResistance} pts");
            SetNumericStat(waterResistanceText, s.waterResistance, $"{s.waterResistance} pts");
            SetNumericStat(iceResistanceText, s.iceResistance, $"{s.iceResistance} pts");
            SetNumericStat(earthResistanceText, s.earthResistance, $"{s.earthResistance} pts");
            SetNumericStat(airResistanceText, s.airResistance, $"{s.airResistance} pts");
            SetNumericStat(lightningResistanceText, s.lightningResistance, $"{s.lightningResistance} pts");
            SetNumericStat(shadowResistanceText, s.shadowResistance, $"{s.shadowResistance} pts");
            SetNumericStat(lightResistanceText, s.lightResistance, $"{s.lightResistance} pts");
            SetNumericStat(arcaneResistanceText, s.arcaneResistance, $"{s.arcaneResistance} pts");

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
            SetNumericStat(manaRegenPerSecondText, manaTotal, manaTotal.ToString("F2") + " pts");
            
            float healthTotal = s.healthRegenPerSecond + s.healthRegenPer5Seconds / 5f + s.healthRegenPer30Seconds / 30f;
            SetNumericStat(healthRegenPerSecondText, healthTotal, healthTotal.ToString("F2") + " pts");

            float experienceTotal = s.experiencePerSecondPercent + s.experiencePer5SecondsPercent / 5f + s.experiencePer30SecondsPercent / 30f;
            SetNumericStat(experiencePerSecondPercentText, experienceTotal, $"{experienceTotal:F2}%");

            SetNumericStat(chanceToBlockText, s.chanceToBlock, $"{s.chanceToBlock * 100f:F2}%");
            SetNumericStat(blockSpeedText, s.blockSpeed);

            SetNumericStat(maxCombosText, s.maxCombos);
            SetNumericStat(timeToStopComboText, s.timeToStopCombo, $"{s.timeToStopCombo:F2}s");
            SetNumericStat(nextComboDelayText, s.nextComboDelay, $"{s.nextComboDelay:F2}s");

            SetNumericStat(additionalManaText, s.additionalMana, $"{s.additionalMana} mana");
            SetNumericStat(additionalHealthText, s.additionalHealth, $"{s.additionalHealth} HP");
            SetNumericStat(increaseAttackSpeedPercentText, s.increaseAttackSpeedPercent, $"{s.increaseAttackSpeedPercent}%");
            SetNumericStat(increaseDamageValueText, s.increaseDamageValue, $"{s.increaseDamageValue} dmg");
            SetNumericStat(increaseMagicalDamageValueText, s.increaseMagicalDamageValue, $"{s.increaseMagicalDamageValue} magic dmg");
            SetNumericStat(additionalExperienceRewardPercentText, s.additionalExperienceRewardPercent, $"{s.additionalExperienceRewardPercent}%");
            SetNumericStat(additionalMoneyRewardPercentText, s.additionalMoneyRewardPercent, $"{s.additionalMoneyRewardPercent}%");
            SetNumericStat(additionalAmberlingsPerMinuteText, s.additionalAmberlingsPerMinute, $"{s.additionalAmberlingsPerMinute} coins");
            SetNumericStat(additionalLunarisPerMinuteText, s.additionalLunarisPerMinute, $"{s.additionalLunarisPerMinute} coins");
            SetNumericStat(additionalSolmiresPerMinuteText, s.additionalSolmiresPerMinute, $"{s.additionalSolmiresPerMinute} coins");
            SetNumericStat(itemPricePercentText, s.itemPricePercent, $"{s.itemPricePercent}%");

            var logger = PlayerBehaviorLogger.Instance;
            if (logger != null)
            {
                SetNumericStat(zonesDiscoveredText, logger.zonesDiscovered);
                SetNumericStat(achievementsUnlockedText, logger.unlockedAchievements?.Count ?? 0);
                SetNumericStat(questsCompletedText, logger.questsCompleted);
            }

            var character = Game.instance?.currentCharacter;
            if (character != null && logger != null)
            {
                int percent = CalculateStorylineProgress(character, logger);
                SetNumericStat(storylineCompletedText, percent, $"{percent}%");
            }
            
            float currentDifficulty = DifficultyManager.Instance != null ? DifficultyManager.Instance.GetRawDifficulty() : 0f;
            float avgMultiplier = AiDDAEntityStatsManager.Instance != null ? AiDDAEntityStatsManager.Instance.GetAverageMultiplier() : 0f;

            SetNumericStat(currentDifficultyText, currentDifficulty, GetDifficultyLabel(currentDifficulty));
            SetNumericStat(difficultyMultiplierText, avgMultiplier, avgMultiplier.ToString("F2"));

            ToggleHeader(m_resistancesHeader, fireResistanceText, waterResistanceText, iceResistanceText, earthResistanceText, airResistanceText, lightningResistanceText, shadowResistanceText, lightResistanceText, arcaneResistanceText);
            ToggleHeader(m_immunitiesHeader, magicImmunityText, fireImmunityText, waterImmunityText, iceImmunityText, earthImmunityText, airImmunityText, lightningImmunityText, shadowImmunityText, lightImmunityText, arcaneImmunityText);
            ToggleHeader(m_regenerationsHeader, manaRegenPerSecondText, healthRegenPerSecondText, experiencePerSecondPercentText);
            ToggleHeader(m_blocksHeader, chanceToBlockText, blockSpeedText);
            ToggleHeader(m_combosHeader, maxCombosText, timeToStopComboText, nextComboDelayText);
            ToggleHeader(m_bonusesHeader, additionalManaText, additionalHealthText, increaseAttackSpeedPercentText, increaseDamageValueText, increaseMagicalDamageValueText, additionalExperienceRewardPercentText, additionalMoneyRewardPercentText, additionalAmberlingsPerMinuteText, additionalLunarisPerMinuteText, additionalSolmiresPerMinuteText, itemPricePercentText);
            ToggleHeader(m_progressHeader, zonesDiscoveredText, achievementsUnlockedText, questsCompletedText, storylineCompletedText);
            ToggleHeader(m_difficultyHeader, currentDifficultyText, difficultyMultiplierText);
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        protected virtual void OnDestroy()
        {
            if (m_entity != null && m_entity.stats != null)
            {
                m_entity.stats.onLevelUp.RemoveListener(Refresh);
                m_entity.stats.onRecalculate.RemoveListener(Refresh);
            }

            if (m_buffManager != null)
            {
                m_buffManager.onBuffAdded.RemoveListener(HandleBuffChanged);
                m_buffManager.onBuffRemoved.RemoveListener(HandleBuffChanged);
            }

            if (m_itemManager != null)
            {
                m_itemManager.onChanged.RemoveListener(Refresh);
            }
        }
    }
}