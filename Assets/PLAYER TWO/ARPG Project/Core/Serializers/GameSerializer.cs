using UnityEngine;
using System.Collections.Generic;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [System.Serializable]
    public class GameSerializer
    {
        public List<CharacterSerializer> characters = new List<CharacterSerializer>();
        public InventorySerializer[] stashes;

        // Globalne mnożniki trudności
        public float dexterityMultiplier = 1.0f;
        public float strengthMultiplier = 1.0f;
        public float speedMultiplier = 1.0f;

        public GameSerializer(Game game)
        {
            // Pobierz aktualny stan DifficultyManager
            if (DifficultyManager.Instance != null)
            {
                dexterityMultiplier = DifficultyManager.Instance.CurrentDexterityMultiplier;
                strengthMultiplier = DifficultyManager.Instance.CurrentStrengthMultiplier;
                speedMultiplier = DifficultyManager.Instance.CurrentSpeedMultiplier;

                Debug.Log($"Saving DifficultyManager: Dexterity={dexterityMultiplier}, Strength={strengthMultiplier}, Speed={speedMultiplier}");
            }

            InitializeCharacters(game.characters);
            InitializeStashes(game.stash);
        }

        protected virtual void InitializeCharacters(List<CharacterInstance> characters)
        {
            foreach (var character in characters)
            {
                this.characters.Add(new CharacterSerializer(character));
            }
        }

        protected virtual void InitializeStashes(GameStash stash)
        {
            stashes = new InventorySerializer[stash.amount];

            for (int i = 0; i < stashes.Length; i++)
            {
                stashes[i] = new InventorySerializer(stash.GetInventory(i));
            }
        }

        /// <summary>
        /// Po deserializacji przywróć stan `DifficultyManager`.
        /// </summary>
        public void ApplyDifficultySettings()
        {
            if (DifficultyManager.Instance != null)
            {
                DifficultyManager.Instance.CurrentDexterityMultiplier = dexterityMultiplier;
                DifficultyManager.Instance.CurrentStrengthMultiplier = strengthMultiplier;
                DifficultyManager.Instance.CurrentSpeedMultiplier = speedMultiplier;

                Debug.Log($"Loaded DifficultyManager: Dexterity={dexterityMultiplier}, Strength={strengthMultiplier}, Speed={speedMultiplier}");
            }
            else
            {
                Debug.LogWarning("DifficultyManager.Instance is null during deserialization!");
            }
        }

        public virtual string ToJson() => JsonUtility.ToJson(this);

        public static GameSerializer FromJson(string json) =>
            JsonUtility.FromJson<GameSerializer>(json);
    }
}
