using UnityEngine;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Misc/Flash Reduction")]
    public class FlashReduction : MonoBehaviour
    {
        static readonly List<FlashReduction> s_instances = new();

        public ParticleSystem[] particles;
        public Light[] lights;

        void OnEnable()
        {
            s_instances.Add(this);
            Apply();
        }

        void OnDisable()
        {
            s_instances.Remove(this);
        }

        public void Apply()
        {
            float mult = GameSettings.instance ? GameSettings.instance.GetFlashReductionMultiplier() : 1f;

            if (particles != null)
            {
                foreach (var p in particles)
                {
                    if (!p) continue;
                    var main = p.main;
                    if (main.startColor.mode == ParticleSystemGradientMode.Color)
                    {
                        Color c = main.startColor.color * mult;
                        main.startColor = new ParticleSystem.MinMaxGradient(c);
                    }
                }
            }

            if (lights != null)
            {
                foreach (var l in lights)
                {
                    if (!l) continue;
                    l.intensity *= mult;
                }
            }
        }

        public static void ApplyAll()
        {
            foreach (var inst in s_instances)
                inst.Apply();
        }
    }
}
