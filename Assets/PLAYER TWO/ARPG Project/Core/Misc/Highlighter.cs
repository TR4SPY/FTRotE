using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Misc/Highlighter")]
    public class Highlighter : MonoBehaviour
    {
        public UnityEvent<bool> onSetHighlight;

        [Header("Highlight Settings")]
        [Range(0f, 1f)]
        [Tooltip("The maximum intensity of the highlighting color.")]
        public float maxIntensity = 1f;

        [Tooltip("The property of the material to change when highlighting.")]
        public string propertyName = "_Emission";

        /// <summary>
        /// Returns the current highlighting state of this object.
        /// </summary>
        public bool highlighted { get; protected set; }

        protected List<Renderer> m_renderers;
        protected MaterialPropertyBlock m_properties;

        protected virtual void InitializeRenderers()
        {
            m_renderers = new List<Renderer>(GetComponentsInChildren<Renderer>());
            m_properties = new MaterialPropertyBlock();
        }

        protected virtual void SetMaterialsEmission(float value)
        {
            foreach (var renderer in m_renderers)
            {
                renderer.GetPropertyBlock(m_properties);

                if (m_properties != null)
                {
                    m_properties.SetFloat(propertyName, value);
                    renderer.SetPropertyBlock(m_properties);
                }
            }
        }

        /// <summary>
        /// Sets if the object is highlighted.
        /// </summary>
        /// <param name="value">The highlighting state of this object.</param>
        public virtual void SetHighlight(bool value)
        {
            if (highlighted == value || m_renderers == null) return;

            SetMaterialsEmission(value ? maxIntensity : 0);
            highlighted = value;
            onSetHighlight.Invoke(value);
        }

        protected virtual void Start() => InitializeRenderers();
    }
}
