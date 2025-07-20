using UnityEngine.InputSystem;

namespace PLAYERTWO.ARPGProject
{
    public partial class EntityInputs
    {
        protected InputAction m_setDestinationAction;
        protected InputAction m_attackModeAction;
        protected InputAction m_skillAction;
        protected InputAction m_consumeItem0;
        protected InputAction m_consumeItem1;
        protected InputAction m_consumeItem2;
        protected InputAction m_consumeItem3;
        protected InputAction m_selectSkill0;
        protected InputAction m_selectSkill1;
        protected InputAction m_selectSkill2;
        protected InputAction m_selectSkill3;
        protected InputAction m_directionalMovement;
        protected InputAction m_attackAction;
        protected InputAction m_interactAction;

        protected virtual InputActionMap GetActiveMap()
        {
            int option = GameSettings.instance ? GameSettings.instance.GetMovementSetting() : 0;
            return option == 0 ? m_gameplayMap : m_gameplay1Map;
        }

        protected virtual void AssignActions(InputActionMap map)
        {
            if (map == null)
                return;

            m_setDestinationAction = map.FindAction("Set Destination", false);
            m_skillAction = map.FindAction("Skill", false);
            m_attackModeAction = map.FindAction("Attack Mode", false);
            m_consumeItem0 = map.FindAction("Consume Item 0", false);
            m_consumeItem1 = map.FindAction("Consume Item 1", false);
            m_consumeItem2 = map.FindAction("Consume Item 2", false);
            m_consumeItem3 = map.FindAction("Consume Item 3", false);
            m_selectSkill0 = map.FindAction("Select Skill 0", false);
            m_selectSkill1 = map.FindAction("Select Skill 1", false);
            m_selectSkill2 = map.FindAction("Select Skill 2", false);
            m_selectSkill3 = map.FindAction("Select Skill 3", false);
            m_directionalMovement = map.FindAction("Directional Movement", false);
            m_attackAction = map.FindAction("Attack", false);
            m_interactAction = map.FindAction("Interact", false);
        }

        protected virtual void InitializeActions()
        {
            if (actions == null)
                return;

            InitializeActionMaps();
            AssignActions(GetActiveMap());
        }

        protected virtual void InitializeActionsCallbacks()
        {
            m_setDestinationAction.performed += OnSetDestination;
            m_setDestinationAction.canceled += OnSetDestinationCancelled;
            m_directionalMovement.performed += OnDirectionalMovement;
            m_skillAction.performed += OnSkill;
            m_skillAction.canceled += OnSkillCancelled;
            m_attackModeAction.performed += OnAttackMode;
            m_attackModeAction.canceled += OnAttackModeCancelled;
            m_consumeItem0.performed += OnConsumeItem0;
            m_consumeItem1.performed += OnConsumeItem1;
            m_consumeItem2.performed += OnConsumeItem2;
            m_consumeItem3.performed += OnConsumeItem3;
            m_selectSkill0.performed += OnSelectSkill0;
            m_selectSkill1.performed += OnSelectSkill1;
            m_selectSkill2.performed += OnSelectSkill2;
            m_selectSkill3.performed += OnSelectSkill3;
            m_attackAction.performed += OnAttack;
            m_attackAction.canceled += OnAttackCancelled;
            m_interactAction.performed += OnInteract;
        }

        protected virtual void FinalizeActionCallbacks()
        {
            if (m_setDestinationAction != null)
            {
                m_setDestinationAction.performed -= OnSetDestination;
                m_setDestinationAction.canceled -= OnSetDestinationCancelled;
            }

            if (m_skillAction != null)
            {
                m_skillAction.performed -= OnSkill;
                m_skillAction.canceled -= OnSkillCancelled;
            }

            if (m_attackModeAction != null)
            {
                m_attackModeAction.performed -= OnAttackMode;
                m_attackModeAction.canceled -= OnAttackModeCancelled;
            }

            if (m_consumeItem0 != null)
                m_consumeItem0.performed -= OnConsumeItem0;
            if (m_consumeItem1 != null)
                m_consumeItem1.performed -= OnConsumeItem1;
            if (m_consumeItem2 != null)
                m_consumeItem2.performed -= OnConsumeItem2;
            if (m_consumeItem3 != null)
                m_consumeItem3.performed -= OnConsumeItem3;

            if (m_selectSkill0 != null)
                m_selectSkill0.performed -= OnSelectSkill0;
            if (m_selectSkill1 != null)
                m_selectSkill1.performed -= OnSelectSkill1;
            if (m_selectSkill2 != null)
                m_selectSkill2.performed -= OnSelectSkill2;
            if (m_selectSkill3 != null)
                m_selectSkill3.performed -= OnSelectSkill3;

            if (m_attackAction != null)
            {
                m_attackAction.performed -= OnAttack;
                m_attackAction.canceled -= OnAttackCancelled;
            }

            if (m_interactAction != null)
                m_interactAction.performed -= OnInteract;
        }
    }
}
