using UnityEngine;
using AI_DDA.Assets.Scripts;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    public class PlayerInitializer : MonoBehaviour
    {
        public GameObject nameplatePrefab;
        private Nametag nametagInstance;

        private bool m_initialized = false;

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (m_initialized) return;
            m_initialized = true;

            GameObject go = Instantiate(nameplatePrefab);
            nametagInstance = go.GetComponent<Nametag>();
            nametagInstance.target = this.transform;

            var entity = GetComponent<Entity>();
            if (entity != null)
            {
                entity.nametag = nametagInstance;
            }

            var character = Game.instance?.currentCharacter;
            if (character != null)
            {
                string playerName = character.name;
                int playerLevel = character.stats.currentLevel;
                string classDisplay = character.GetName();
                string guild = character.guildName;

                nametagInstance.SetNametag(playerName, playerLevel, guild, classDisplay);
            }
            else
            {
                Debug.LogWarning("PlayerInitializer: Missing currentCharacter in Game.instance");
            }

            if (entity?.stats != null && nametagInstance != null)
            {
                entity.stats.onLevelUp.AddListener(() =>
                {
                    var currentCharacter = Game.instance?.currentCharacter;
                    if (currentCharacter != null)
                    {
                        string charName = currentCharacter.name;
                        int level = entity.stats.level;
                        string classDisplay = currentCharacter.GetName();
                        string guild = currentCharacter.guildName;

                        nametagInstance.SetNametag(charName, level, guild, classDisplay);
                    }
                });
            }

            var difficultyManager = Object.FindFirstObjectByType<DifficultyManager>();
            if (difficultyManager != null)
            {
                difficultyManager.playerLogger = GetComponent<PlayerBehaviorLogger>();
            }
            else
            {
                Debug.LogError("DifficultyManager not found in the scene!");
            }

            var rlModel = Object.FindFirstObjectByType<RLModel>();
            if (rlModel != null)
            {
                rlModel.SetPlayerLogger(GetComponent<PlayerBehaviorLogger>());
            }
            else
            {
                Debug.LogError("[AI-DDA] RLModel not found in the scene!");
            }

            var playerLogger = GetComponent<PlayerBehaviorLogger>();
            if (playerLogger != null)
            {
                var gameSave = Object.FindFirstObjectByType<GameSave>();
                if (gameSave != null)
                {
                    var currentCharacter = Game.instance?.currentCharacter;
                    if (currentCharacter != null)
                    {
                        gameSave.LoadLogsForCharacter(currentCharacter);
                    }
                    else
                    {
                        Debug.LogWarning("No current character found in Game.instance.");
                    }
                }
                else
                {
                    Debug.LogError("GameSave not found in the scene!");
                }
            }
            else
            {
                Debug.LogError("PlayerBehaviorLogger not found on the player entity!");
            }

            var chatManager = Object.FindFirstObjectByType<ChatManager>();
            if (chatManager != null)
            {
                chatManager.Reinitialize();
            }
            else
            {
                Debug.LogWarning("[ChatManager] ChatManager not found during PlayerInitializer.");
            }
        }
    }
}
