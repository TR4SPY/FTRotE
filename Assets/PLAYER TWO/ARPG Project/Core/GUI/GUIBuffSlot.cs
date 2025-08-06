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
        [Tooltip("A reference to the Image component used as the buff cool down image.")]
        public Image coolDownImage;

        [Tooltip("A reference to the Image component used as the slot frame.")]
        public Image frame;
    
        [Tooltip("Canvas Group used to control the slot transparency.")]
        public CanvasGroup canvasGroup;

        [Header("Pulse Settings")]
        [Tooltip("Pulse speed used for buffs.")]
        public float buffPulseSpeed = 1f;

        [Tooltip("Pulse speed used for debuffs.")]
        public float debuffPulseSpeed = 2f;

        [Tooltip("Frame target color for buffs.")]
        public Color buffPulseColor = GameColors.Green;

        [Tooltip("Frame target color for debuffs.")]
        public Color debuffPulseColor = GameColors.LightRed;

        [Tooltip("Curve describing the pulse intensity over time.")]
        public AnimationCurve heartbeatCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Slot Events")]
        public UnityEvent onIconClick;
        public UnityEvent onIconDoubleClick;

        protected GUISkillIcon m_icon;
        protected Coroutine m_coolDownRoutine;
        protected Coroutine m_pulseRoutine;
        protected Coroutine m_expiryFadeRoutine;
        protected float m_expiryTimeLeft;

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
                var target = isDebuff ? debuffPulseColor : buffPulseColor;
                var speed = isDebuff ? debuffPulseSpeed : buffPulseSpeed;
                frame.color = Color.Lerp(Color.black, target, Mathf.PingPong(Time.unscaledTime * speed, 1f));
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
        public virtual void BeginExpiryFade(float timeLeft)
        {
            m_expiryTimeLeft = timeLeft;
            if (m_expiryFadeRoutine == null)
            {
                m_expiryFadeRoutine = StartCoroutine(ExpiryFadeRoutine());
            }
        }

        public virtual void StopExpiryFade()
        {
            if (m_expiryFadeRoutine != null)
            {
                StopCoroutine(m_expiryFadeRoutine);
                m_expiryFadeRoutine = null;
            }

            SetAlpha(1f);
        }

        protected IEnumerator ExpiryFadeRoutine()
        {
            float t = 0f;
            while (true)
            {
                var strength = Mathf.Clamp01((10f - m_expiryTimeLeft) / 10f);
                t += Time.unscaledDeltaTime * strength;
                var alpha = Mathf.Lerp(1f, 1f - strength, Mathf.PingPong(t, 1f));
                SetAlpha(alpha);
                yield return null;
            }
        }

        protected void SetAlpha(float value)
        {
            if (canvasGroup)
            {
                canvasGroup.alpha = value;
            }
            else if (icon)
            {
                var c = icon.image.color;
                c.a = value;
                icon.image.color = c;
            }
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

            StopExpiryFade();

            if (frame)
                frame.color = Color.black;
        }

        protected IEnumerator PulseFrame(bool isDebuff)
        {
            var target = isDebuff ? debuffPulseColor : buffPulseColor;
            var speed = isDebuff ? debuffPulseSpeed : buffPulseSpeed;

            while (true)
            {
                var phase = Mathf.PingPong(Time.unscaledTime * speed, 1f);
                if (frame)
                    frame.color = Color.Lerp(Color.black, target, phase);
                    yield return null;
            }
        }
    }
}
