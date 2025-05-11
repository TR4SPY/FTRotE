using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Minimap/Minimap Icon")]
    public class MinimapIcon : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("The sprite that represents the Icon on the Minimap.")]
        public Sprite iconSprite;

        [Tooltip("If true, the sprite starts visible on the Minimap.")]
        public bool initialVisibility = true;

        [Tooltip("The initial size of the icon.")]
        public Vector2 initialSize = new Vector2(32, 32);

        [Tooltip("A rotation offset applied to the icon on the Minimap to adjust its orientation.")]
        public float rotationOffset;

        [Tooltip("If true, the Icon will adjust its rotation based on the target Y rotation.")]
        public bool rotateWithOwner = false;

        /// <summary>
        /// Returns the Image component of the Minimap Icon.
        /// </summary>
        public Image image { get; protected set; }

        /// <summary>
        /// The transform that the icon is attached to.
        /// </summary>
        public Transform owner => transform;

        protected virtual void InitializeEntity()
        {
            if (TryGetComponent(out Entity entity))
            {
                entity.onDie.AddListener(() =>
                {
                    SetVisibility(false);
                });
            }
        }

        protected virtual void InitializeImage()
        {
            image = new GameObject().AddComponent<Image>();
            image.sprite = iconSprite;
            image.SetNativeSize();
        }

        /// <summary>
        /// Change how big the icon is in the minimap.
        /// </summary>
        public virtual void Rescale(Vector2 size)
        {
            image.rectTransform.sizeDelta = size;
        }

        /// <summary>
        /// Set the visibility of the icon.
        /// </summary>
        public virtual void SetVisibility(bool value)
        {
            if (!image) return;

            image.enabled = value;
        }

        public virtual void AddIconToMinimap() => MinimapHUD.instance.AddIcon(this);

        protected virtual void Awake()
        {
            InitializeEntity();
            InitializeImage();
            Rescale(initialSize);
            SetVisibility(initialVisibility);
            AddIconToMinimap();

            if (TryGetComponent(out Entity entity))
            {
                entity.onRevive.AddListener(() =>
                {
                    SetVisibility(true);
                    AddIconToMinimap();
                });
            }
        }

        protected virtual void OnEnable() => SetVisibility(true);
        protected virtual void OnDisable() => SetVisibility(false);
    }
}
