using UnityEngine;
using System.Collections;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Misc/Shaker")]
    public class Shaker : MonoBehaviour
    {
        [Header("General Settings")]
        [Tooltip("The transform you want to shake.")]
        public Transform target;

        [Tooltip("The duration in seconds to keep shaking the transform.")]
        public float shakeDuration = 0.1f;

        [Tooltip("The magnitude of the shaking movement.")]
        public float shakeMagnitude = 0.15f;

        protected Vector3 originalPosition;
        protected bool shakeEnabled = true;

        protected virtual void Awake()
        {
            originalPosition = target.localPosition;
        }

        /// <summary>
        /// Starts shaking the target transform with the regular shaking settings.
        /// </summary>
        public virtual void Shake()
        {
            if (!gameObject.activeSelf || !shakeEnabled) return;

            StopAllCoroutines();
            StartCoroutine(ShakeCoroutine());
        }

        public virtual void SetShakeEnabled(bool enabled)
        {
            shakeEnabled = enabled;
            if (!enabled && target != null)
            {
                StopAllCoroutines();
                target.localPosition = originalPosition;
            }
        }

        protected IEnumerator ShakeCoroutine()
        {
            float elapsedTime = 0f;

            while (elapsedTime < shakeDuration)
            {
                Vector3 shakeOffset = Random.insideUnitSphere * shakeMagnitude;
                target.localPosition = originalPosition + shakeOffset;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            target.localPosition = originalPosition;
        }
    }
}
