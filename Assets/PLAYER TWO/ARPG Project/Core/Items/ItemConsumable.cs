using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public abstract class ItemConsumable : Item
    {
        [Header("Consumable Settings")]
        [Tooltip("The sound played when this Consumable is consumed.")]
        public AudioClip consumeSound;

        [Tooltip("The Particle System played when this Consumable is consumed.")]
        public ParticleSystem consumeParticles;

        [Tooltip("An offset for the Particle System played when this Consumable is consumed.")]
        public Vector3 particleOffset;

        [Tooltip("Buffs applied when this Consumable is consumed.")]
        public Buff[] appliedBuffs;

        /// <summary>
        /// Consumes the item and applies its effects to the given entity.
        /// </summary>
        /// <param name="entity">The Entity you want to apply the effects to.</param>
        public virtual void Consume(Entity entity)
        {
            if (consumeParticles)
            {
                var particlePosition = entity.position + particleOffset;
                Instantiate(
                        consumeParticles,
                        particlePosition,
                        Quaternion.identity,
                        entity.transform
                    )
                    .Play();
            }

            if (consumeSound && GameAudio.instance)
                GameAudio.instance.PlayEffect(consumeSound);
        }

        protected void ApplyBuffs(Entity entity)
        {
            if (appliedBuffs == null || appliedBuffs.Length == 0)
                return;

            var manager = entity.GetComponent<EntityBuffManager>();
            if (!manager)
                return;

            foreach (var buff in appliedBuffs)
                manager.AddBuff(buff);
        }
    }
}
