using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/UI/UI Character Form")]
    public class UICharacterForm : MonoBehaviour
    {
        [Tooltip("A reference to the Input Field component used to input the character name.")]
        public InputField characterName;
        //[Tooltip("Transform location to instantiate the class prefab preview")]
        //public Transform previewTransform;  // This is the new Transform field
        [Tooltip("The maximum amount of letters in Characters name.")]
        public GameObject maxCharactersWarning;
        public float yRotation = 0f;
        private GameObject currentPreview;


        [Tooltip("A reference to the Dropdown component used to select the character's class.")]
        public Dropdown characterClass;

        [Tooltip("A reference to the Button component to create characters.")]
        public Button createButton;
        public UnityEvent onSubmit;
        public UnityEvent onCancel;

        [Header("Audio Settings")]
        [Tooltip("The Audio Clip that plays when showing the form.")]
        public AudioClip showFormClip;

        [Tooltip("The Audio Clip that plays when cancelling the form.")]
        public AudioClip cancelFormClip;

        [Tooltip("The Audio Clip that plays when submitting the form.")]
        public AudioClip submitFormClip;

        private Coroutine hideWarningCoroutine;

        protected List<Character> m_starterClasses;
        protected CharacterInstance m_characterInstance;


        protected string[] m_characterNames;

        protected GameAudio m_audio => GameAudio.instance;

        /// <summary>
        /// Cancels the form.
        /// </summary>
        public virtual void Cancel()
        {
            m_audio.PlayUiEffect(cancelFormClip);
            CharacterPreview.instance.Clear(); 
            onCancel.Invoke();
        }

        /*
        protected virtual void HandleSubmit()
        {
            if (!ValidName(characterName.text))
                return;

            Game.instance.CreateCharacter(characterName.text, characterClass.value);
            m_audio.PlayUiEffect(submitFormClip);
            onSubmit.Invoke();
        }
        */

        protected virtual void HandleSubmit()
{
    if (Game.instance.characters.Count >= 5)
    {
        Debug.LogWarning("Cannot create more than 5 characters!");
        return;
    }

    if (!ValidName(characterName.text))
        return;

    var selectedCharacter = m_starterClasses[characterClass.value];
    var classId = GameDatabase.instance.GetElementId(selectedCharacter);

    Game.instance.CreateCharacter(characterName.text, classId);

    if (currentPreview == null && selectedCharacter.classPrefab != null)
    {
        Quaternion rotation = Quaternion.Euler(0, yRotation, 0);
        // currentPreview = Instantiate(selectedCharacter.classPrefab, previewTransform.position, rotation, previewTransform);
    }

    m_audio.PlayUiEffect(submitFormClip);

    DestroyPreviewPrefab();

    onSubmit.Invoke();

    UICharacterSelection characterSelection = Object.FindFirstObjectByType<UICharacterSelection>();
    if (characterSelection != null)
    {
        characterSelection.RefreshList();
        characterSelection.SelectCharacter(Game.instance.characters.Count - 1);
    }
}


        protected virtual bool ValidName(string name) =>
    name.Length > 0 && name.Length <= 16 && !m_characterNames.Contains(name);

        protected IEnumerator SelectInputField()
        {
            yield return null;

            characterName.ActivateInputField();
            characterName.Select();
        }

        protected virtual void Start()
        {
            characterName.characterLimit = 16;
            //characterLimitInfo.text = "Max 16 characters."; 

            characterName.onValueChanged.AddListener(HandleNameInput);

            createButton.onClick.AddListener(HandleSubmit);
            characterName.onValueChanged.AddListener((value) =>
                createButton.interactable = ValidName(value));
            characterClass.onValueChanged.AddListener(HandleClassChange);
        }

        private void HandleNameInput(string name)
    {
        if (name.Length >= 16)
        {
            ShowMaxCharactersWarning();
        }
        else
        {
            if (maxCharactersWarning.activeSelf)
            {
                HideMaxCharactersWarningImmediately();
            }
        }
    }

        private void ShowMaxCharactersWarning()
        {
            if (maxCharactersWarning != null)
            {
                maxCharactersWarning.SetActive(true);

                if (hideWarningCoroutine != null)
                {
                    StopCoroutine(hideWarningCoroutine);
                }

                hideWarningCoroutine = StartCoroutine(HideWarningAfterDelay(5f));
            }
        }
        private void HideMaxCharactersWarningImmediately()
        {
            if (hideWarningCoroutine != null)
            {
                StopCoroutine(hideWarningCoroutine);
                hideWarningCoroutine = null;
            }
            maxCharactersWarning.SetActive(false);
        }

        private IEnumerator HideWarningAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            maxCharactersWarning.SetActive(false);
            hideWarningCoroutine = null;
        }

        protected virtual void HandleClassChange(int index)
        {
            if (index < 0 || index >= m_starterClasses.Count)
            {
                Debug.LogWarning($"[UICharacterForm] Invalid class index: {index}");
                return;
            }

            var selectedCharacter = m_starterClasses[index];
            m_characterInstance = new CharacterInstance(selectedCharacter, "[PREVIEW]");
            CharacterPreview.instance.Preview(m_characterInstance);
        }


#if UNITY_STANDALONE || UNITY_WEBGL
        protected virtual void Update()
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
                HandleSubmit();
            else if (Keyboard.current.escapeKey.wasPressedThisFrame)
                Cancel();
        }
#endif

        private void DestroyPreviewPrefab()
        {
            if (currentPreview != null)
            {
                Destroy(currentPreview);
                currentPreview = null;
            }
        }
        /*
        protected virtual void OnEnable()
        {
            var classes = GameDatabase.instance.characters.Select(c => c.name);

            m_characterNames = Game.instance.characters.Select(c => c.name).ToArray();
            characterClass.ClearOptions();
            characterClass.AddOptions(classes.ToList());
            characterName.text = "";
            createButton.interactable = false;
            StartCoroutine(SelectInputField());
            m_audio.PlayUiEffect(showFormClip);
        }
        */
        
        protected virtual void OnEnable()
{
    characterClass.ClearOptions();

    var allowedStartClasses = new[]
    {
        CharacterClassRestrictions.Knight,
        CharacterClassRestrictions.Arcanist
    };

    m_starterClasses = GameDatabase.instance.characters
        .Where(c => ClassHierarchy.NameToBits.TryGetValue(c.name, out var classType) && allowedStartClasses.Contains(classType)).ToList();

    m_characterNames = Game.instance.characters.Select(c => c.name).ToArray();

    characterClass.AddOptions(m_starterClasses.Select(c => c.name).ToList());

    characterName.text = "";
    createButton.interactable = false;

    characterClass.SetValueWithoutNotify(0);
    HandleClassChange(0);

    StartCoroutine(SelectInputField());
    m_audio.PlayUiEffect(showFormClip);
}

protected virtual void OnDisable()
{
    CharacterPreview.instance.Clear();
}

    }
}
