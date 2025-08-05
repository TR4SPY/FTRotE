using System.Collections.Generic;
using UnityEngine;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    /// <summary>
    /// Handles spawning and cleanup of particle systems for active buffs on the player.
    /// </summary>
    public class BuffParticle : MonoBehaviour
    {
        [System.Serializable]
        public class BuffParticleMapping
        {
            [Tooltip("Name of the buff to match.")]
            public string buffName;
            [Tooltip("Particle system prefab to spawn when the buff is applied.")]
            public ParticleSystem particlePrefab;
        }

        [Tooltip("Particles to spawn for specific buffs.")]
        public List<BuffParticleMapping> buffParticles = new();

        protected EntityBuffManager m_buffManager;
        protected readonly Dictionary<BuffInstance, ParticleSystem> m_activeParticles = new();

        protected virtual void Awake()
        {
            m_buffManager = GetComponent<EntityBuffManager>();
            if (m_buffManager)
            {
                m_buffManager.onBuffAdded.AddListener(OnBuffAdded);
                m_buffManager.onBuffRemoved.AddListener(OnBuffRemoved);
            }
        }

        protected virtual void OnDestroy()
        {
            if (m_buffManager)
            {
                m_buffManager.onBuffAdded.RemoveListener(OnBuffAdded);
                m_buffManager.onBuffRemoved.RemoveListener(OnBuffRemoved);
            }
        }

        protected virtual void OnBuffAdded(BuffInstance instance)
        {
            var prefab = GetParticlePrefab(instance.buff.name);
            if (!prefab) return;

            var particle = Instantiate(prefab, transform);
            m_activeParticles[instance] = particle;
        }

        protected virtual void OnBuffRemoved(BuffInstance instance)
        {
            if (!m_activeParticles.TryGetValue(instance, out var particle))
                return;

            if (particle)
            {
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                Destroy(particle.gameObject);
            }

            m_activeParticles.Remove(instance);
        }

        protected ParticleSystem GetParticlePrefab(string buffName)
        {
            foreach (var mapping in buffParticles)
            {
                if (mapping.buffName == buffName)
                    return mapping.particlePrefab;
            }
            return null;
        }
    }
}
