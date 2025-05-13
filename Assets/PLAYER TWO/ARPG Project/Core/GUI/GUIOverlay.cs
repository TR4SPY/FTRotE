using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public class GUIOverlay : MonoBehaviour
    {
        [Tooltip("Message life time (within overlay) in seconds.")]
        public float messageLifetime = 5f;

        private class OverlayEntry
        {
            public GameObject obj;
            public float expireAt;
            public bool fading;
        }

        private readonly Dictionary<GameObject, OverlayEntry> messages = new();

        void Update()
        {
            float now = Time.time;

            var toFade = new List<GameObject>();

            foreach (var kvp in messages)
            {
                var entry = kvp.Value;

                if (entry.obj == null)
                {
                    toFade.Add(kvp.Key); // null-safe cleanup
                    continue;
                }

                if (!entry.fading && now >= entry.expireAt)
                {
                    entry.fading = true;
                    StartCoroutine(FadeAndDestroy(entry));
                }
            }

            foreach (var key in toFade)
                messages.Remove(key);
        }

        public void FadeMessage(GameObject obj)
        {
            if (obj == null) return;

            if (!messages.ContainsKey(obj))
            {
                var cg = obj.GetComponent<CanvasGroup>() ?? obj.AddComponent<CanvasGroup>();
                cg.alpha = 1f;

                messages[obj] = new OverlayEntry
                {
                    obj = obj,
                    expireAt = Time.time + messageLifetime,
                    fading = false
                };

                // Debug.Log($"[GUIOverlay] FadeMessage: registered {obj.name}, expires at {messages[obj].expireAt:F2}");
            }
            else
            {
                messages[obj].expireAt = Time.time + messageLifetime;
                messages[obj].fading = false;

                // Debug.Log($"[GUIOverlay] FadeMessage: updated {obj.name} with new expireAt {messages[obj].expireAt:F2}");
            }
        }

        public void RegisterMessageWithoutFade(GameObject obj)
        {
            if (obj == null) return;

            if (!messages.ContainsKey(obj))
            {
                var cg = obj.GetComponent<CanvasGroup>() ?? obj.AddComponent<CanvasGroup>();
                cg.alpha = 1f;

                messages[obj] = new OverlayEntry
                {
                    obj = obj,
                    expireAt = float.PositiveInfinity,
                    fading = false
                };

                // Debug.Log($"[GUIOverlay] Registered historical message {obj.name}, no fade yet");
            }
            else
            {
                // Debug.Log($"[GUIOverlay] Historical message {obj.name} already tracked");
            }
        }

        public void ResumeFadeForAllVisible()
        {
            // Debug.Log($"[GUIOverlay - DEBUG] ResumeFadeForAllVisible() called. Messages tracked: {messages.Count}");

            float now = Time.time;

            foreach (var kvp in messages)
            {
                var entry = kvp.Value;

                // Debug.Log($"[GUIOverlay - DEBUG] Checking {entry.obj?.name}, fading={entry.fading}, expireAt={entry.expireAt}");

                if (entry.obj != null && !entry.fading && float.IsInfinity(entry.expireAt))
                {
                    entry.expireAt = Time.time + messageLifetime;
                    // Debug.Log($"[GUIOverlay - DEBUG] Set {entry.obj.name} to expire at {entry.expireAt:F2}");
                }
            }
        }

        private IEnumerator FadeAndDestroy(OverlayEntry entry)
        {
            if (entry.obj == null) yield break;

            var cg = entry.obj.GetComponent<CanvasGroup>();
            if (cg == null) yield break;

            // Debug.Log($"[GUIOverlay] Fading out {entry.obj.name}");

            float duration = 1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                cg.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            cg.alpha = 0f;
            Destroy(entry.obj);

            messages.Remove(entry.obj);
            // Debug.Log($"[GUIOverlay] Destroyed {entry.obj.name}");
        }
    }
}
