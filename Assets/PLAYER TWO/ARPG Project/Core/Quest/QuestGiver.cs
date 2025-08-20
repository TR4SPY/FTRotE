//  ZMODYFIKOWANO 31 GRUDNIA 2024

using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using AI_DDA.Assets.Scripts;
using System.Collections;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Quest/Quest Giver")]
    public class QuestGiver : Interactive
    {
        public enum State
        {
            None,
            QuestAvailable,
            QuestInProgress,
        }

        [Header("Quest Giver Settings")]
        [Tooltip("The list of Quests this Quest Giver offers to the Player.")]
        public Quest[] quests;

        [Tooltip("Additional quests available only for Achiever-type Players.")]
        public Quest[] additionalQuests;

        [Space(10)]
        public UnityEvent<State> onStateChange;
        public Dialog assignedDialog;


        /// <summary>
        /// The current state of the Quest Giver.
        /// </summary>
        public State state { get; protected set; } = State.None;

        protected QuestsManager m_manager => Game.instance.quests;

        protected override void Start()
        {
            base.Start();
            InitializeCallbacks();
            InitializeStates();

            var stats = Game.instance.currentCharacter?.stats;
            if (stats != null)
            {
                stats.onLevelUp.AddListener(CheckClassUpgradeQuestAvailability);
                stats.onLevelUp.AddListener(CheckSpecializationQuestAvailability);
            }

            CheckClassUpgradeQuestAvailability();
            CheckSpecializationQuestAvailability();
        }

        protected virtual void InitializeCallbacks()
        {
            m_manager.onQuestAdded += OnQuestAdded;
            m_manager.onQuestCompleted += OnQuestCompleted;
            m_manager.onQuestRemoved += OnQuestRemoved;
        }

        public virtual void InitializeStates()
        {
            QuestInstance instance;
            var completedQuests = quests.Count(quest =>
                m_manager.TryGetQuest(quest, out instance) && instance.completed
            );

            if (completedQuests == quests.Length)
            {
                ChangeStateTo(State.None);
                return;
            }

            var state = quests.Any(quest =>
                m_manager.TryGetQuest(quest, out instance) && !instance.completed
            )
                ? State.QuestInProgress
                : State.QuestAvailable;

            ChangeStateTo(state);
        }

        /// <summary>
        /// Returns the first non-completed Quest from the quests array.
        /// </summary>
        public virtual Quest CurrentQuest()
        {
            foreach (var quest in quests)
            {
                if (!IsQuestEligible(quest))
                    continue;

                if (!m_manager.TryGetQuest(quest, out var instance) || !instance.completed)
                    return quest;
            }

            return null;
        }


        public bool HasQuest(Quest quest)
        {
            return quests.Contains(quest) || additionalQuests.Contains(quest);
        }

        /// <summary>
        /// Determines if the current player meets class and level requirements for a quest.
        /// </summary>
        private bool IsQuestEligible(Quest quest)
        {
            var player = Game.instance.currentCharacter;
            var className = player.Entity?.name?.Replace("(Clone)", "").Trim() ?? string.Empty;

            if (!ClassHierarchy.NameToBits.TryGetValue(className, out var currentClass))
                return false;

            if (quest.requiredClass != CharacterClassRestrictions.None &&
                (quest.requiredClass & currentClass) == 0)
                return false;

            if (quest.questType == QuestType.Specialization)
            {
                int currentTier = ClassHierarchy.GetTier(currentClass);
                int unlockLevel = CharacterSpecializations.GetTierUnlockLevel(currentTier);
                if (player.stats.currentLevel < unlockLevel)
                    return false;
                if (player.specializations.IsTierUnlocked(currentTier))
                    return false;
            }

            if (quest.requireMaxLevel && player.stats.currentLevel < Game.instance.maxLevel)
                return false;

            return true;
        }


        public Quest CurrentExclusiveQuest(string pType)
        {
            foreach (var quest in additionalQuests)
            {
                if (!MatchesPlayerType(quest, pType)) continue;
                if (!IsQuestOwnedByThisNPC(quest)) continue;
                if (!IsQuestEligible(quest)) continue;
                if (InQuestsManager(quest) && !IsQuestCompleted(quest))
                    return quest;
            }

            foreach (var quest in additionalQuests)
            {
                if (!MatchesPlayerType(quest, pType)) continue;
                if (!IsQuestOwnedByThisNPC(quest)) continue;
                if (!IsQuestEligible(quest)) continue;
                if (!InQuestsManager(quest))
                    return quest;
            }

            return null;
        }

        public Quest CurrentClassUpgradeQuest()
        {
            var player = Game.instance.currentCharacter;
            var className = player.Entity?.name?.Replace("(Clone)", "").Trim() ?? "";
            Debug.Log($"[ClassUpgrade] Entity class name: {className}");

            if (!ClassHierarchy.NameToBits.TryGetValue(className, out var currentClass))
            {
                Debug.LogWarning($"[ClassUpgrade] Klasa nieznana: '{className}'");
                return null;
            }

            int currentLevel = player.stats.currentLevel;
            int maxLevel = Game.instance.maxLevel;
            Debug.Log($"[ClassUpgrade] Level: {currentLevel}, Max: {maxLevel}, Class: {currentClass}");

            var allQuests = quests.Concat(additionalQuests);
            foreach (var quest in allQuests)
            {
                if (quest.questType != QuestType.ClassUpgrade)
                {
                    Debug.Log($"[ClassUpgrade] Pomijam '{quest.title}' – nie jest typu ClassUpgrade");
                    continue;
                }

                if (quest.requiredClass != CharacterClassRestrictions.None &&
                    (quest.requiredClass & currentClass) == 0)
                {
                    Debug.Log($"[ClassUpgrade] Pomijam '{quest.title}' – wymaga klasy {quest.requiredClass}, gracz ma {currentClass}");
                    continue;
                }

                if (quest.requireMaxLevel && currentLevel < maxLevel)
                {
                    Debug.Log($"[ClassUpgrade] Pomijam '{quest.title}' – wymaga max levelu");
                    continue;
                }

                if (!Game.instance.quests.ContainsQuest(quest) || 
                    (Game.instance.quests.TryGetQuest(quest, out var q) && !q.completed))
                {
                    Debug.Log($"[ClassUpgrade] Zwracam questa: {quest.title}");
                    return quest;
                }
            }

            Debug.Log("[ClassUpgrade] Żaden quest nie pasuje.");
            return null;
        }

        public Quest CurrentSpecializationQuest()
        {
            var allQuests = quests.Concat(additionalQuests);
            foreach (var quest in allQuests)
            {
                if (quest.questType != QuestType.Specialization)
                    continue;

                if (!IsQuestEligible(quest))
                    continue;

                if (!Game.instance.quests.ContainsQuest(quest) ||
                    (Game.instance.quests.TryGetQuest(quest, out var q) && !q.completed))
                    return quest;
            }

            return null;
        }

     protected void CheckClassUpgradeQuestAvailability()
        {
            var quest = CurrentClassUpgradeQuest();
            if (quest == null)
                return;

            var player = Game.instance.currentCharacter;
            var className = player.Entity?.name?.Replace("(Clone)", "").Trim() ?? string.Empty;
            if (!ClassHierarchy.NameToBits.TryGetValue(className, out var currentClass))
                return;

            var nextClass = ClassHierarchy.GetNextTierClass(currentClass);
            if (nextClass == CharacterClassRestrictions.None)
                return;

            int currentTier = ClassHierarchy.GetTier(currentClass);
            int unlockLevel = CharacterSpecializations.GetTierUnlockLevel(currentTier);

            if (player.stats.currentLevel >= unlockLevel)
            {
                ChangeStateTo(State.QuestAvailable);
            }
        }

        protected void CheckSpecializationQuestAvailability()
        {
            var quest = CurrentSpecializationQuest();

            if (quest != null)
            {
                ChangeStateTo(State.QuestAvailable);
            }
        }

        private bool MatchesPlayerType(Quest q, string pType)
        {
            if (q.questType != QuestType.Exclusive) return false;
            if (q.forKiller && pType == "Killer") return true;
            if (q.forAchiever && pType == "Achiever") return true;
            if (q.forExplorer && pType == "Explorer") return true;
            if (q.forSocializer && pType == "Socializer") return true;
            return false;
        }
        private bool IsQuestOwnedByThisNPC(Quest q)
        {
            return true;
        }

        private bool InQuestsManager(Quest q)
        {
            return Game.instance.quests.ContainsQuest(q);
        }

        private bool IsQuestCompleted(Quest q)
        {
            if (Game.instance.quests.TryGetQuest(q, out var instance))
                return instance.completed;
            return false;
        }

        public static QuestGiver FindReturnNPC(string npcId)
        {
        #if UNITY_2023_1_OR_NEWER
            var npcs = Object.FindObjectsByType<QuestGiver>(FindObjectsSortMode.None);
        #else
            var npcs = Object.FindObjectsOfType<QuestGiver>();
        #endif

            foreach (var npc in npcs)
            {
                if (npc.name == npcId)
                {
                    return npc;
                }
            }

            Debug.LogWarning($"NPC {npcId} not found in the scene.");
            return null;
        }

        public void ReceiveFetchItem(QuestItemReward item)
        {
            var player = Game.instance.currentCharacter;

            if (!player.inventory.HasItem(item.data))
            {
                Debug.Log($"[Quest] Gracz nie ma wymaganego przedmiotu: {item.data.name}");
                return;
            }

            if (!player.inventory.RemoveItem(item.data))
            {
                Debug.LogWarning($"[Quest] Nie udało się usunąć przedmiotu: {item.data.name}");
                return;
            }

            Debug.Log($"[Quest] Przedmiot {item.data.name} został zwrócony do {name}");

            Quest currentQuest = CurrentQuest();
            if (currentQuest == null)
            {
                Debug.LogWarning("[Quest] Nie znaleziono aktualnego questa przy NPC.");
                return;
            }

            if (!Game.instance.quests.TryGetQuest(currentQuest, out var instance))
            {
                Debug.LogWarning("[Quest] Brak instancji questa dla gracza.");
                return;
            }

            if (instance.IsMultiStage())
            {
                var stage = instance.GetCurrentStage();
                if (stage.completingMode == Quest.CompletingMode.FetchAfterKill)
                {
                    Debug.Log($"[Quest] Etap {instance.currentStageIndex + 1} questa '{instance.data.title}' zakończony.");
                    instance.AdvanceStage();

                    if (instance.IsFullyCompleted())
                    {
                        Debug.Log($"[Quest] ✅ Quest zakończony: {instance.data.title}");
                        Game.instance.quests.CompleteQuest(instance);
                    }
                }
                else
                {
                    Debug.LogWarning($"[Quest] Błąd logiczny: aktualny etap nie jest typu FetchAfterKill.");
                }
            }
            else
            {
                Debug.Log($"[Quest] ✅ Quest zakończony: {currentQuest.title}");
                Game.instance.quests.CompleteQuest(instance);
            }
        }

        protected override void OnInteract(object other)
        {
            if (!(other is Entity entity)) return;

            if (assignedDialog != null)
            {
                if (GUIWindowsManager.instance.dialogWindow == null)
                {
                    Debug.LogError("dialogWindow is NULL in GUIWindowsManager! Ensure it is assigned.");
                    return;
                }

                GUIWindowsManager.instance.dialogWindow.Show(entity, this, assignedDialog);
                return;
            }

            // Logowanie interakcji przy użyciu Collidera
            var interactionLogger = GetComponent<NpcInteractionLogger>();
            if (interactionLogger != null)
            {
                // Pobierz Collider gracza lub Agenta AI
                var collider = entity.GetComponent<Collider>();
                if (collider != null)
                {
                    interactionLogger.LogInteraction(collider);
                }
                else
                {
                    Debug.LogWarning("Collider for interacting entity is null. Cannot log interaction.");
                }
            }
            else
            {
                Debug.LogWarning("NpcInteractionLogger not found on QuestGiver. Cannot log interaction.");
            }
        }

        public void OpenQuestDialog()
        {
            var current = CurrentQuest();
            if (!current) return;

            if (Game.instance.quests.ContainsQuest(current))
                GUIWindowsManager.instance.quest.SetQuestFromLog(current);
            else
                GUIWindowsManager.instance.quest.SetQuest(current);

            Debug.Log("QuestGiver interacted with by player. Quest UI opened.");
        }

        public Dialog GetDialog()
        {
            var merchant = GetComponent<Merchant>();
            if (merchant != null && merchant.assignedDialog != null)
                return merchant.assignedDialog;

            var blacksmith = GetComponent<Blacksmith>();
            if (blacksmith != null && blacksmith.assignedDialog != null)
                return blacksmith.assignedDialog;

            return null;
        }

        /// <summary>
        /// Returns true if the QuestInstance matches any quest in the quests array.
        /// </summary>
        /// <param name="instance">The QuestInstance to check.</param>
        protected virtual bool MatchesQuest(QuestInstance instance)
        {
            return quests.Contains(instance.data) || additionalQuests.Contains(instance.data);
        }


        protected virtual void ChangeStateTo(State state)
        {
            this.state = state;
            onStateChange.Invoke(state);
        }

        protected virtual void OnQuestAdded(QuestInstance instance)
        {
            if (!MatchesQuest(instance))
                return;

            ChangeStateTo(State.QuestInProgress);
        }

        public virtual void OnQuestCompleted(QuestInstance instance)
        {
            if (!MatchesQuest(instance)) return;

            var quest = instance.data;

            bool isFetch = (!instance.IsMultiStage() && quest.IsFetchAfterKill()) ||
                        (instance.IsMultiStage() && instance.GetCurrentStage().completingMode == Quest.CompletingMode.FetchAfterKill);

            if (isFetch && (!instance.completed || quest.questType == QuestType.Specialization))
            {
                var fetchItem = instance.IsMultiStage()
                    ? instance.GetCurrentStage().requiredItem
                    : quest.requiredItem;

                var returnToNpc = instance.IsMultiStage()
                    ? instance.GetCurrentStage().returnToNPC
                    : quest.returnToNPC;

                QuestGiver returnNPC = FindReturnNPC(returnToNpc);

                if (returnNPC != null)
                {
                    returnNPC.ReceiveFetchItem(fetchItem);
                }
                else
                {
                    Debug.LogWarning($"Quest '{quest.title}' has no assigned NPC! Check returnToNPC.");
                }
            }

            if (quest.questType == QuestType.Specialization)
            {
                var player = Game.instance.currentCharacter;
                string currentClassName = player.Entity?.name?.Replace("(Clone)", "").Trim() ?? "";
                if (!ClassHierarchy.NameToBits.TryGetValue(currentClassName, out var currentClass))
                {
                    Debug.LogWarning($"[Specialization] Nieznana klasa gracza: {currentClassName}");
                }
                else
                {
                    int tierToUnlock = ClassHierarchy.GetTier(currentClass);
                    var specs = Game.instance?.currentCharacter?.specializations;
                    if (specs != null)
                    {
                        for (int t = 0; t <= tierToUnlock; t++)
                            specs.UnlockTierInstance(t);
                    }
                    GameSave.instance?.Save();
                    Debug.Log($"[Specialization] Unlocking tier {tierToUnlock} for character {player.name} ({currentClassName})");
                    GUIWindowsManager.Instance?.specializationsWindow?.GetComponent<GUIWindow>()?.Show();
                }
            }
            else if (quest.questType == QuestType.ClassUpgrade)
            {
                var player = Game.instance.currentCharacter;
                var oldEntity = player.Entity;

                string currentClassName = oldEntity?.name?.Replace("(Clone)", "").Trim() ?? "";

                if (!ClassHierarchy.NameToBits.TryGetValue(currentClassName, out var currentClass))
                {
                    Debug.LogWarning($"[ClassUpgrade] Nieznana klasa gracza: {currentClassName}");
                    return;
                }

                if ((quest.requiredClass & currentClass) == 0)
                {
                    Debug.LogWarning($"[ClassUpgrade] Gracz nie spełnia wymagań klasy: {currentClass}");
                    return;
                }

                var nextClass = ClassHierarchy.GetNextTierClass(currentClass);
                if (nextClass == CharacterClassRestrictions.None)
                {
                    Debug.LogWarning($"[ClassUpgrade] Brak dalszej klasy po: {currentClass}");
                    return;
                }

                string newClassName = nextClass.ToString();
                var newCharacter = GameDatabase.instance.characters.FirstOrDefault(c => c.name == newClassName);
                if (newCharacter == null)
                {
                    Debug.LogWarning($"[ClassUpgrade] Nie znaleziono prefabu dla klasy: {newClassName}");
                    return;
                }

                Vector3 oldPos = oldEntity.transform.position;
                Quaternion oldRot = oldEntity.transform.rotation;

                player.savedHealth = oldEntity.stats.health;
                player.savedMana = oldEntity.stats.mana;
                player.data = newCharacter;
                player.stats = new CharacterStats(newCharacter);

                var newEntity = player.Instantiate();
                newEntity.name = newClassName;

                newEntity.Teleport(oldPos, oldRot);

                newEntity.isPlayer = true;
                newEntity.controller.enabled = true;
                newEntity.stats.Initialize();
                newEntity.inputs.enabled = true;
                newEntity.skills.enabled = true;
                newEntity.items.enabled = true;
                newEntity.items.RevalidateEquippedItems();

                var feedback = newEntity.GetComponent<EntityFeedback>();
                feedback?.SendMessage("InitializeEntity", SendMessageOptions.DontRequireReceiver);

                newEntity.gameObject.tag = "Entity/Player";
                newEntity.gameObject.layer = LayerMask.NameToLayer("Entities");

                newEntity.stats.BulkUpdate(
                    level: 1,
                    strength: newCharacter.strength,
                    dexterity: newCharacter.dexterity,
                    vitality: newCharacter.vitality,
                    energy: newCharacter.energy,
                    availablePoints: 0,
                    experience: 0
                );

                if (newEntity.nametag != null)
                {
                    var cc = Game.instance.currentCharacter;
                    newEntity.nametag.SetNametag(
                        cc.name,
                        newEntity.stats.level,
                        cc.guildName,
                        cc.GetName()
                    );
                }

                player.RestoreSavedVitals();
                player.SetEntity(newEntity);
                Level.instance.SetPlayer(newEntity);

                FindFirstObjectByType<GUIStatsManager>()?.Refresh();

                newEntity.GetComponent<PlayerInitializer>()?.Initialize();

                var specs = Game.instance.currentCharacter?.specializations;
                specs?.ClearUnlockedTiersInstance();
                GUIWindowsManager.Instance?.specializationsWindow?.GetComponent<GUIWindow>()?.Hide();
            }

            ChangeStateTo(CurrentQuest() ? State.QuestAvailable : State.None);
        }

        protected virtual void OnQuestRemoved(QuestInstance instance)
        {
            if (!MatchesQuest(instance))
                return;

            ChangeStateTo(State.QuestAvailable);
        }

        private void OnEnable()
        {
            QuestGiverRegistry.Instance?.Register(this);
        }

        private void OnDisable()
        {
            QuestGiverRegistry.Instance?.Unregister(this);
        }
    }
}
