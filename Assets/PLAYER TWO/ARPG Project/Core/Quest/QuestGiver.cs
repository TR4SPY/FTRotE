//  ZMODYFIKOWANO 31 GRUDNIA 2024

using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using AI_DDA.Assets.Scripts;

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
        }

        protected virtual void InitializeCallbacks()
        {
            m_manager.onQuestAdded += OnQuestAdded;
            m_manager.onQuestCompleted += OnQuestCompleted;
            m_manager.onQuestRemoved += OnQuestRemoved;
        }

        protected virtual void InitializeStates()
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
            var isAchiever = (Game.instance.currentCharacter.currentDynamicPlayerType == "Achiever");

            foreach (var quest in isAchiever ? additionalQuests.Concat(quests) : quests)
            {
                if (!m_manager.TryGetQuest(quest, out var instance) || !instance.completed)
                    return quest;
            }

            return null;
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
            if (Game.instance.currentCharacter.inventory.HasItem(item.data))
            {
                if (Game.instance.currentCharacter.inventory.RemoveItem(item.data))
                {
                    Debug.Log($"Item {item.data.name} returned to {name}. Quest completed!");
                    Game.instance.quests.RemoveQuest(CurrentQuest());
                    ChangeStateTo(State.None);
                }
                else
                {
                    Debug.LogWarning($"Failed to remove item {item.data.name} from inventory.");
                }
            }
            else
            {
                Debug.Log($"Player does not have {item.data.name} yet.");
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

                GUIWindowsManager.instance.quest.SetQuest(current);
                Debug.Log("QuestGiver interacted with by player. Quest UI opened.");
        }

        /// <summary>
        /// Returns true if the QuestInstance matches any quest in the quests array.
        /// </summary>
        /// <param name="instance">The QuestInstance to check.</param>
        protected virtual bool MatchesQuest(QuestInstance instance)
        {
            if (quests.Length == 0 || !quests.Contains(instance.data))
                return false;

            return true;
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

        protected virtual void OnQuestCompleted(QuestInstance instance)
        {
            if (!MatchesQuest(instance)) return;

            var quest = instance.data;

            if (quest.IsFetchAfterKill())
            {
                QuestGiver returnNPC = FindReturnNPC(quest.returnToNPC);

                if (returnNPC != null)
                {
                    Debug.Log($"Quest '{quest.title}' requires the player to return {quest.requiredItem.data.name} to NPC {returnNPC.name}.");
                    returnNPC.ReceiveFetchItem(quest.requiredItem);
                }
                else
                {
                    Debug.LogWarning($"Quest '{quest.title}' has no assigned NPC! Check returnToNPC.");
                }
                return;
            }

            ChangeStateTo(CurrentQuest() ? State.QuestAvailable : State.None);
        }

        protected virtual void OnQuestRemoved(QuestInstance instance)
        {
            if (!MatchesQuest(instance))
                return;

            ChangeStateTo(State.QuestAvailable);
        }
    }
}
