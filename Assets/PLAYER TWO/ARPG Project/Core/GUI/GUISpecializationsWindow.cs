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

        [Tooltip("Buttons representing the available specializations (in order 0,1,2).")]
        public Button[] specializationButtons = new Button[3];

        [Tooltip("Window to show after a specialization is selected.")]
        public GUIWindow masterSkillTreeWindow;

        /// <summary>
        /// Build or refresh specialization buttons when the window opens.
        /// </summary>
        protected override void OnOpen()
        {
            base.OnOpen();
            BuildButtons();
        }

        private void BuildButtons()
        {
            for (int i = 0; i < specializationButtons.Length; i++)
            {
                Button btn = specializationButtons[i];
                if (!btn)
                    continue;

                int index = i;

                Image img = btn.GetComponent<Image>();
                if (img)
                    img.sprite = Resources.Load<Sprite>($"Specializations_{classFamily}_{index}");

                Transform frameTr = btn.transform.Find("Frame");
                Image frame = null;
                if (frameTr)
                    frame = frameTr.GetComponent<Image>();
                else
                {
                    GameObject frameGO = new GameObject("Frame", typeof(RectTransform), typeof(Image));
                    frameGO.transform.SetParent(btn.transform, false);
                    frame = frameGO.GetComponent<Image>();
                }

                frame.sprite = Resources.Load<Sprite>("Container_Specializations");
                frame.SetNativeSize();

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnSelectSpecialization(index));
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