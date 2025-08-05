using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Reflection;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Buff Slot")]
    public class GUIBuffSlot : GUISlot, IPointerEnterHandler, IPointerExitHandler
    {
        [Tooltip("A reference to the Image component used as the selection outline.")]
        public Image selection;

        [Tooltip("A reference to the Image component used as the buff cool down image.")]
        public Image coolDownImage;

        [Tooltip("A reference to the Image component used as the slot frame.")]
        public Image frame;

        [Header("Pulse Settings")]
        [Tooltip("Pulse speed used for buffs.")]
        public float buffPulseSpeed = 1f;

        [Tooltip("Pulse speed used for debuffs.")]
        public float debuffPulseSpeed = 2f;

        [Tooltip("Frame target color for buffs.")]
        public Color buffPulseColor = GameColors.Green;

        [Tooltip("Frame target color for debuffs.")]
        public Color debuffPulseColor = GameColors.LightRed;


        [Header("Slot Events")]
        public UnityEvent onIconClick;
        public UnityEvent onIconDoubleClick;

        protected GUISkillIcon m_icon;
        protected Coroutine m_coolDownRoutine;
        protected Coroutine m_pulseRoutine;

        private static readonly FieldInfo s_lastTargetField = typeof(GUITooltip).GetField("lastTarget", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Returns the current buff on this slot.
        /// </summary>
        public BuffInstance buff { get; protected set; }

        /// <summary>
        /// Returns the GUI Skill Icon children of this slot.
        /// </summary>
        public GUISkillIcon icon
        {
            get
            {
                if (!m_icon)
                {
                    m_icon = GetComponentInChildren<GUISkillIcon>();
                }

                return m_icon;
            }
        }

        /// <summary>
        /// Returns true if this buff slot is selected.
        /// </summary>
        public bool selected => selection && selection.enabled;

        /// <summary>
        /// Sets a given buff instance on this slot.
        /// </summary>
        /// <param name="instance">The buff instance you want to set.</param>
        public virtual void SetBuff(BuffInstance instance)
        {
            buff = instance;
            if (icon) icon.draggable = false;
            //SetIcon(buff?.data?.icon);
            SetIcon(buff?.buff?.icon);
            Visible(buff != null);

            StopPulse();

            if (buff != null && frame)
            {
                bool isDebuff = buff.isDebuff || (buff.buff != null && buff.buff.isDebuff);
                m_pulseRoutine = StartCoroutine(PulseFrame(isDebuff));
            }
            else if (frame)
            {
                frame.color = Color.black;
            }

        }

        /// <summary>
        /// Sets the sprite of the icon on this slot.
        /// </summary>
        /// <param name="sprite">The sprite you want to set.</param>
        public virtual void SetIcon(Sprite sprite)
        {
            if (icon) icon.image.sprite = sprite;
        }

        /// <summary>
        /// Sets the visibility of the icon.
        /// </summary>
        /// <param name="value">If true, the icon is visible.</param>
        public virtual void Visible(bool value)
        {
            if (icon) icon.image.enabled = value;
        }

        /// <summary>
        /// Selects this slot highlighting it.
        /// </summary>
        /// <param name="value">If true, the slot will be highlighted.</param>
        public virtual void Select(bool value)
        {
            if (selection) selection.enabled = value;
        }

        /// <summary>
        /// Starts the cool down counter.
        /// </summary>
        /// <param name="duration">The duration of the cool down.</param>
        public virtual void StartCoolDown(float duration)
        {
            if (m_coolDownRoutine != null)
                StopCoroutine(m_coolDownRoutine);

            m_coolDownRoutine = StartCoroutine(CoolDownRoutine(duration));
        }

        protected IEnumerator CoolDownRoutine(float coolDown)
        {
            var duration = coolDown;

            if (coolDownImage) coolDownImage.fillAmount = 1;

            while (duration > 0)
            {
                var delta = Time.unscaledDeltaTime;
                if (delta <= 0f)
                    break;

                duration -= delta;
                if (coolDownImage) coolDownImage.fillAmount = duration / coolDown;
                yield return null;
            }

            if (coolDownImage) coolDownImage.fillAmount = 0;
        }

        protected virtual void Start()
        {
            if (icon)
            {
                icon.onClick.AddListener(onIconClick.Invoke);
                icon.onDoubleClick.AddListener(onIconDoubleClick.Invoke);
            }
        }

        protected virtual void ShowBuffTooltip()
        {
            if (buff != null)
            {
                GUITooltip.instance.ShowTooltip(buff.buff.name, TooltipFormatter.FormatBuffTooltip(buff), gameObject);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowBuffTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            GUITooltip.instance.HideTooltipDynamic();
        }

        protected virtual void OnEnable() => Select(false);

        protected virtual void OnDisable()
        {
            GUITooltip.instance.HideTooltipDynamic();
            StopPulse();
        }

        protected virtual void OnDestroy()
        {
            StopPulse();

            if (!GUITooltip.instance || s_lastTargetField == null) return;

            var target = s_lastTargetField.GetValue(GUITooltip.instance) as GameObject;
            if (target == gameObject)
            {
                GUITooltip.instance.HideTooltipDynamic();
            }
        }
        protected virtual void StopPulse()
        {
            if (m_pulseRoutine != null)
            {
                StopCoroutine(m_pulseRoutine);
                m_pulseRoutine = null;
            }

            if (frame)
                frame.color = Color.black;
        }

        protected IEnumerator PulseFrame(bool isDebuff)
        {
            var target = isDebuff ? debuffPulseColor : buffPulseColor;
            var speed = isDebuff ? debuffPulseSpeed : buffPulseSpeed;

            float t = 0f;
            while (true)
            {
                t += Time.unscaledDeltaTime * speed;
                if (frame)
                    frame.color = Color.Lerp(Color.black, target, Mathf.PingPong(t, 1f));
                yield return null;
            }
        }
    }
}
