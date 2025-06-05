using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Skill Inspector")]
    public class GUISkillInspector : GUIInspector<GUISkillInspector>
    {
        [Tooltip("A reference to the Text component used as the Skill name.")]
        public Text skillName;

        [Tooltip("A reference to the Text component used as the Skill mana cost.")]
        public Text manaCost;

        [Tooltip("A reference to the Text component used as the Skill health cost.")]
        public Text bloodCost;

        [Tooltip("A reference to the Text component used as the Skill base damage.")]
        public Text baseDamage;

        [Tooltip("A reference to the Text component used as the Skill current damage.")]
        public Text currentDamage;

        [Tooltip("A reference to the Text component used as the Skill damage mode.")]
        public Text damageMode;

        [Tooltip("A reference to the Text component used as the Skill effect description.")]
        public Text effectDescription;
        
        [Tooltip("A reference to the Text component used for magic classification.")]
        public Text magicDescription;


        protected bool m_visible = true;

        protected CanvasGroup m_group;
        protected Skill m_skill;

        protected virtual void InitializeCanvasGroup()
        {
            if (!TryGetComponent(out m_group))
                m_group = gameObject.AddComponent<CanvasGroup>();

            m_group.blocksRaycasts = false;
        }

        /// <summary>
        /// Shows the inspector with the information from a given Skill.
        /// </summary>
        /// <param name="skill">The Skill you want inspect.</param>
        public virtual void Show(Skill skill)
        {
            if (!skill || gameObject.activeSelf) return;

            m_skill = skill;
            gameObject.SetActive(true);
            UpdateAll();
            FadIn();
        }

        /// <summary>
        /// Hides the inspector.
        /// </summary>
        public virtual void Hide()
        {
            if (!gameObject.activeSelf) return;

            gameObject.SetActive(false);
        }

        protected virtual void UpdateAll()
        {
            UpdateSkillName();
            UpdateManaCost();
            UpdateBloodCost();
            UpdateDamage();
            UpdateMagicClassification();
            UpdateEffect();
        }

        // protected virtual void UpdateSkillName() => skillName.text = m_skill.name;
        protected virtual void UpdateSkillName()
        {
            if (m_skill.element != MagicElement.None)
            {
                var color = GameColors.ElementColor(m_skill.element);
                skillName.text = StringUtils.StringWithColor(m_skill.name, color);
            }
            else
            {
                skillName.text = m_skill.name;
            }
        }

        protected virtual void UpdateManaCost()
        {
            manaCost.gameObject.SetActive(m_skill.useMana);

            if (manaCost.gameObject.activeSelf)
                manaCost.text = $"Mana Cost: {m_skill.manaCost.ToString()}";
        }

        protected virtual void UpdateBloodCost()
        {
            bloodCost.gameObject.SetActive(m_skill.useBlood);

            if (bloodCost.gameObject.activeSelf)
                bloodCost.text = $"Blood Cost: {m_skill.bloodCost}";
        }

        protected virtual void UpdateDamage()
        {
            bool isAttackSkill = m_skill.IsAttack();

            baseDamage.gameObject.SetActive(isAttackSkill);
            currentDamage.gameObject.SetActive(isAttackSkill);
            damageMode.gameObject.SetActive(isAttackSkill);

            if (!isAttackSkill)
            {
                baseDamage.text = "Base damage: N/A";
                currentDamage.text = "Current damage: N/A";
                damageMode.text = "Type: N/A";
                return;
            }

            var attackSkill = m_skill.AsAttack();
            baseDamage.text = $"Base damage: {attackSkill.minDamage} ~ {attackSkill.maxDamage}";

            var entityStats = Game.instance.currentCharacter.Entity?.stats;
            if (entityStats == null)
            {
                currentDamage.text = "Current damage: N/A";
                return;
            }

            int baseMin = attackSkill.minDamage;
            int baseMax = attackSkill.maxDamage;

            int bonusWeaponMin = entityStats.CalculateSkillBonusFromWeapon(attackSkill, false);
            int bonusWeaponMax = entityStats.CalculateSkillBonusFromWeapon(attackSkill, true);

            int elementBonus = m_skill.element != MagicElement.None
                ? entityStats.GetElementalBonus(m_skill.element)
                : 0;

            int finalMin = baseMin + bonusWeaponMin + elementBonus;
            int finalMax = baseMax + bonusWeaponMax + elementBonus;

            int totalMin = baseMin + bonusWeaponMin;
            int totalMax = baseMax + bonusWeaponMax;

            var elementColor = GameColors.ElementColor(m_skill.element);
            string colored = "";
            var currentWeapon = entityStats.GetCurrentWeapon();

            bool matchingElement =
                m_skill.element != MagicElement.None &&
                currentWeapon != null &&
                currentWeapon.magicElement == m_skill.element;

            if (matchingElement)
            {
                colored = " " + StringUtils.StringWithColor($"({finalMin} – {finalMax})", elementColor);
            }

            currentDamage.text = $"Current damage: {totalMin} – {totalMax}{colored}";
            damageMode.text = $"Damage mode: {attackSkill.damageMode}";
        }

        protected virtual void UpdateMagicClassification()
        {
            if (!magicDescription) return;

            var school = m_skill.school != MagicSchool.None ? m_skill.school.ToString() : null;
            var form = m_skill.form != MagicForm.None ? m_skill.form.ToString() : null;
            var type = m_skill.type != MagicType.None ? m_skill.type.ToString() : null;

            string classification = string.Join(" > ", new[] { school, form, type }.Where(x => !string.IsNullOrEmpty(x)));

            magicDescription.gameObject.SetActive(!string.IsNullOrEmpty(classification));
            magicDescription.text = classification;
        }


        protected virtual void UpdateEffect()
        {
            effectDescription.gameObject.SetActive(m_skill.useHealing);

            if (effectDescription.gameObject.activeSelf)
                effectDescription.text = $"Increases Health by {m_skill.healingAmount}";
        }

        protected virtual void Start()
        {
            InitializeCanvasGroup();
            Hide();
        }
    }
}
