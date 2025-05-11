using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Skill Manager")]
    public class GUISkillsManager : MonoBehaviour
    {
        [Header("Available Skills")]
        public GameObject availableSkillsContainer;  
        public GUISkillSlot slotPrefab;

        [Header("Equipped Skill Sets")]
        [Tooltip("Parent (CanvasGroup) z 4 slotami – zestaw 1.")]
        public GameObject equippedSkillsSet1;
        [Tooltip("Parent (CanvasGroup) z 4 slotami – zestaw 2.")]
        public GameObject equippedSkillsSet2;

        [Header("UI Buttons (Apply/Cancel + L/R)")]
        public Button buttonApply;
        public Button buttonCancel;
        public Button buttonLeft;
        public Button buttonRight;

        [Tooltip("Tekst typu '1/2' lub '2/2' itp.")]
        public Text skillSetText;

        private List<GUISkillSlot> m_dynamicAvailableSlots = new List<GUISkillSlot>();

        private GUISkillSlot[] m_equippedSet1Slots;
        private GUISkillSlot[] m_equippedSet2Slots;

        private List<Skill> m_equippedSkills;
        private List<Skill> equippedSkillsBefore;

        private Entity m_entity;
        private bool showingFirstSet = true;
        private bool suppressButtonUpdate = false;

        protected virtual void Start()
        {
            InitializeEntity();
            InitializeSlots();
            InitializeLists();
            InitializeCallbacks();
            Refresh();

            equippedSkillsBefore = new List<Skill>(m_equippedSkills);
            UpdateButtons();

            ShowSet1();
        }

        /// <summary>
        /// Pobranie referencji do gracza (Entity).
        /// </summary>
        protected virtual void InitializeEntity()
        {
            m_entity = Level.instance.player; 
        }

        /// <summary>
        /// Zbiera sloty z obiektów "equippedSkillsSet1" i "equippedSkillsSet2".
        /// </summary>
        protected virtual void InitializeSlots()
        {
            m_equippedSet1Slots = equippedSkillsSet1.GetComponentsInChildren<GUISkillSlot>();
            m_equippedSet2Slots = equippedSkillsSet2.GetComponentsInChildren<GUISkillSlot>();
        }

        /// <summary>
        /// Tworzymy listę 8 Skill (4 w set1 + 4 w set2).
        /// </summary>
        protected virtual void InitializeLists()
        {
            m_equippedSkills = new List<Skill>(new Skill[8]);

            for (int i = 0; i < m_equippedSet1Slots.Length; i++)
            {
                int index = i;
                m_equippedSet1Slots[i].OnDropSKill += (skill) => EquipSkill(index, skill);
            }
            for (int i = 0; i < m_equippedSet2Slots.Length; i++)
            {
                int index = i + 4;
                m_equippedSet2Slots[i].OnDropSKill += (skill) => EquipSkill(index, skill);
            }
        }

        /// <summary>
        /// Podpinamy eventy: 
        /// - onUpdatedSkills, onUpdatedEquippedSkills
        /// - doubleclick do usuwania skilli
        /// - przyciski left/right
        /// - apply/cancel
        /// </summary>
        protected virtual void InitializeCallbacks()
        {
            m_entity.skills.onUpdatedSkills.AddListener(_ => Refresh());
            m_entity.skills.onUpdatedEquippedSkills.AddListener(_ =>
            {
                suppressButtonUpdate = true;
                Refresh();
                suppressButtonUpdate = false;
            });

            for (int i = 0; i < m_equippedSet1Slots.Length; i++)
            {
                int index = i; 
                m_equippedSet1Slots[i].onIconDoubleClick.AddListener(() => RemoveSkill(index));
            }
            for (int i = 0; i < m_equippedSet2Slots.Length; i++)
            {
                int index = i + 4;
                m_equippedSet2Slots[i].onIconDoubleClick.AddListener(() => RemoveSkill(index));
            }

            if (buttonLeft)  buttonLeft.onClick.AddListener(ShowSet1);
            if (buttonRight) buttonRight.onClick.AddListener(ShowSet2);

            if (buttonApply)   buttonApply.onClick.AddListener(Apply);
            if (buttonCancel)  buttonCancel.onClick.AddListener(Cancel);
        }

        #region --- Dostępne skille (Dynamicznie) ---

        /// <summary>
        /// Ustaw listę dostępnych skilli w widoku (scroll).
        /// </summary>
        public virtual void SetAvailableSkills(Skill[] skills)
        {
            while (m_dynamicAvailableSlots.Count < skills.Length)
            {
                GUISkillSlot newSlot = Instantiate(slotPrefab, availableSkillsContainer.transform);
                m_dynamicAvailableSlots.Add(newSlot);

                // double-click => equipSkill
                newSlot.onIconDoubleClick.AddListener(() =>
                {
                    EquipSkill(newSlot.skill);
                });
            }
            while (m_dynamicAvailableSlots.Count > skills.Length)
            {
                int lastIndex = m_dynamicAvailableSlots.Count - 1;
                var slotToRemove = m_dynamicAvailableSlots[lastIndex];
                m_dynamicAvailableSlots.RemoveAt(lastIndex);
                Destroy(slotToRemove.gameObject);
            }
            // 3) Ustaw skille
            for (int i = 0; i < m_dynamicAvailableSlots.Count; i++)
            {
                var slot = m_dynamicAvailableSlots[i];
                slot.SetSkill(skills[i], true);
            }
        }

        #endregion

        #region --- Wyekwipowane skille (8) ---

        /// <summary>
        /// Ustaw 8 skillów w m_equippedSkills, potem odśwież sloty.
        /// </summary>
        public virtual void SetEquippedSkills(Skill[] skills)
        {
            if (m_equippedSkills.Count < 8)
            {
                m_equippedSkills = new List<Skill>(new Skill[8]);
            }

            for (int i = 0; i < 8; i++)
            {
                if (i < skills.Length)
                    m_equippedSkills[i] = skills[i];
                else
                    m_equippedSkills[i] = null;
            }

            RefreshEquippedSkills();
        }

        /// <summary>
        /// Odświeża UI w set1 (index 0..3) i set2 (4..7).
        /// </summary>
        public virtual void RefreshEquippedSkills()
        {
            for (int i = 0; i < m_equippedSet1Slots.Length; i++)
            {
                var skill = m_equippedSkills[i]; 
                m_equippedSet1Slots[i].SetSkill(skill, true);
            }

            for (int i = 0; i < m_equippedSet2Slots.Length; i++)
            {
                var skill = m_equippedSkills[i + 4];
                m_equippedSet2Slots[i].SetSkill(skill, true);
            }
        }

        /// <summary>
        /// Dodanie skilla w pierwsze wolne miejsce (0..7).
        /// </summary>
        public virtual void EquipSkill(Skill skill)
        {
            if (m_equippedSkills.Contains(skill)) return;

            for (int i = 0; i < m_equippedSkills.Count; i++)
            {
                if (m_equippedSkills[i] == null)
                {
                    EquipSkill(i, skill);
                    break;
                }
            }
            UpdateButtons();
        }

        /// <summary>
        /// Wstaw skill do slotu index.
        /// </summary>
        public virtual void EquipSkill(int index, Skill skill)
        {
            if (index < 0 || index >= m_equippedSkills.Count) return;

            if (m_equippedSkills.Contains(skill))
            {
                var oldIndex = m_equippedSkills.IndexOf(skill);
                if (m_equippedSkills[index] != null)
                {
                    m_equippedSkills[oldIndex] = m_equippedSkills[index];
                }
                else
                {
                    m_equippedSkills[oldIndex] = null;
                }
            }

            m_equippedSkills[index] = skill;
            RefreshEquippedSkills();
        }

        /// <summary>
        /// Usuwamy skill z danego slotu (np. double-click).
        /// </summary>
        public virtual void RemoveSkill(int index)
        {
            if (index < 0 || index >= m_equippedSkills.Count) return;

            m_equippedSkills[index] = null;
            RefreshEquippedSkills();
            UpdateButtons();
        }

        #endregion

        #region --- Apply/Cancel/Refresh ---

        /// <summary>
        /// Odświeża: available i equipped.
        /// </summary>
        public virtual void Refresh()
        {
            var allSkills = m_entity.skills.ToArray();
            SetAvailableSkills(allSkills);

            var eq = m_entity.skills.GetEquippedSkills();
            SetEquippedSkills(eq);

            if (!suppressButtonUpdate)
                UpdateButtons();
        }

        public virtual void Apply()
        {
            m_entity.skills.SetEquippedSkills(m_equippedSkills.ToArray());
            equippedSkillsBefore = new List<Skill>(m_equippedSkills);
            UpdateButtons();
        }

        public void Cancel()
        {
            m_equippedSkills = new List<Skill>(equippedSkillsBefore);
            RefreshEquippedSkills();
            UpdateButtons();
        }

        public void UpdateButtons()
        {
            bool changed = !CompareSkills();
            buttonApply.gameObject.SetActive(changed);
            buttonCancel.gameObject.SetActive(changed);
        }

        private bool CompareSkills()
        {
            if (equippedSkillsBefore == null || equippedSkillsBefore.Count != m_equippedSkills.Count)
                return false;

            for (int i = 0; i < equippedSkillsBefore.Count; i++)
            {
                if (equippedSkillsBefore[i] != m_equippedSkills[i])
                    return false;
            }
            return true;
        }

        #endregion

        #region --- Pokaż/Ukryj set1 / set2 ---

        private void ShowSet1()
        {
            SetCanvasGroup(equippedSkillsSet1, true);
            SetCanvasGroup(equippedSkillsSet2, false);

            showingFirstSet = true;
            if (skillSetText) skillSetText.text = "1 / 2 Equipped skill set";

            if (buttonLeft)  SetCanvasGroup(buttonLeft.gameObject, false);
            if (buttonRight) SetCanvasGroup(buttonRight.gameObject, true);
        }

        private void ShowSet2()
        {
            SetCanvasGroup(equippedSkillsSet1, false);
            SetCanvasGroup(equippedSkillsSet2, true);

            showingFirstSet = false;
            if (skillSetText) skillSetText.text = "2 / 2 Equipped skill set";

            if (buttonLeft)  SetCanvasGroup(buttonLeft.gameObject, true);
            if (buttonRight) SetCanvasGroup(buttonRight.gameObject, false);
        }

        private void SetCanvasGroup(GameObject obj, bool active)
        {
            if (!obj) return;
            var cg = obj.GetComponent<CanvasGroup>();
            if (!cg) return;

            cg.alpha = active ? 1f : 0f;
            cg.interactable = active;
            cg.blocksRaycasts = active;
        }

        #endregion
    }
}
