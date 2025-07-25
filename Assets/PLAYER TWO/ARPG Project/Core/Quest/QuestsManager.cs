//  ZMODYFIKOWANO 31 GRUDNIA 2024

using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    public class QuestsManager
    {
        /// <summary>
        /// Invoked when a new Quest Instance was added to the active quests list.
        /// </summary>
        public System.Action<QuestInstance> onQuestAdded;

        /// <summary>
        /// Invoked when the progress of any Quest Instance was changed.
        /// </summary>
        public System.Action<QuestInstance> onProgressChanged;

        /// <summary>
        /// Invoked when any Quest Instance was completed.
        /// </summary>
        public System.Action<QuestInstance> onQuestCompleted;

        /// <summary>
        /// Invoked when a Quest Instance was removed from the active quests list.
        /// </summary>
        public System.Action<QuestInstance> onQuestRemoved;

        protected List<QuestInstance> m_quests = new();

        /// <summary>
        /// Returns an array as copy of the active quests list.
        /// </summary>
        public QuestInstance[] list => m_quests.ToArray();

        /// <summary>
        /// Overrides the list of active quests from a given array.
        /// </summary>
        /// <param name="quests">The array you want to read from.</param>
        public virtual void SetQuests(QuestInstance[] quests)
        {
            m_quests = new List<QuestInstance>(quests);
        }

        /// <summary>
        /// Adds a new Quest Instance to the active quests list.
        /// </summary>
        /// <param name="quest">The Quest you want to create the instance from.</param>
        public virtual void AddQuest(Quest quest)
        {
            if (ContainsQuest(quest))
                return;

            m_quests.Add(new QuestInstance(quest));
            onQuestAdded?.Invoke(m_quests[^1]);

            AdjustQuestItems(m_quests[^1]);
        }

        /// <summary>
        /// Removes a Quest Instance from the active quests list.
        /// </summary>
        /// <param name="quest">The Quest you want to remove.</param>
        public virtual void RemoveQuest(Quest quest)
        {
            if (!TryGetQuest(quest, out var instance))
                return;

            m_quests.Remove(instance);
            onQuestRemoved?.Invoke(instance);
        }

        /// <summary>
        /// Gets a Quest Instance from the active quests list.
        /// </summary>
        /// <param name="quest">The data you are trying to find.</param>
        /// <param name="instance">The instance representing the given Quest.</param>
        /// <returns>Returns true if the Quest in the the active quests list.</returns>
        public virtual bool TryGetQuest(Quest quest, out QuestInstance instance)
        {
            instance = m_quests.Find(q => q.data == quest);
            return instance != null;
        }

        /// <summary>
        /// Completes a given Quest Instance.
        /// </summary>
        /// <param name="quest">The Quest Instance you want to complete.</param>
        public virtual void CompleteQuest(QuestInstance quest)
        {
            if (quest.completed)
                return;

            if (Level.instance.player)
                quest.Reward(Level.instance.player);

            quest.Complete();

            PlayerBehaviorLogger.Instance?.LogQuestCompleted();

            onQuestCompleted?.Invoke(quest);

            foreach (var giver in QuestGiverRegistry.Instance.GetAll())
            {
                if (giver.HasQuest(quest.data))
                {
                    giver.OnQuestCompleted(quest);
                    break;
                }
            }
        }

        /// <summary>
        /// Returns true if the active quests list contains a given Quest.
        /// </summary>
        /// <param name="quest">The Quest you want to search on the list.</param>
        public virtual bool ContainsQuest(Quest quest) => m_quests.Exists((q) => q.data == quest);

        /// <summary>
        /// Completes all quests from tue active quests list containing a given destination scene name.
        /// </summary>
        /// <param name="scene">The destination scene name.</param>
        public virtual void ReachedScene(string scene)
        {
            foreach (var quest in m_quests)
            {
                if (quest.CanCompleteOnScene(scene))
                    CompleteQuest(quest);
            }
        }

        /// <summary>
        /// Adds progress to all quests from the active quests list containing a given progress key.
        /// It also triggers the completion of Quests that reached their target progress.
        /// </summary>
        /// <param name="key">The progress key of the quests.</param>
        public virtual void AddProgress(string key)
        {
            foreach (var quest in m_quests)
            {
                if (quest.CanAddProgress(key))
                {
                    quest.progress++;
                    onProgressChanged?.Invoke(quest);

                    if (quest.progress >= quest.GetFinalTargetProgress())
                    {
                        if (quest.IsMultiStage())
                        {
                            var stage = quest.GetCurrentStage();

                            if (stage.completingMode == Quest.CompletingMode.FetchAfterKill)
                            {
                                Debug.Log($"[Quest] '{quest.data.title}' (etap {quest.currentStageIndex + 1}) wymaga zwrotu przedmiotu do {stage.returnToNPC}.");
                            }
                            else
                            {
                                quest.AdvanceStage();

                                if (quest.IsFullyCompleted())
                                {
                                    CompleteQuest(quest);
                                }
                            }
                        }
                        else
                        {
                            if (quest.data.IsFetchAfterKill())
                            {
                                Debug.Log($"[Quest] '{quest.data.title}' wymaga zwrócenia przedmiotu do {quest.data.returnToNPC}.");
                            }
                            else
                            {
                                CompleteQuest(quest);
                            }
                        }
                    }
                }
            }
        }
        
        public void CompleteManualQuest(Quest quest)
        {
            if (!TryGetQuest(quest, out var instance))
                return;

            if (!instance.RequiresManualCompletion())
                return;

            foreach (var giver in QuestGiverRegistry.Instance.GetAll())
            {
                if (giver.HasQuest(quest))
                {
                    giver.OnQuestCompleted(instance);
                    break;
                }
            }

            if (Level.instance.player)
                instance.Reward(Level.instance.player);

            instance.Complete();
            onQuestCompleted?.Invoke(instance);
        }

        
        /// <summary>
        /// Zwraca pierwszy dostępny nieukończony quest.
        /// </summary>
        /// <summary>
        public Quest GetNextAvailableQuest()
        {
            foreach (var questInstance in m_quests)
            {
                if (!questInstance.completed)
                    return questInstance.data;
            }

            return null;
        }

        /// <summary>
        /// Triggers the completion of a given Quest from the active quests list.
        /// </summary>
        /// <param name="quest">The Quest to trigger completion.</param>
        public virtual void Trigger(Quest quest)
        {
            if (!TryGetQuest(quest, out var instance))
                return;

            if (!instance.CanCompleteByTrigger())
                return;

            if (instance.IsMultiStage())
            {
                instance.AdvanceStage();

                if (instance.IsFullyCompleted())
                {
                    CompleteQuest(instance);
                }
            }
            else
            {
                CompleteQuest(instance);
            }
        }


        /*
        public void AdjustQuestItems(QuestInstance questInstance)
        {
            if (questInstance == null || !questInstance.data.IsProgress()) return;

            string itemKey = questInstance.data.progressKey;
            int requiredAmount = questInstance.GetFinalTargetProgress(); 

            QuestItem[] existingItems = Object.FindObjectsByType<QuestItem>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(item => item.itemKey == itemKey)
                .ToArray();

            int currentAmount = existingItems.Length;

            Debug.Log($"[QUEST] Checking quest items for key: {itemKey} - Found {currentAmount}, Required: {requiredAmount}");

            if (currentAmount < requiredAmount)
            {
                int toSpawn = requiredAmount - currentAmount;
                Debug.Log($"[QUEST] Spawning {toSpawn} missing Quest Items for key: {itemKey}");
                SpawnQuestItems(itemKey, toSpawn);
            }
            else if (currentAmount > requiredAmount)
            {
                int toDisable = currentAmount - requiredAmount;
                Debug.Log($"[QUEST] Disabling {toDisable} extra Quest Items for key: {itemKey}");
                DisableExtraQuestItems(existingItems, toDisable);
            }
        }
        */

        public void AdjustQuestItems(QuestInstance questInstance)
        {
            if (questInstance == null)
                return;

            string itemKey = null;
            int requiredAmount = 0;

            // ✅ Obsługa Multi-Stage
            if (questInstance.IsMultiStage())
            {
                var stage = questInstance.GetCurrentStage();
                if (stage.completingMode != Quest.CompletingMode.Progress)
                    return;

                itemKey = stage.progressKey;
                requiredAmount = stage.targetProgress;
            }
            else if (questInstance.data.IsProgress())
            {
                itemKey = questInstance.data.progressKey;
                requiredAmount = questInstance.GetFinalTargetProgress();
            }
            else
            {
                return; // ❌ Nie jest questem Progress i nie ma etapu Progress
            }

            QuestItem[] existingItems = Object.FindObjectsByType<QuestItem>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(item => item.itemKey == itemKey)
                .ToArray();

            int currentAmount = existingItems.Length;

            Debug.Log($"[QUEST] Checking quest items for key: {itemKey} - Found {currentAmount}, Required: {requiredAmount}");

            if (currentAmount < requiredAmount)
            {
                int toSpawn = requiredAmount - currentAmount;
                Debug.Log($"[QUEST] Spawning {toSpawn} missing Quest Items for key: {itemKey}");
                SpawnQuestItems(itemKey, toSpawn);
            }
            else if (currentAmount > requiredAmount)
            {
                int toDisable = currentAmount - requiredAmount;
                Debug.Log($"[QUEST] Disabling {toDisable} extra Quest Items for key: {itemKey}");
                DisableExtraQuestItems(existingItems, toDisable);
            }
        }

        /// <summary>
        /// Tworzy brakujące przedmioty na mapie.
        /// </summary>
        public void SpawnQuestItems(string itemKey, int amount)
        {
            GameObject prefab = GetQuestItems(itemKey);

            if (prefab == null)
            {
               // Debug.LogError($"[QUEST] ERROR: No prefab found for itemKey {itemKey}");
                return;
            }

            GameObject parent = GameObject.Find("Quest Items");

            for (int i = 0; i < amount; i++)
            {
                Vector3 spawnPosition = GetValidSpawnPosition(itemKey);

                if (spawnPosition == Vector3.zero)
                {
                    Game.instance.StartCoroutine(SpawnWithDelay(itemKey, prefab, parent, 2f));
                    continue;
                }

                GameObject newItem = UnityEngine.Object.Instantiate(prefab, spawnPosition, Quaternion.identity);

                if (parent != null)
                {
                    newItem.transform.SetParent(parent.transform);
                }

                newItem.SetActive(true);
            }
        }

        private IEnumerator SpawnWithDelay(string itemKey, GameObject prefab, GameObject parent, float delay)
        {
            yield return new WaitForSeconds(delay);

            List<Transform> existingItems = parent.GetComponentsInChildren<QuestItem>(true)
                                                .Where(item => item.itemKey == itemKey)
                                                .Select(item => item.transform)
                                                .ToList();

            if (existingItems.Count == 0)
            {
                Debug.LogError($"[QUEST] ERROR: No existing items found for itemKey {itemKey} after delay!");
                yield break;
            }

            Vector3 spawnPosition = existingItems[Random.Range(0, existingItems.Count)].position;

            GameObject newItem = UnityEngine.Object.Instantiate(prefab, spawnPosition, Quaternion.identity);
            if (parent != null)
            {
                newItem.transform.SetParent(parent.transform);
            }

            newItem.SetActive(true);
        }

        /// <summary>
        /// Ukrywa nadmiarowe przedmioty na mapie.
        /// </summary>
        private void DisableExtraQuestItems(QuestItem[] items, int amount)
        {
            int count = 0;
            foreach (var item in items)
            {
                if (count >= amount) break;
                item.gameObject.SetActive(false);
                count++;
            }
        }

        /// <summary>
        /// Pobiera prefabrykat QuestItem na podstawie itemKey.
        /// </summary>
        public GameObject GetQuestItems(string itemKey)
        {
            return GameDatabase.instance.gameData.questItems.FirstOrDefault(prefab =>
            {
                QuestItem questItem = prefab.GetComponent<QuestItem>();
                return questItem != null && questItem.itemKey == itemKey;
            });
        }

        /// <summary>
        /// Znajduje losową pozycję do wygenerowania przedmiotu.
        /// </summary>
        private Vector3 GetValidSpawnPosition(string itemKey)
        {
            Vector3 spawnPosition;
            float maxDistance = 5f;
            int maxAttempts = 20;

            GameObject questItemsParent = GameObject.Find("Quest Items");

            if (questItemsParent == null)
            {
                Debug.LogError("[QUEST] ERROR: 'Quest Items' parent object not found in the scene!");
                return Vector3.zero;
            }

            List<Transform> existingItems = questItemsParent.GetComponentsInChildren<QuestItem>(true)
                                                            .Where(item => item.itemKey == itemKey)
                                                            .Select(item => item.transform)
                                                            .ToList();

            if (existingItems.Count == 0)
            {
                Debug.LogError($"[QUEST] ERROR: No existing items found for itemKey '{itemKey}' inside 'Quest Items'!");
                return Vector3.zero;
            }

            for (int i = 0; i < maxAttempts; i++)
            {
                Vector3 basePosition = existingItems[Random.Range(0, existingItems.Count)].position;

                float offsetX = Random.Range(-maxDistance, maxDistance);
                float offsetZ = Random.Range(-maxDistance, maxDistance);
                spawnPosition = new Vector3(basePosition.x + offsetX, basePosition.y, basePosition.z + offsetZ);

                if (Terrain.activeTerrain != null)
                {
                    float terrainHeight = Terrain.activeTerrain.SampleHeight(spawnPosition);
                    spawnPosition.y = terrainHeight + 0.2f;
                }

                if (!Physics.CheckSphere(spawnPosition, 0.7f))
                {
                    return spawnPosition;
                }
            }

            Debug.LogWarning($"[QUEST] WARNING: Could not find a fully free position for {itemKey}, using delayed fallback.");
            return Vector3.zero;
        }
    }
}
