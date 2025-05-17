using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(Entity))]
    [AddComponentMenu("PLAYER TWO/ARPG Project/Entity/Entity Health Bar")]
    public class EntityHealthBar : MonoBehaviour
    {
        [Tooltip("Offset of the health bar from the entity's position in screen space.")]
        public Vector2 offset = new(0, 50f);

        protected Entity m_entity;
        protected GUIHealthBar m_healthBar;

        protected virtual void Start()
        {
            InitializeEntity();
            InitializeHealthBar();
            InitializeCallbacks();
        }

        protected virtual void InitializeEntity()
        {
            m_entity = GetComponent<Entity>();
        }

        protected virtual void InitializeHealthBar()
        {
            m_healthBar = GUIHealthBarManager.instance.InstantiateHealthBar(m_entity);

            if (m_healthBar)
            {
                m_healthBar.target = transform;
                m_healthBar.offset = offset;
                m_healthBar.UpdatePosition();
                m_healthBar.SetHealth(m_entity.stats.GetHealthPercent());
                GUIHealthBarManager.instance.AddHealthBar(m_healthBar);
            }
        }

        protected virtual void InitializeCallbacks()
        {
            if (m_healthBar == null)
                return;

            m_entity.onDie.AddListener(
                () => GUIHealthBarManager.instance.RemoveHealthBar(m_healthBar)
            );
            m_entity.stats.onHealthChanged.AddListener(
                () => m_healthBar.SetHealth(m_entity.stats.GetHealthPercent())
            );
        }

        protected virtual void OnDisable()
        {
            if (m_healthBar)
                m_healthBar.gameObject.SetActive(false);
        }

        protected virtual void OnEnable()
        {
            if (m_healthBar)
                m_healthBar.gameObject.SetActive(true);
        }
    }
}
