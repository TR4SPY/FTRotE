using UnityEngine;
using AI_DDA.Assets.Scripts;

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
    }
}
