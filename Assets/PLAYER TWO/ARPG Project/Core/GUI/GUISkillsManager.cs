using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Skill Manager")]
    public class GUISkillsManager : MonoBehaviour
    {
        [Tooltip("A reference to the Game Object that contains all available Skill slots.")]
        public GameObject availableSkillsContainer;

        [Tooltip("A reference to the Game Object that contains all equipped Skill slots.")]
        public GameObject equippedSkillsContainer;

        [Header("Main Buttons")]
        [Tooltip("A reference to the Apply button.")]
        public Button applyButton;

        [Tooltip("A reference to the Cancel button.")]
        public Button cancelButton;

        protected GUISkillSlot[] m_availableSkillsSlots;
        protected GUISkillSlot[] m_equippedSkillsSlots;

        protected List<Skill> m_availableSkills;
        protected List<Skill> m_equippedSkills;
        private List<Skill> equippedSkillsBefore;

        protected Entity m_entity;

        protected virtual void InitializeEntity() => m_entity = Level.instance.player;

        protected virtual void InitializeSlots()
        {
            m_availableSkillsSlots = availableSkillsContainer.GetComponentsInChildren<GUISkillSlot>();
            m_equippedSkillsSlots = equippedSkillsContainer.GetComponentsInChildren<GUISkillSlot>();
        }

        protected virtual void InitializeLists()
        {
            m_availableSkills = new List<Skill>(new Skill[m_availableSkillsSlots.Length]);
            m_equippedSkills = new List<Skill>(new Skill[m_equippedSkillsSlots.Length]);

            for (int i = 0; i < m_equippedSkillsSlots.Length; i++)
            {
                var index = i;
                m_equippedSkillsSlots[i].OnDropSKill += (skill) => EquipSkill(index, skill);
            }
        }

        protected virtual void InitializeCallbacks()
        {
            m_entity.skills.onUpdatedSkills.AddListener(_ => Refresh());
            m_entity.skills.onUpdatedEquippedSkills.AddListener(_ => Refresh());

            for (int i = 0; i < m_availableSkillsSlots.Length; i++)
            {
                var index = i;

                if (i < m_equippedSkillsSlots.Length)
                {
                    m_equippedSkillsSlots[i].onIconDoubleClick.AddListener(() =>
                        RemoveSkill(index));
                }

                m_availableSkillsSlots[i].onIconDoubleClick.AddListener(() =>
                    EquipSkill(m_availableSkillsSlots[index].skill));
            }
        }

        /// <summary>
        /// Sets all available Skill Slots from a given array.
        /// </summary>
        /// <param name="skills">The array of Skills to be set as available skills.</param>
        public virtual void SetAvailableSkills(Skill[] skills)
        {
            for (int i = 0; i < m_availableSkillsSlots.Length; i++)
            {
                if (i >= skills.Length)
                {
                    m_availableSkills[i] = null;
                    m_availableSkillsSlots[i].SetSkill(null);
                    continue;
                }

                m_availableSkills[i] = skills[i];
                m_availableSkillsSlots[i].SetSkill(m_availableSkills[i], true);
            }
        }

        /// <summary>
        /// Sets all equipped Skill Slots from a given array.
        /// </summary>
        /// <param name="skills">The array of Skills to be set as equipped skills.</param>
        public virtual void SetEquippedSkills(Skill[] skills)
        {
            for (int i = 0; i < m_equippedSkillsSlots.Length; i++)
            {
                if (i >= skills.Length)
                {
                    m_equippedSkills[i] = null;
                    m_equippedSkillsSlots[i].SetSkill(null);
                    continue;
                }

                m_equippedSkills[i] = skills[i];
                m_equippedSkillsSlots[i].SetSkill(m_equippedSkills[i], true);
            }
        }

        /// <summary>
        /// Equips a given Skill on the first free slot.
        /// </summary>
        /// <param name="skill">The Skill you want to equip.</param>
        public virtual void EquipSkill(Skill skill)
        {
            if (m_equippedSkills.Contains(skill)) return;

            for (int i = 0; i < m_equippedSkillsSlots.Length; i++)
            {
                if (!m_equippedSkills[i])
                {
                    EquipSkill(i, skill);
                    break;
                }
            }

            UpdateButtons();
        }

        /// <summary>
        /// Removes a equipped Skill based on its index.
        /// </summary>
        /// <param name="index">The index of the equipped Skill you want to remove.</param>
        public virtual void RemoveSkill(int index)
        {
            if (index < 0 || index >= m_equippedSkills.Count) return;

            m_equippedSkills[index] = null;
            RefreshEquippedSkills();
            UpdateButtons();
        }

        /// <summary>
        /// Equips a Skill in a given slot by its index.
        /// </summary>
        /// <param name="index">The index of the slot you want to equip.</param>
        /// <param name="skill">The Skill you want to equip.</param>
        public virtual void EquipSkill(int index, Skill skill)
        {
            if (m_equippedSkills.Contains(skill))
            {
                var i = m_equippedSkills.IndexOf(skill);

                if (m_equippedSkills[index])
                {
                    m_equippedSkills[i] = m_equippedSkills[index];
                }
                else
                {
                    m_equippedSkills[i] = null;
                }
            }

            m_equippedSkills[index] = skill;
            RefreshEquippedSkills();
        }

        /// <summary>
        /// Refreshes the list of equipped skills updating its data.
        /// </summary>
        public virtual void RefreshEquippedSkills()
        {
            for (int i = 0; i < m_equippedSkillsSlots.Length; i++)
            {
                if (i >= m_equippedSkills.Count)
                {
                    m_equippedSkillsSlots[i].SetSkill(null);
                    continue;
                }

                m_equippedSkillsSlots[i].SetSkill(m_equippedSkills[i], true);
            }
        }

        /// <summary>
        /// Aktualizuje widoczność przycisków Apply i Cancel.
        /// </summary>
        public void UpdateButtons()
        {
            bool shouldShow = !CompareSkills();

            applyButton.gameObject.SetActive(shouldShow);
            cancelButton.gameObject.SetActive(shouldShow);
        }

        /// <summary>
        /// Sprawdza, czy obecne skille są identyczne z zapisanymi.
        /// </summary>
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

        /// <summary>
        /// Refreshes the available and equipped skills slots.
        /// </summary>
        public virtual void Refresh()
        {
            SetAvailableSkills(m_entity.skills.ToArray());
            SetEquippedSkills(m_entity.skills.GetEquippedSkills());
        }

        /// <summary>
        /// Applies the equipped Skills to Entity.
        /// </summary>
        public virtual void Apply()
        {
            m_entity.skills.SetEquippedSkills(m_equippedSkills.ToArray());
            
            equippedSkillsBefore = new List<Skill>(m_equippedSkills);
            UpdateButtons();
        }

        /// <summary>
        /// Cancel all the changes and revert back to the initial state.
        /// </summary>
        public void Cancel()
        {
            m_equippedSkills = new List<Skill>(equippedSkillsBefore);
            RefreshEquippedSkills();

            UpdateButtons();
        }

        protected virtual void Start()
        {
            InitializeEntity();
            InitializeSlots();
            InitializeLists();
            InitializeCallbacks();
            Refresh();

            equippedSkillsBefore = new List<Skill>(m_equippedSkills);
            UpdateButtons();
        }
    }
}
