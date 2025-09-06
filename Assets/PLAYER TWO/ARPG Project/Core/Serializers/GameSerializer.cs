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
        public BankAccountSerializer[] bankAccounts;

        public float dexterityMultiplier = 1.0f;
        public float strengthMultiplier = 1.0f;
        public float vitalityMultiplier = 1.0f;
        public float energyMultiplier = 1.0f;

        public GameSerializer(Game game)
        {
            if (DifficultyManager.Instance != null)
            {
                dexterityMultiplier = DifficultyManager.Instance.CurrentDexterityMultiplier;
                strengthMultiplier = DifficultyManager.Instance.CurrentStrengthMultiplier;
                vitalityMultiplier = DifficultyManager.Instance.CurrentVitalityMultiplier;
                energyMultiplier = DifficultyManager.Instance.CurrentEnergyMultiplier;

                Debug.Log($"Saving DifficultyManager: Dexterity={dexterityMultiplier}, Strength={strengthMultiplier}, Vitality={vitalityMultiplier}, Energy={energyMultiplier}");
            }

            InitializeCharacters(game.characters);
            InitializeStashes(game.stash);
            InitializeBank(game.bank);
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
        
        protected virtual void InitializeBank(GameBank bank)
        {
            bankAccounts = new BankAccountSerializer[bank.amount];

            for (int i = 0; i < bankAccounts.Length; i++)
            {
                bankAccounts[i] = new BankAccountSerializer(bank.GetAccount(i));
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
                DifficultyManager.Instance.CurrentVitalityMultiplier = vitalityMultiplier;
                DifficultyManager.Instance.CurrentEnergyMultiplier = energyMultiplier;

                Debug.Log($"Loaded DifficultyManager: Dexterity={dexterityMultiplier}, Strength={strengthMultiplier}, Vitality={vitalityMultiplier}, Energy={energyMultiplier}");
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
