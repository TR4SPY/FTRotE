using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    /// <summary>
    /// Interfejs, który powinien zaimplementować komponent odpowiedzialny
    /// za sprawdzanie/ustawianie posiadanych skilli w danej specjalizacji.
    /// </summary>
    public interface ISkillAllocator
    {
        bool HasSkill(Specializations specialization, Skill skill);
        void SetSkill(Specializations specialization, Skill skill, bool allocated);
    }

    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Master Skill Tree")]
    public class GUIMasterSkillTree : MonoBehaviour
    {
        [System.Serializable]
        public class SkillNode
        {
            [Tooltip("Skill represented by this node.")]
            public Skill skill;

            [Tooltip("GUI slot displaying the skill.")]
            public GUISkillSlot slot;

            [Tooltip("Point cost required to unlock this skill.")]
            public int cost = 1;

            [HideInInspector] public bool allocated;
        }

        [Header("Specialization")]
        [Tooltip("Selected specialization whose skills are shown (must expose 'skills' list).")]
        public Specializations specialization;

        [Tooltip("Component that implements ISkillAllocator (e.g., your CharacterSpecializations).")]
        [SerializeField] private MonoBehaviour characterSpecializationsComponent;

        private ISkillAllocator m_allocator;

        [Header("Skill Nodes")]
        [Tooltip("Nodes displayed for this skill tree. Populate in the inspector.")]
        public List<SkillNode> nodes = new();

        [Header("UI")]
        [Tooltip("Text showing available points.")]
        public Text availablePointsText;

        [Tooltip("Apply button.")]
        public Button applyButton;

        [Tooltip("Cancel button.")]
        public Button cancelButton;

        [Tooltip("Maximum number of points available to spend.")]
        public int maxPoints = 5;

        private readonly Dictionary<SkillNode, bool> m_original = new();
        private int m_availablePoints;

        protected virtual void Start()
        {
            // Bezpieczne rzutowanie na interfejs — bez dynamic.
            m_allocator = characterSpecializationsComponent as ISkillAllocator;
            if (characterSpecializationsComponent && m_allocator == null)
            {
                Debug.LogWarning($"{nameof(GUIMasterSkillTree)}: Assigned component does not implement {nameof(ISkillAllocator)}.");
            }

            BuildFromSpecialization();

            if (applyButton) applyButton.onClick.AddListener(Apply);
            if (cancelButton) cancelButton.onClick.AddListener(Cancel);

            UpdateAvailablePointsText();
            UpdateApplyCancelButtons();
        }

        private void BuildFromSpecialization()
        {
            m_availablePoints = maxPoints;

            var specializationSkills = new HashSet<Skill>();
            if (specialization && specialization.skills != null)
            {
                foreach (var s in specialization.skills)
                    if (s) specializationSkills.Add(s);
            }

            foreach (var node in nodes)
            {
                bool inSpec = specializationSkills.Count == 0 || specializationSkills.Contains(node.skill);

                if (node.slot)
                    node.slot.gameObject.SetActive(inSpec);

                if (!inSpec)
                    continue;

                if (node.slot)
                {
                    node.slot.SetSkill(node.skill, false);
                    var n = node; // local copy for closure
                    node.slot.onIconClick.AddListener(() => ToggleNode(n));
                }

                bool hasSkill = HasSkill(node.skill);
                node.allocated = hasSkill;
                m_original[node] = hasSkill;
                if (hasSkill)
                    m_availablePoints -= node.cost;
            }
        }

        private bool HasSkill(Skill skill)
        {
            if (m_allocator == null || specialization == null || skill == null)
                return false;

            return m_allocator.HasSkill(specialization, skill);
        }

        private void ToggleNode(SkillNode node)
        {
            if (node.allocated)
                Deallocate(node);
            else
                Allocate(node);
        }

        private void Allocate(SkillNode node)
        {
            if (node.allocated || m_availablePoints < node.cost)
                return;

            node.allocated = true;
            m_availablePoints -= node.cost;
            UpdateAvailablePointsText();
            UpdateApplyCancelButtons();
        }

        private void Deallocate(SkillNode node)
        {
            if (!node.allocated)
                return;

            node.allocated = false;
            m_availablePoints += node.cost;
            UpdateAvailablePointsText();
            UpdateApplyCancelButtons();
        }

        private void UpdateAvailablePointsText()
        {
            if (availablePointsText)
                availablePointsText.text = m_availablePoints.ToString();
        }

        public void Apply()
        {
            if (m_allocator == null || specialization == null)
                return;

            foreach (var node in nodes)
            {
                bool original = m_original.TryGetValue(node, out var value) && value;
                if (node.allocated != original)
                {
                    m_allocator.SetSkill(specialization, node.skill, node.allocated);
                    m_original[node] = node.allocated;
                }
            }

            UpdateApplyCancelButtons();
        }

        public void Cancel()
        {
            foreach (var node in nodes)
            {
                if (m_original.TryGetValue(node, out var value))
                    node.allocated = value;
            }

            m_availablePoints = maxPoints;
            foreach (var node in nodes)
                if (node.allocated)
                    m_availablePoints -= node.cost;

            UpdateAvailablePointsText();
            UpdateApplyCancelButtons();
        }

        public void UpdateApplyCancelButtons()
        {
            bool changed = false;
            foreach (var node in nodes)
            {
                bool original = m_original.TryGetValue(node, out var value) && value;
                if (node.allocated != original)
                {
                    changed = true;
                    break;
                }
            }

            if (applyButton)
                applyButton.gameObject.SetActive(changed);
            if (cancelButton)
                cancelButton.gameObject.SetActive(changed);
        }
    }
}
