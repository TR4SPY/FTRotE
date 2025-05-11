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

        protected readonly List<MinimapIcon> m_icons = new();

        protected WaitForSeconds m_coroutineWait = new(1f / 30);

        protected virtual void InitializeTarget()
        {
            if (target) return;

            target = Level.instance.player.transform;
        }

        protected virtual void UpdateRect()
        {
            var position = Minimap.instance.WorldToMapPosition(target.position);
            minimap.uvRect = new Rect(position.x, position.y, 1, 1);
        }

        protected virtual void UpdateRotation()
        {
            var eulerAngles = new Vector3(0, 0, rotationOffset);

            if (rotateWithTarget)
            {
                eulerAngles.z = target.eulerAngles.y;
            }

            minimap.transform.eulerAngles = eulerAngles;
        }

        protected virtual void UpdateIcons()
        {
            foreach (var icon in m_icons)
            {
                if (!icon || !icon.image.enabled) continue;

                var minimapSize = minimap.rectTransform.sizeDelta;
                var minimapScale = minimap.rectTransform.localScale;
                var minimapRectPosition = minimap.uvRect.position;

                var localPosition = Minimap.instance.WorldToMapPosition(icon.owner.position);
                var iconEulerAngles = new Vector3(0, 0, icon.rotationOffset);
                var offset = minimapRectPosition * minimapSize * minimapScale;

                localPosition *= minimapSize * minimapScale;

                if (icon.rotateWithOwner)
                {
                    iconEulerAngles.z -= icon.owner.eulerAngles.y - rotationOffset;

                    if (rotateWithTarget)
                    {
                        iconEulerAngles.z += container.eulerAngles.z - rotationOffset;
                    }
                }

                icon.image.transform.localPosition = localPosition - offset;
                icon.image.transform.eulerAngles = iconEulerAngles;
            }
        }

        /// <summary>
        /// Change the scale, zoom level, of the minimap texture.
        /// </summary>
        public virtual void Rescale(float scale)
        {
            scale = Mathf.Max(0, scale);
            minimap.rectTransform.localScale = Vector3.one * scale;
        }

        /// <summary>
        /// Set a new texture to the minimap.
        /// </summary>
        public virtual void SetTexture(Texture texture)
        {
            minimap.texture = texture;
        }

        /// <summary>
        /// Add a new Icon to the minimap renderer.
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
            Rescale(initialZoom);
            StartCoroutine(UpdateIconsRoutine());
        }

        protected virtual void LateUpdate()
        {
            UpdateRect();
            UpdateRotation();
        }
    }
}
