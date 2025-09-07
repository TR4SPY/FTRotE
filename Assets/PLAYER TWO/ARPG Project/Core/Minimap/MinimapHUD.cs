using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Minimap/Minimap HUD")]
    public class MinimapHUD : Singleton<MinimapHUD>
    {
        [Header("General Settings")]
        [Tooltip("The Rect Transform to be used as parent of all icons.")]
        public RectTransform container;

        [Tooltip("The texture of the Minimap.")]
        public RawImage minimap;

        [Header("Movement Settings")]
        [Tooltip("The target that moves the Minimap.")]
        public Transform target;

        [Tooltip("The initial zoom amount of the Minimap.")]
        public float initialZoom = 3f;

        [Tooltip("The initial offset rotation applied to the Minimap.")]
        public float rotationOffset;

        [Tooltip("If true, the Minimap will also rotate with the target Y axis.")]
        public bool rotateWithTarget;

        protected readonly List<MinimapIcon> m_icons = new List<MinimapIcon>();
        protected WaitForSeconds m_coroutineWait = new WaitForSeconds(1f / 30f);
        Coroutine m_iconRoutine;

        protected virtual void InitializeTarget()
        {
            if (target) return;
            target = Level.instance.player.transform;
        }

        protected virtual void UpdateRect()
        {
            Vector2 position = Minimap.instance.WorldToMapPosition(target.position);
            minimap.uvRect = new Rect(position.x, position.y, 1f, 1f);
        }

        protected virtual void UpdateRotation()
        {
            float z = rotationOffset;
            if (rotateWithTarget)
                z = target.eulerAngles.y;
            minimap.rectTransform.eulerAngles = new Vector3(0f, 0f, z);
        }

        protected virtual void UpdateIcons()
        {
            Vector2 minimapSize = minimap.rectTransform.sizeDelta;
            Vector3 scale = minimap.rectTransform.localScale;
            Vector2 uvPosition = minimap.uvRect.position;

            float rotationZ = minimap.rectTransform.eulerAngles.z;
            float rad = rotationZ * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            foreach (var icon in m_icons)
            {
                if (icon == null || !icon.image.enabled)
                    continue;

                Vector2 localPos = Minimap.instance.WorldToMapPosition(icon.owner.position);
                Vector3 iconEuler = new Vector3(0f, 0f, icon.rotationOffset);

                Vector2 offset = uvPosition * minimapSize * (Vector2)scale;
                localPos *= minimapSize * (Vector2)scale;

                Vector2 relativePos = localPos - offset;
                Vector2 rotatedPos = new Vector2(
                    relativePos.x * cos - relativePos.y * sin,
                    relativePos.x * sin + relativePos.y * cos);
                icon.image.transform.localPosition = rotatedPos;

                if (icon.rotateWithOwner)
                {
                    iconEuler.z += rotationZ - icon.owner.eulerAngles.y;
                }

                icon.image.transform.localEulerAngles = iconEuler;
            }
        }

        /// <summary>
        /// Sets the zoom level of the minimap.
        /// </summary>
        public virtual void Rescale(float scale)
        {
            scale = Mathf.Max(0f, scale);
            minimap.rectTransform.localScale = Vector3.one * scale;
        }

        /// <summary>
        /// Changes the minimap texture.
        /// </summary>
        public virtual void SetTexture(Texture texture)
        {
            minimap.texture = texture;
        }

        /// <summary>
        /// Adds an icon to the minimap.
        /// </summary>
        public virtual void AddIcon(MinimapIcon icon)
        {
            if (m_icons.Contains(icon)) return;
            icon.image.transform.SetParent(container, false);
            m_icons.Add(icon);
        }

        protected virtual IEnumerator UpdateIconsRoutine()
        {
            while (true)
            {
                UpdateIcons();
                yield return m_coroutineWait;
            }
        }

        protected virtual void Start()
        {
            InitializeTarget();
            SetTexture(Minimap.instance.minimapTexture);
            float scale = GameSettings.instance ? GameSettings.instance.GetMinimapSize() : initialZoom;
            Rescale(scale);
            if (GameSettings.instance)
                rotateWithTarget = GameSettings.instance.GetMinimapRotation();

            m_iconRoutine = StartCoroutine(UpdateIconsRoutine());

            var window = GUIWindowsManager.instance?.minimapWindow;
            if (window != null)
            {
                window.onOpen.AddListener(() =>
                {
                    window.GetComponent<RectTransform>().SetAsFirstSibling();
                });
            }
        }

        protected virtual void LateUpdate()
        {
            UpdateRect();
            UpdateRotation();
        }

        protected virtual void OnEnable()
        {
            if (m_iconRoutine != null)
                StopCoroutine(m_iconRoutine);
            m_iconRoutine = StartCoroutine(UpdateIconsRoutine());
        }

        protected virtual void OnDisable()
        {
            if (m_iconRoutine != null)
                StopCoroutine(m_iconRoutine);
        }

        /// <summary>
        /// Allows changing the follow target at runtime.
        /// </summary>
        public void SetTarget(Entity entity)
        {
            if (entity == null) return;
            target = entity.transform;
        }
    }
}
