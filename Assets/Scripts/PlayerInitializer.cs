using UnityEngine;
using AI_DDA.Assets.Scripts;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    public class PlayerInitializer : MonoBehaviour
    {
        private void Start()
        {
            var difficultyManager = Object.FindFirstObjectByType<DifficultyManager>();
            if (difficultyManager != null)
            {
                difficultyManager.playerLogger = GetComponent<PlayerBehaviorLogger>();
                Debug.Log("PlayerBehaviorLogger assigned to DifficultyManager dynamically.");
            }
            else
            {
                Debug.LogError("DifficultyManager not found in the scene!");
            }
            
            var rlModel = Object.FindFirstObjectByType<RLModel>();
            if (rlModel != null)
            {
                rlModel.SetPlayerLogger(GetComponent<PlayerBehaviorLogger>());
                Debug.Log("[AI-DDA] PlayerBehaviorLogger assigned to RLModel dynamically.");
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
                        Debug.Log($"Logs loaded for character: {currentCharacter.name}");
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
        }
    }
}
