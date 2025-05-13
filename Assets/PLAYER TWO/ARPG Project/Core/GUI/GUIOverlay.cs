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
            public Coroutine fadeCoroutine;
        }

        private readonly Dictionary<GameObject, OverlayEntry> messages = new();
        private readonly HashSet<GameObject> hoveredMessages = new();

        void Update()
        {
            float now = Time.time;

            foreach (var kvp in messages)
            {
                var entry = kvp.Value;

                if (entry.obj == null) continue;
                if (entry.fading) continue;
                if (hoveredMessages.Contains(entry.obj)) continue;

                if (now >= entry.expireAt)
                {
                    entry.fading = true;
                    entry.fadeCoroutine = StartCoroutine(FadeAndDestroy(entry));
                }
            }
        }

        public void FadeMessage(GameObject obj)
        {
            if (obj == null) return;

            var cg = obj.GetComponent<CanvasGroup>() ?? obj.AddComponent<CanvasGroup>();
            cg.alpha = 1f;

            if (!messages.ContainsKey(obj))
            {
                messages[obj] = new OverlayEntry
                {
                    obj = obj,
                    expireAt = Time.time + messageLifetime,
                    fading = false,
                    fadeCoroutine = null
                };
            }
            else
            {
                var entry = messages[obj];
                entry.expireAt = Time.time + messageLifetime;
                entry.fading = false;

                if (entry.fadeCoroutine != null)
                {
                    StopCoroutine(entry.fadeCoroutine);
                    entry.fadeCoroutine = null;
                }

                cg.alpha = 1f;
            }
        }

        public void RegisterMessageWithoutFade(GameObject obj)
        {
            if (obj == null) return;

            var cg = obj.GetComponent<CanvasGroup>() ?? obj.AddComponent<CanvasGroup>();
            cg.alpha = 1f;

            messages[obj] = new OverlayEntry
            {
                obj = obj,
                expireAt = float.PositiveInfinity,
                fading = false,
                fadeCoroutine = null
            };
        }

        public void ResumeFadeForAllVisible()
        {
            float now = Time.time;

            foreach (var kvp in messages)
            {
                var entry = kvp.Value;

                if (entry.obj != null && !entry.fading && float.IsInfinity(entry.expireAt))
                {
                    entry.expireAt = now + messageLifetime;
                }
            }
        }

        public void PauseFade(GameObject obj)
        {
            if (obj == null || !messages.TryGetValue(obj, out var entry)) return;

            hoveredMessages.Add(obj);

            if (entry.fadeCoroutine != null)
            {
                StopCoroutine(entry.fadeCoroutine);
                entry.fadeCoroutine = null;
            }

            entry.fading = false;
            entry.expireAt = float.PositiveInfinity;

            var cg = obj.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
        }

        public void ResumeFade(GameObject obj)
        {
            if (obj == null || !messages.TryGetValue(obj, out var entry)) return;

            hoveredMessages.Remove(obj);

            entry.expireAt = Time.time + messageLifetime;
            entry.fading = false;

            var cg = obj.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
        }

        public void PauseAllFade()
        {
            foreach (var entry in messages.Values)
            {
                if (entry.obj != null)
                {
                    if (entry.fadeCoroutine != null)
                    {
                        StopCoroutine(entry.fadeCoroutine);
                        entry.fadeCoroutine = null;
                    }

                    entry.expireAt = float.PositiveInfinity;
                    entry.fading = false;

                    var cg = entry.obj.GetComponent<CanvasGroup>();
                    if (cg != null) cg.alpha = 1f;

                    hoveredMessages.Add(entry.obj);
                }
            }
        }

        public void ResumeAllFade()
        {
            float now = Time.time;

            foreach (var entry in messages.Values)
            {
                if (entry.obj != null)
                {
                    entry.expireAt = now + messageLifetime;
                    entry.fading = false;

                    var cg = entry.obj.GetComponent<CanvasGroup>();
                    if (cg != null) cg.alpha = 1f;

                    hoveredMessages.Remove(entry.obj);
                }
            }
        }

        private IEnumerator FadeAndDestroy(OverlayEntry entry)
        {
            if (entry.obj == null) yield break;

            var cg = entry.obj.GetComponent<CanvasGroup>();
            if (cg == null) yield break;

            float duration = 1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (hoveredMessages.Contains(entry.obj))
                {
                    cg.alpha = 1f;
                    entry.fading = false;
                    entry.expireAt = Time.time + messageLifetime;
                    entry.fadeCoroutine = null;
                    yield break;
                }

                cg.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            cg.alpha = 0f;
            messages.Remove(entry.obj);
            Destroy(entry.obj);
        }
    }
}
