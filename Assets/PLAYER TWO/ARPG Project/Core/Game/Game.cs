using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Game/Game")]
    public class Game : Singleton<Game>
    {
        [Header("Version Info")]
        public string gameName = "Forsaken Tribe: Rise of the Exiled";
        public string version = "0.05C";
        public string buildDate = "050825";

        [Header("Game Settings")]
        [Tooltip("The base amount of experience necessary to level up")]
        public int baseExperience = 1973;

/*
        [Tooltip("The amount of additional experience the Player will need to reach the next level")]
        public int experiencePerLevel = 179;
*/
        
        [Tooltip("The base amount of experience earned from defeating an enemy")]
        public int baseEnemyDefeatExperience = 679;

        [Tooltip("Multiplier applied to experience required per level")]
        public float expMultiplier = 1.5f;

        [Tooltip("The amount of point(s) added to the distribution points after raising a level")]
        public int levelUpPoints = 5;

        [Tooltip("The maximum level the Player can reach")]
        public int maxLevel = 500;

        [Tooltip(
            "The rate at which the amount of money looted from "
                + "enemies increases per enemy level. For example, a "
                + "value of 0.15 means a 15% increase per level."
        )]
        public float enemyLootMoneyIncreaseRate = 0.15f;

        [Header("Specialization Settings")]
        [Tooltip("Player level required to unlock tier 1 specializations")]
        public int tier1UnlockLevel = 0;
        [Tooltip("Player level required to unlock tier 2 specializations")]
        public int tier2UnlockLevel = 0;
        [Tooltip("Player level required to unlock tier 3 specializations")]
        public int tier3UnlockLevel = 0;
        [Tooltip("The cost required to reset a specialization")]
        public int specializationRespecCost = 0;

        [Header("Combat Settings")]
        [Tooltip("The damage multiplier applied to critical hits.")]
        public float criticalMultiplier = 1.25f;

        [Tooltip("The maximum attack speed the stats can reach.")]
        public int maxAttackSpeed = 1000;

        [Tooltip("The maximum chance of an entity to block an attack.")]
        public float maxBlockChance = 0.75f;

        [Tooltip("The maximum block speed (block recover) the stats can reach.")]
        public int maxBlockSpeed = 1000;

        [Tooltip("The maximum chance of an entity to stun another.")]
        public float maxStunChance = 0.75f;

        [Tooltip("The maximum stun speed (stun recover) the stats can reach.")]
        public int maxStunSpeed = 1000;

        [Header("Item Attributes Settings")]
        [Tooltip("The maximum chance of an item have additional attributes")]
        public float maxAttributeChance = 0.8f;

        [Tooltip("The minimum chance of an item have additional attributes")]
        public float minAttributeChance = 0.1f;

        [Tooltip("The additional price an item will have per attribute")]
        public int pricePerAttribute = 500;

        [Header("Collectibles Prefabs")]
        public CollectibleItem collectibleItemPrefab;
        public CollectibleMoney collectibleMoneyPrefab;

        [Header("Input Actions references")]
        public InputActionAsset gameplayActions;
        public InputActionAsset guiActions;

        [Space(15)]
        public UnityEvent<int> onCharacterAdded;
        public UnityEvent onCharacterDeleted;
        public UnityEvent onDataLoaded;

        protected GameStash m_stash;
        protected bool m_gameLoaded;
        protected int m_currentCharacterId = -1;

        public int lastNPCID = 0;
        private Dictionary<GameObject, int> npcIDs = new Dictionary<GameObject, int>();
        private Dictionary<string, string> npcIDMap = new Dictionary<string, string>();

        public List<CharacterInstance> characters { get; protected set; } =
            new List<CharacterInstance>();

        /// <summary>
        /// Returns the current Character Instance of this Game session.
        /// </summary>
        private bool alreadyLoaded = false;

        public CharacterInstance currentCharacter
        {
            get
            {
                if (!alreadyLoaded)
                {
                    alreadyLoaded = true;
                    LoadGameData();
                }

                if (m_currentCharacterId < 0 || m_currentCharacterId >= characters.Count)
                {
                    if (characters.Count == 0)
                        CreateCharacter("Player", 0);

                    m_currentCharacterId = 0;
                }

                return characters[m_currentCharacterId];
            }
        }

        public QuestsManager quests => currentCharacter.quests.manager;

        public GameStash stash
        {
            get
            {
                if (!m_stash)
                    m_stash = GetComponent<GameStash>();

                return m_stash;
            }
        }

        /// <summary>
        /// Starts a new Game session with a given Character Instance.
        /// </summary>
        /// <param name="character">The Character Instance you want to start a new session with.</param>
        public virtual void StartGame(int index)
        {
            if (characters.Count == 0)
                return;

            m_currentCharacterId = index;

            if (currentCharacter.currentScene != null)
                GameScenes.instance.LoadScene(currentCharacter.currentScene);
                GUIWindowsManager.Instance?.ResetWindowsState();
        }

        /// <summary>
        /// Closes the game application.
        /// </summary>
        public virtual void ExitGame()
        {
#if UNITY_EDITOR
            Fader.instance.FadeOut(() => UnityEditor.EditorApplication.isPlaying = false);
#elif UNITY_STANDALONE
            Fader.instance.FadeOut(() => Application.Quit());
#endif
        }

        /// <summary>
        /// Creates a new Character Instance with a given name.
        /// </summary>
        /// <param name="name">The name of the Character Instance to create.</param>
        /// <param name="classId">The index of the character data.</param>
        public virtual void CreateCharacter(string name, int classId)
        {
            var characterType = GameDatabase.instance.FindElementById<Character>(classId);
            var character = new CharacterInstance(characterType, name);
            characters.Add(character);
            m_currentCharacterId = characters.Count - 1;
            onCharacterAdded?.Invoke(m_currentCharacterId);
        }

        /// <summary>
        /// Deletes a Character Instance from the list of available characters.
        /// </summary>
        /// <param name="character">The Character Instance you want to delete.</param>
        public virtual void DeleteCharacter(int characterId)
        {
            if (characterId < 0 || characterId >= characters.Count)
                return;

            characters.RemoveAt(characterId);
            onCharacterDeleted?.Invoke();
        }

        public string GetNPCIDForName(string npcName)
        {
            if (!npcIDMap.ContainsKey(npcName))
            {
                npcIDMap[npcName] = npcIDMap.Count.ToString(); // Numerowanie od 0, można zacząć od 1
            }

            return npcIDMap[npcName];
        }

        public int GetNextNPCID()
        {
            lastNPCID++;
            return lastNPCID;
        }

        public bool CanSelectTier(int tier, int currentLevel)
        {
            switch (tier)
            {
                case 1:
                    return currentLevel >= tier1UnlockLevel;
                case 2:
                    return currentLevel >= tier2UnlockLevel;
                case 3:
                    return currentLevel >= tier3UnlockLevel;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Loads the Game data from the memory.
        /// </summary>

        public virtual void LoadGameData()
        {
            if (m_gameLoaded)
                return;

            // Ensure the GameDatabase is initialized before loading character data.
            // Without this the Specializations lookup dictionary might not be
            // populated yet, causing CharacterSpecializations to miss their
            // references when deserializing.  Accessing the singleton forces its
            // Awake() to run which registers all Specializations.
            var _ = GameDatabase.instance;
            var data = GameSave.instance.Load();

            // Debug.Log($"[Game] Loaded {data?.characters.Count ?? 0} characters from save."); //  DEBUG - In case of issues with currentCharacter ID

            m_gameLoaded = true;
            
            if (data == null)
                return;

            characters = data
                .characters.Select(c => CharacterInstance.CreateFromSerializer(c))
                .ToList();

            stash.LoadData(data.stashes);
            onDataLoaded?.Invoke();
        }

        /// <summary>
        /// Reloads the Game data from the memory.
        /// </summary>
        public virtual void ReloadGameData()
        {
            m_gameLoaded = false;
            LoadGameData();
        }

        protected override void Initialize()
        {
            LoadGameData();
            DontDestroyOnLoad(gameObject);
        }

        protected virtual void OnApplicationFocus(bool hasFocus)
        {

        }

        protected virtual void OnApplicationQuit() => GameSave.instance.Save();
    }
}
