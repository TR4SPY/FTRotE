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

        /*
        protected override void OnInteract(object other)
        {
            if (other is Entity && TryCollect((other as Entity).inventory.instance))
                Collect(other);
        }
        */

        protected override void OnInteract(object other)
        {
            // Sprawdzenie, czy obiekt 'other' nie jest null
            if (other == null)
            {
                Debug.LogError("Collectible.OnInteract: 'other' is null!");
                return;
            }

            // Sprawdzenie, czy obiekt 'other' jest typu Entity
            if (other is not Entity entity)
            {
                Debug.LogError("Collectible.OnInteract: 'other' is not an Entity!");
                return;
            }

            // Sprawdzenie, czy inventory i inventory.instance nie są null
            if (entity.inventory == null || entity.inventory.instance == null)
            {
                Debug.LogError("Collectible.OnInteract: Inventory or Inventory.Instance is null!");
                return;
            }

            // Jeśli wszystkie warunki są spełnione, wykonaj interakcję
            if (TryCollect(entity.inventory.instance))
            {
                Collect(other);
            }
        }

        /// <summary>
        /// Returns the name of the Item on the Collectible.
        /// </summary>
        public abstract string GetName();

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
            InitializeCanvas();
            StartLifetimeTimer();
        }
    }
}
