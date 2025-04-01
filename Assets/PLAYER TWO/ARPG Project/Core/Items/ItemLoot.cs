using System.Collections;
using UnityEngine;
using System.Linq;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Item/Item Loot")]
    public class ItemLoot : MonoBehaviour
    {
        [Tooltip("The Item Loot Stats settings for the loots.")]
        public ItemLootStats stats;

        [Header("Ground Settings")]
        [Tooltip(
            "The ground layer mask. The loot won't be "
                + "spawned if the ground below the loot point is not in this layer."
        )]
        public LayerMask groundMask = -5;

        [Tooltip(
            "The maximum distance to the ground. If the ground is "
                + "further than this distance from the loot point, the loot won't be spawned."
        )]
        public float maxGroundDistance = 2f;

        protected Entity m_entity;

        protected WaitForSeconds m_waitForLootLoopDelay;

        protected const float k_groundOffset = 0.1f;
        protected const float k_lootRayOffset = 0.5f;
        protected const float k_lootLoopDelay = 0.1f;

        protected Game m_game => Game.instance;
        protected CollectibleItem m_itemPrefab => m_game.collectibleItemPrefab;
        protected CollectibleMoney m_moneyPrefab => m_game.collectibleMoneyPrefab;

        protected virtual void InitializeWaits()
        {
            m_waitForLootLoopDelay = new WaitForSeconds(k_lootLoopDelay);
        }

        protected virtual void InitializeEntity()
        {
            if (TryGetComponent(out m_entity))
                m_entity.onDie.AddListener(Loot);
        }

        /// <summary>
        /// Starts the looting routine.
        /// </summary>
        public virtual void Loot()
        {
            if (Random.Range(0, 1f) > stats.lootChance)
                return;

            StopAllCoroutines();
            StartCoroutine(LootRoutine());
        }

        protected virtual void InstantiateItem(Vector3 position)
        {
            if (stats.jewelDrops != null && stats.jewelDrops.Length > 0 && Random.value <= stats.jewelDropChance)
            {
                var possibleJewels = stats.jewelDrops
                    .Where(j => j.jewel != null && Random.value <= j.dropChance)
                    .Select(j => j.jewel)
                    .ToList();
    
                if (possibleJewels.Count > 0)
                {
                    var jewel = possibleJewels[Random.Range(0, possibleJewels.Count)];
                    var jewelItem = new ItemInstance(jewel, false);
                    jewelItem.stack = 1;

                    var jewelDrop = Instantiate(m_itemPrefab, position, Quaternion.identity);
                    jewelDrop.SetItem(jewelItem);
                    return;
                }
            }

            var index = Random.Range(0, stats.items.Length);
            var item = new ItemInstance(
                stats.items[index],
                stats.generateAttributes,
                stats.minAttributes,
                stats.maxAttributes
            );

            if (item.IsEquippable())
            {
                int lvl = Random.Range(stats.minItemLevel, stats.maxItemLevel + 1);
                int maxLvl = item.GetEquippable().maxUpgradeLevel;

                lvl = Mathf.Clamp(lvl, 0, maxLvl);
                for (int i = 0; i < lvl; i++)
                    item.UpgradeLevel();
            }

            var collectible = Instantiate(m_itemPrefab, position, Quaternion.identity);
            collectible.SetItem(item);
        }

        protected virtual void InstantiateMoney(Vector3 position)
        {
            var level = m_entity ? m_entity.stats.level : 1;
            var money = Instantiate(m_moneyPrefab, position, Quaternion.identity);
            var baseAmount = Random.Range(stats.minMoneyAmount, stats.maxMoneyAmount);
            var multiplier = 1 + (level - 1) * m_game.enemyLootMoneyIncreaseRate;
            var finalAmount = Mathf.RoundToInt(baseAmount * multiplier);
            money.amount = finalAmount;
        }

        protected virtual Vector3 GetLootOrigin()
        {
            var random = Random.insideUnitCircle;
            var radius = Random.Range(stats.randomPositionMinRadius, stats.randomPositionMaxRadius);
            var randomOffset = new Vector3(random.x, 0, random.y) * radius;
            var position = transform.position + Vector3.up * k_lootRayOffset;

            if (stats.randomPosition)
                position += randomOffset;

            return position;
        }

        protected IEnumerator LootRoutine()
        {
            for (int i = 0; i <= stats.loopCount; i++)
            {
                yield return m_waitForLootLoopDelay;

                var origin = GetLootOrigin();

                if (
                    Physics.Raycast(
                        origin,
                        Vector3.down,
                        out var hit,
                        maxGroundDistance,
                        groundMask,
                        QueryTriggerInteraction.Ignore
                    )
                )
                {
                    var position = hit.point + Vector3.up * k_groundOffset;

                    if (Random.Range(0, 1f) > stats.moneyChance)
                    {
                        InstantiateItem(position);
                        continue;
                    }

                    InstantiateMoney(position);
                }
            }
        }

        protected virtual void Start()
        {
            InitializeWaits();
            InitializeEntity();
        }
    }
}
