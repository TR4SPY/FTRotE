using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("PLAYER TWO/ARPG Project/UI/UI Character")]
    public class UICharacterButton : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("A reference to the Text component used as the Character's name.")]
        public Text nameText;

        [Tooltip("A reference to the Text component used as the Character's level.")]
        public Text levelText;
        public UnityEvent<CharacterInstance> onSelect;
        public UnityEvent<CharacterInstance> onDoubleClick;

        protected Button m_button;

        /// <summary>
        /// Returns the Character Instance associated to this UI Character Button.
        /// </summary>
        public CharacterInstance character { get; protected set; }

        protected virtual void InitializeButton()
        {
            m_button = GetComponent<Button>();
        }


        /// <summary>
        /// Sets the Character Instance of this UI Character Button.
        /// </summary>
        /// <param name="character">The Character Instance you want to set.</param>
        public virtual void SetCharacter(CharacterInstance character)
        {
            this.character = character;
            nameText.text = this.character.name;
            levelText.text = $"Level {this.character.stats.initialLevel.ToString()}";
        }

        /// <summary>
        /// Sets the interactable value.
        /// </summary>
        /// <param name="value">If true, the button will be interactable.</param>
        public virtual void SetInteractable(bool value) => m_button.interactable = value;

        protected virtual void Awake() => InitializeButton();

        public void OnPointerClick(PointerEventData e)
        {
            if (e.clickCount == 1)
                onSelect.Invoke(character);
            else if (e.clickCount == 2)
                onDoubleClick.Invoke(character);
        }

    }
}
