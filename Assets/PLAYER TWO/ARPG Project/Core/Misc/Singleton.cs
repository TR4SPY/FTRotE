using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public class Singleton<T> : MonoBehaviour
        where T : MonoBehaviour
    {
        protected static T m_instance;

        public static T instance
        {
            get
            {
                if (!m_instance)
                {
#if UNITY_6000_0_OR_NEWER
                    m_instance = FindFirstObjectByType<T>();
#else
                    m_instance = FindObjectOfType<T>();
#endif
                    if (!m_instance)
                    {
                        Debug.LogWarning($"Singleton<{typeof(T).Name}> instance not found!");
                    }
                }

                return m_instance;
            }
        }

        protected bool m_initialized;

        protected virtual void Initialize() { }

        protected virtual void Awake()
        {
            if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
            else if (!m_initialized)
            {
                Initialize();
                m_initialized = true;
                // Debug.Log($"Singleton<{typeof(T).Name}> initialized.");
            }
        }
    }
}
