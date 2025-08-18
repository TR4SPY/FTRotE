namespace PLAYERTWO.ARPGProject
{
    public partial class ItemInstance
    {
        /// <summary>
        /// Indicates whether this item is sealed due to unmet requirements.
        /// </summary>
        public bool isSealed { get; private set; }

        /// <summary>
        /// Represents the current effectiveness of this item (0-1 range).
        /// </summary>
        public float effectiveness { get; private set; } = 1f;

        /// <summary>
        /// Evaluates item requirements against an entity and updates seal state and effectiveness.
        /// </summary>
        public void EvaluateRequirements(Entity entity)
        {
            bool meets = MeetsRequirements(entity);
            isSealed = !meets;
            effectiveness = meets ? 1f : 0f;
        }
    }
}