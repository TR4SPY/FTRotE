using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/UI/UI Character Selection")]
    public class UICharacterSelection : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Determines if the first character on the list should be selected on start.")]
        public bool selectFirstOnStart = true;

        [Header("Character References")]
        [Tooltip("A reference to the UI Character Window to display the Character's data.")]
        public UICharacterWindow characterWindow;

        [Tooltip("The prefab to use to represent each character on the list.")]
        public UICharacterButton characterSlot;

        [Tooltip("A reference to the UI Character Form component.")]
        public UICharacterForm characterForm;

        [Tooltip("A reference to the Game Object that contains the character's list.")]
        public GameObject charactersWindow;

        [Tooltip("A reference to the Game Object that contains the actions to manage characters.")]
        public GameObject characterActions;

        [Tooltip("A reference to the transform used as container for placing all characters.")]
        public Transform charactersContainer;

        [Header("Button References")]
        [Tooltip("A reference to the Button component that opens the character creation window.")]
        public Button newCharacterButton;

        [Tooltip("A reference to the Button component that starts the Game.")]
        public Button startGameButton;

        [Tooltip("A reference to the Button component that deletes the current selected character.")]
        public Button deleteCharacterButton;

        [Header("Audio References")]
        [Tooltip("The Audio Clip that plays when selecting a character.")]
        public AudioClip selectCharacterAudio;

        [Tooltip("The Audio Clip that plays when deleting a character.")]
        public AudioClip deleteCharacterAudio;

        [Space(15)]
        public UnityEvent<CharacterInstance> onCharacterSelected;

        protected CanvasGroup m_charactersWindowGroup;
        protected CanvasGroup m_characterActionsGroup;

        protected int m_currentCharacterId = -1;
        protected bool m_creatingCharacter;

        protected List<UICharacterButton> m_characters = new();

        protected GameAudio m_audio => GameAudio.instance;

        protected virtual void InitializeGroups()
        {
            if (!charactersWindow.TryGetComponent(out m_charactersWindowGroup))
                m_charactersWindowGroup = charactersWindow.AddComponent<CanvasGroup>();

            if (!characterActions.TryGetComponent(out m_characterActionsGroup))
                m_characterActionsGroup = characterActions.AddComponent<CanvasGroup>();
        }

        protected virtual void InitializeCallbacks()
        {
            newCharacterButton.onClick.AddListener(ToggleCharacterCreation);
            characterForm.onSubmit.AddListener(ToggleCharacterCreation);
            characterForm.onCancel.AddListener(ToggleCharacterCreation);
            startGameButton.onClick.AddListener(StartGame);

            deleteCharacterButton.onClick.AddListener(() =>
            {
                m_audio.PlayUiEffect(deleteCharacterAudio);
                DeleteSelectedCharacter();
            });

            Game.instance.onCharacterAdded.AddListener(AddCharacter);
            Game.instance.onDataLoaded.AddListener(RefreshList);
            Game.instance.onCharacterDeleted.AddListener(() =>
            {
                var nextToSelect = m_currentCharacterId - 1;
                m_currentCharacterId = -1;
                RefreshList();

                if (nextToSelect >= 0)
                    SelectCharacter(nextToSelect);
            });
        }

        public virtual void ToggleCharacterCreation()
        {
            m_creatingCharacter = !m_creatingCharacter;
            m_charactersWindowGroup.alpha = m_characterActionsGroup.alpha = m_creatingCharacter ? 0.5f : 1;
            m_charactersWindowGroup.blocksRaycasts = m_characterActionsGroup.blocksRaycasts = !m_creatingCharacter;
            characterForm.gameObject.SetActive(m_creatingCharacter);
        }

        public virtual void StartGame()
        {
            if (m_currentCharacterId >= 0 && m_currentCharacterId < m_characters.Count)
            {
                var character = m_characters[m_currentCharacterId].character;

                GameSave.instance.LoadDifficultyForCharacter(character);

                Debug.Log($"Difficulty loaded for character: {character.name}");
            }

            characterActions.SetActive(false);
            Game.instance.StartGame(m_currentCharacterId);
        }


        public virtual void AddCharacter(int characterId)
        {
            m_currentCharacterId = -1;
            RefreshList();
            SelectCharacter(characterId);
        }

        public virtual void DeleteSelectedCharacter()
        {
            if (m_currentCharacterId < 0 || m_currentCharacterId >= m_characters.Count)
                return;

            characterWindow.gameObject.SetActive(false);
            characterActions.gameObject.SetActive(false);
            Game.instance.DeleteCharacter(m_currentCharacterId);
        }

        public virtual void SelectCharacter(int characterId)
        {
            if (characterId < 0 || characterId >= m_characters.Count)
                return;

            m_currentCharacterId = characterId;
            m_characters[m_currentCharacterId].SetInteractable(false);
            characterWindow.gameObject.SetActive(true);
            characterActions.gameObject.SetActive(true);
            characterWindow.UpdateTexts(m_characters[characterId].character);

            // Wywołaj zdarzenie zmiany postaci
            onCharacterSelected.Invoke(m_characters[characterId].character);

            var selectedCharacter = Game.instance.characters[characterId];

            // Wczytaj dane trudności z GameSave
            GameSave.instance.LoadDifficultyForCharacter(selectedCharacter);

            Debug.Log($"Difficulty loaded for selected character: {selectedCharacter.name}");
        }

        public virtual void SelectFirstCharacter()
        {
            if (m_characters.Count > 0)
                SelectCharacter(0);
        }

        public virtual void RefreshList()
        {
            var characters = Game.instance.characters;

            foreach (var character in m_characters)
            {
                character.gameObject.SetActive(false);
            }

            for (int i = 0; i < characters.Count; i++)
            {
                if (m_characters.Count < i + 1)
                {
                    var index = i;

                    m_characters.Add(Instantiate(characterSlot, charactersContainer));
                    m_characters[i].onSelect.AddListener((character) =>
                    {
                        if (m_currentCharacterId != index && m_currentCharacterId >= 0 &&
                            m_currentCharacterId < m_characters.Count)
                            m_characters[m_currentCharacterId]?.SetInteractable(true);

                        m_audio.PlayUiEffect(selectCharacterAudio);
                        SelectCharacter(index);
                    });
                }

                m_characters[i].SetCharacter(characters[i]);
                m_characters[i].gameObject.SetActive(true);
                m_characters[i].SetInteractable(true);
            }
        }

        protected virtual void Start()
        {
            Game.instance.ReloadGameData();
            InitializeGroups();
            InitializeCallbacks();
            RefreshList();

            if (selectFirstOnStart && m_characters.Count > 0)
            {
                SelectFirstCharacter();
            }
        }
    }
}
