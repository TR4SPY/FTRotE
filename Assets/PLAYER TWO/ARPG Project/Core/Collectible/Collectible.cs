using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(Collider))]
    public abstract class Collectible : Interactive
    {
        [Header("GUI Name Settings")]
        [Tooltip("The GUI Collectible prefab that represents this Collectible name on the GUI.")]
        public GUICollectibleName guiName;

        [Tooltip("The color of the text on the GUI Collectible.")]
        public Color nameColor = Color.white;

        [Header("Collectible Items Settings")]
        [Tooltip("Time (in seconds) before the collectible is automatically removed.")]
        public float lifetime = 30f;

        [HideInInspector]
        public bool suppressNameDisplay = false;

        protected virtual void InitializeCanvas()
        {
            var gui = Instantiate(guiName);
            gui.SetCollectible(this, nameColor);
        }

        protected override void InitializeTag() => tag = GameTags.Collectible;

        /// <summary>
        /// Collects this Collectible.
        /// </summary>
        /// <param name="other">The object that is collecting this Collectible.</param>
        public virtual void Collect(object other) => Destroy(gameObject);

        protected override void OnInteract(object other)
        {
            if (other == null)
            {
                Debug.LogError("Collectible.OnInteract: 'other' is null!");
                return;
            }

            if (other is not Entity entity)
            {
                Debug.LogError("Collectible.OnInteract: 'other' is not an Entity!");
                return;
            }

            if (entity.inventory == null || entity.inventory.instance == null)
            {
                Debug.LogError("Collectible.OnInteract: Inventory or Inventory.Instance is null!");
                return;
            }

            if (TryCollect(entity.inventory.instance))
            {
                Collect(other);
            }
        }

        /// <summary>
        /// Returns the name of the item (or items) on this Collectible.
        /// </summary>
        public abstract string GetName();

        /// <summary>
        /// Attempts to add this Collectible's contents to the given inventory.
        /// </summary>
        protected abstract bool TryCollect(Inventory inventory);

        private void StartLifetimeTimer()
        {
            Invoke(nameof(DestroyCollectible), lifetime);
        }

        private void DestroyCollectible()
        {
            Debug.Log($"[Collectible] {gameObject.name} has been removed after {lifetime} seconds.");
            Destroy(gameObject);
        }

        protected override void Start()
        {
            base.Start();

            if (!suppressNameDisplay)
            {
                InitializeCanvas();
            }

            StartLifetimeTimer();
        }
    }
}
