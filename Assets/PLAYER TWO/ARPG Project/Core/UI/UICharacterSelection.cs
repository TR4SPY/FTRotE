using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using AI_DDA.Assets.Scripts;
using System.Linq;

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

        public Transform characterCenterPoint; // Punkt centralny półokręgu
        public GameObject characterInfoUIPrefab; // Prefab UI dla informacji o postaci
        float characterRadius = 5f; // Promień półokręgu
        float characterAngleStep = 20f; // Kąt między postaciami
        private Transform cameraTransform;
        private List<GameObject> activeCharacterGUIs = new List<GameObject>();
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

              //  GameSave.instance.LoadLogsForCharacter(character);
              //  Debug.Log($"Logs loaded for character: {character.name}");
            }

            characterActions.SetActive(false);
            Game.instance.StartGame(m_currentCharacterId);
        }
        private void ClearAttachmentPoints(Transform characterObject)
        {
            foreach (var attachmentPointName in new[] { "RightHand", "LeftHand", "Head", "Chest", "Pants", "Gloves", "Boots" })
            {
                var attachmentPoint = characterObject.Find(attachmentPointName);
                if (attachmentPoint != null)
                {
                    foreach (Transform child in attachmentPoint)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }
        private Transform FindAttachmentPoint(GameObject characterObject, string pointName)
        {
            Transform[] allChildren = characterObject.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                if (child.name == pointName)
                {
                    // Debug.Log($"Attachment point '{pointName}' found at: {child.name}");
                    return child;
                }
            }

            Debug.LogWarning($"Attachment point '{pointName}' not found on prefab '{characterObject.name}'");
            return null;
        }

        private void AttachEquipment(CharacterInstance characterInstance, GameObject characterObject)
        {
            // Pobierz wyposażenie postaci jako słownik {slot: ItemInstance}
            var equippedItems = characterInstance.equipments.GetEquippedItems();

            // Pobierz Entity Item Manager
            var itemManager = characterObject.GetComponent<EntityItemManager>();
            if (itemManager == null)
            {
                Debug.LogError("EntityItemManager not found on characterObject!");
                return;
            }

            // Iteruj przez każde wyposażenie
            foreach (var itemSlot in equippedItems.Keys)
            {
                var itemInstance = equippedItems[itemSlot];

                // Obsługa tarczy
                if (itemInstance?.data is ItemShield shield)
                {
                    if (itemManager.leftHandShieldSlot != null)
                    {
                        shield.Instantiate(itemManager.leftHandShieldSlot);
                        // Debug.Log($"Attached shield: {shield.name} to {itemManager.leftHandShieldSlot.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"Left hand shield slot not found for shield: {shield.name}");
                    }
                }
                // Obsługa miecza jednoręcznego
                else if (itemInstance?.data is ItemBlade blade)
                {
                    Transform attachmentPoint = null;

                    // Wybierz odpowiedni slot w zależności od miejsca przeznaczenia
                    if (itemSlot == "RightHand" && itemManager.rightHandSlot != null)
                    {
                        attachmentPoint = itemManager.rightHandSlot;
                    }
                    else if (itemSlot == "LeftHand" && itemManager.leftHandSlot != null)
                    {
                        attachmentPoint = itemManager.leftHandSlot;
                    }

                    if (attachmentPoint != null)
                    {
                        blade.InstantiateRightHand(attachmentPoint);
                        // Debug.Log($"Attached blade: {blade.name} to {attachmentPoint.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"No attachment point found for blade: {blade.name} in slot: {itemSlot}");
                    }
                }
                // Obsługa łuku
                else if (itemInstance?.data is ItemBow bow)
                {
                    if (itemManager.rightHandSlot != null && itemManager.leftHandSlot != null)
                    {
                        bow.Instantiate(itemManager.rightHandSlot, itemManager.leftHandSlot);
                        // Debug.Log($"Attached bow: {bow.name} using {itemManager.rightHandSlot.name} and {itemManager.leftHandSlot.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"Bow slots not found for bow: {bow.name}");
                    }
                }
                // Obsługa zbroi
                else if (itemInstance?.data is ItemArmor armor)
                {
                    SkinnedMeshRenderer targetRenderer = null;

                    // Dopasowanie rendererów na podstawie slotu
                    switch (itemSlot)
                    {
                        case "Chest":
                            targetRenderer = itemManager.chestRenderer;
                            break;
                        case "Pants":
                            targetRenderer = itemManager.pantsRenderer;
                            break;
                        case "Gloves":
                            targetRenderer = itemManager.glovesRenderer;
                            break;
                        case "Boots":
                            targetRenderer = itemManager.bootsRenderer;
                            break;
                        case "Head":
                            targetRenderer = itemManager.helmRenderer;
                            break;
                    }

                    if (targetRenderer != null)
                    {
                        // Przypisz mesh i materiały
                        targetRenderer.sharedMesh = armor.mesh;
                        targetRenderer.materials = armor.materials;
                        // Debug.Log($"Assigned armor: {armor.name} to renderer: {targetRenderer.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"No SkinnedMeshRenderer found for slot '{itemSlot}'");
                    }
                }
                // Ogólny przypadek
                else if (itemInstance?.data != null)
                {
                    Debug.LogWarning($"Unhandled item type for slot '{itemSlot}': {itemInstance.data.name}");
                }
            }
        }

        public void ArrangeCharactersInSemiCircle(List<CharacterInstance> characters, Transform centerPoint, float radius, Transform cameraTransform, float angleStep = 15f)
        {
            int count = characters.Count;

            // Oblicz początkowy kąt, aby symetrycznie rozmieścić postacie
            float totalAngle = angleStep * (count - 1); // Łączny zakres kątów
            float startAngle = -totalAngle / 2f; // Początek łuku

            for (int i = 0; i < count; i++)
            {
                // Obliczenie pozycji i kąta dla każdej postaci
                float angle = startAngle + angleStep * i; // Kąt w stopniach
                float radians = angle * Mathf.Deg2Rad; // Zamiana kąta na radiany

                // Ustawienie pozycji postaci
                Vector3 position = new Vector3(
                    centerPoint.position.x + radius * Mathf.Cos(radians),
                    centerPoint.position.y,
                    centerPoint.position.z - radius * Mathf.Sin(radians) // Znak "-" odwraca kierunek
                );

                // Pobierz prefab postaci i ustaw pozycję oraz rotację
                var characterInstance = characters[i];
                if (characterInstance.data.classPrefab == null)
                {
                    Debug.LogError($"Brak prefab w characterInstance {characterInstance.name}!");
                    continue; 
                }

                if (characterInstance.data.classPrefab != null)
                {
                    GameObject characterObject = Instantiate(characterInstance.data.classPrefab, position, Quaternion.identity);
                    characterObject.transform.SetParent(centerPoint); // Przypisz jako dziecko do punktu centralnego

                    // Wyłącz rotację lokalną (jeśli prefab ma niechciane komponenty)
                    characterObject.transform.localRotation = Quaternion.identity;

                    // Ustaw rotację postaci w stronę kamery
                    Vector3 directionToCamera = cameraTransform.position - characterObject.transform.position;
                    directionToCamera.y = 0; // Ignoruj różnice w wysokości
                    characterObject.transform.rotation = Quaternion.LookRotation(directionToCamera);

                    // Obrót o 180 stopni (jeśli prefab jest zwrócony tyłem)
                    //characterObject.transform.rotation = Quaternion.Euler(0, characterObject.transform.rotation.eulerAngles.y + 180, 0);

                    // Dodaj wyposażenie do postaci
                    AttachEquipment(characterInstance, characterObject);

                    // Wywołaj DisplayCharacterInfo
                    DisplayCharacterInfo(characterObject, characterInstance);
                }
            }
        }

        private void DisplayCharacterInfo(GameObject characterObject, CharacterInstance characterInstance)
        {
            if (characterObject == null || characterInstance == null)
            {
                Debug.LogWarning("Character object or instance is null. Skipping display.");
                return;
            }

            if (characterInfoUIPrefab == null)
            {
                Debug.LogError("Brak prefab UI dla informacji o postaci!");
                return;
            }

            // Sprawdź, czy GUI już istnieje dla tej postaci
            var existingGUI = activeCharacterGUIs.FirstOrDefault(gui => 
                gui != null && gui.GetComponent<GUICharacterInfo>()?.m_target == characterObject.transform);

            if (existingGUI != null)
            {
                // Debug.Log($"GUI already exists for character {characterObject.name}. Skipping creation.");
                return;
            }

            var uiInstance = Instantiate(characterInfoUIPrefab, Vector3.zero, Quaternion.identity);

            var mainCanvas = Object.FindFirstObjectByType<Canvas>();
            if (mainCanvas == null)
            {
                Debug.LogError("Main Canvas not found! Ensure there is a Canvas in the scene.");
                Destroy(uiInstance);
                return;
            }

            uiInstance.transform.SetParent(mainCanvas.transform, false);
            uiInstance.transform.SetAsFirstSibling();
            uiInstance.transform.localScale = Vector3.one;

            var guiCharacterInfo = uiInstance.GetComponent<GUICharacterInfo>();
            if (guiCharacterInfo == null)
            {
                Debug.LogWarning("Prefab is missing GUICharacterInfo component. Destroying UI instance.");
                Destroy(uiInstance);
                return;
            }

            // Ustaw informacje o postaci
            var playerName = characterInstance.name;
            var classFullName = characterInstance.data.classPrefab.name;
            var characterClass = classFullName.Replace(" Class", "");
            var level = characterInstance.stats.currentLevel;

            guiCharacterInfo.SetCharacterInfo(
                characterObject.transform,
                $"{playerName}\n{characterClass}\nLevel {level}",
                characterClass,
                level
            );

            // Dodaj instancję do listy aktywnych GUI
            activeCharacterGUIs.Add(uiInstance);
        }

        private void RemoveCharacterGUI(GameObject characterObject)
        {
            var guiToRemove = activeCharacterGUIs.FirstOrDefault(gui => 
                gui != null && gui.GetComponent<GUICharacterInfo>()?.m_target == characterObject.transform);

            if (guiToRemove != null)
            {
                activeCharacterGUIs.Remove(guiToRemove);
                Destroy(guiToRemove);
            }
        }

        public void RefreshCharacterDisplay()
        {
            // Debug.Log("RefreshCharacterDisplay called.");

            var characters = Game.instance.characters;

            // Najpierw usuń powiązane GUI
            foreach (Transform child in characterCenterPoint)
            {
                RemoveCharacterGUI(child.gameObject);
            }
            // Teraz usuń obiekty postaci
            foreach (Transform child in characterCenterPoint)
            {
                Destroy(child.gameObject);
            }

            // Dopiero potem generuj od nowa
            ArrangeCharactersInSemiCircle(characters, characterCenterPoint, characterRadius, cameraTransform, characterAngleStep);
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

            // Sprawdź liczbę postaci po usunięciu
            var characters = Game.instance.characters;
            newCharacterButton.interactable = characters.Count < 5;

            // Odśwież listę postaci
            RefreshList();
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

            // Wczytaj logi gracza z GameSave
           //     if (PlayerBehaviorLogger.Instance != null)
           //     {
           //         GameSave.instance.LoadLogsForCharacter(selectedCharacter);
           //         Debug.Log($"Logs loaded for selected character: {selectedCharacter.name}");
           //     }
           //     else
           //     {
           //         Debug.LogError("PlayerBehaviorLogger.Instance is null. Make sure the player prefab is instantiated.");
           //     }
        }

        public virtual void SelectFirstCharacter()
        {
            if (m_characters.Count > 0)
                SelectCharacter(0);
        }

        public virtual void RefreshList()
        {
            var characters = Game.instance.characters;

            // Najpierw wyłączamy wszystkie (istniejące) sloty w UI
            foreach (var character in m_characters)
            {
                character.gameObject.SetActive(false);
            }

            // Tworzymy lub uaktualniamy sloty dla każdej postaci
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
                        {
                            m_characters[m_currentCharacterId]?.SetInteractable(true);
                        }

                        m_audio.PlayUiEffect(selectCharacterAudio);
                        SelectCharacter(index);
                    });
                }

                m_characters[i].SetCharacter(characters[i]);
                m_characters[i].gameObject.SetActive(true);
                m_characters[i].SetInteractable(true);
            }

            // Wyłącz przycisk "New Character", jeśli gracz ma 5 postaci
            newCharacterButton.interactable = characters.Count < 5;

            // Na samym końcu - odświeżenie 3D / ustawienie w półokręgu
            RefreshCharacterDisplay();
        }


        protected virtual void Start()
        {
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
            else
            {
                Debug.LogError("Main Camera not found! Ensure your camera is tagged as 'MainCamera'.");
            }

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