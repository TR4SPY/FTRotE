using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    /// <summary>
    /// Simple window that lets the player choose one of three specializations
    /// for the current class family. Each specialization button uses a sprite
    /// named <c>Specializations_&lt;ClassFamily&gt;_X</c> where X is 0, 1 or 2. A
    /// decorative frame using the <c>Container_Specializations</c> sprite is
    /// overlaid on top of each button. When a specialization is selected the
    /// choice is forwarded to <see cref="CharacterSpecializations"/> and the
    /// Master Skill Tree window is shown.
    /// </summary>
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Specializations Window")]
    public class GUISpecializationsWindow : GUIWindow
    {
        [Header("Specialization Settings")]
        [Tooltip("Name of the class family to load specialization sprites for.")]
        public string classFamily;

        [Tooltip("Parent transform where specialization buttons will be created.")]
        public RectTransform buttonsContainer;

        [Tooltip("Prefab used to create specialization buttons.")]
        public Button buttonPrefab;

        [Tooltip("Window to show after a specialization is selected.")]
        public GUIWindow masterSkillTreeWindow;

        private readonly List<Button> m_buttons = new();

        /// <summary>
        /// Build the specialization buttons when the window opens.
        /// </summary>
        protected override void OnOpen()
        {
            base.OnOpen();
            BuildButtons();
        }

        private void BuildButtons()
        {
            if (!buttonPrefab || !buttonsContainer)
                return;

            if (m_buttons.Count > 0)
                return; // Already built

            for (int i = 0; i < 3; i++)
            {
                int index = i;
                Button btn = Object.Instantiate(buttonPrefab, buttonsContainer);

                // Set specialization sprite
                Image img = btn.GetComponent<Image>();
                if (img)
                    img.sprite = Resources.Load<Sprite>($"Specializations_{classFamily}_{index}");

                // Overlay container frame
                GameObject frameGO = new GameObject("Frame", typeof(RectTransform), typeof(Image));
                frameGO.transform.SetParent(btn.transform, false);
                Image frame = frameGO.GetComponent<Image>();
                frame.sprite = Resources.Load<Sprite>("Container_Specializations");
                frame.SetNativeSize();

                btn.onClick.AddListener(() => OnSelectSpecialization(index));
                m_buttons.Add(btn);
            }
        }

        private void OnSelectSpecialization(int index)
        {
            var def = Specializations.FindById(index);
            Game.instance?.currentCharacter?.SelectSpecialization(0, def);

            Hide();

            if (masterSkillTreeWindow != null)
                masterSkillTreeWindow.Show();
        }
    }
}
