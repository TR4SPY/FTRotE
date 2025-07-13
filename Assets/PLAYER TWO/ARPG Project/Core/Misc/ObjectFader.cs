using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public class ObjectFader : MonoBehaviour
    {
        [Header("Ustawienia gracza")]
        public Transform target;
        public string playerTag = "Entity/Player";

        [Header("Drzewa")]
        public Transform convertedTreesRoot;

        [Header("Efekt zanikania")]
        public float fadeDuration = 1.0f;
        public float transparentAlpha = 0.2f;

        [Header("Obszar widoku")]
        public float screenMargin = 0.15f; // od 0.1 (ciasno) do 0.3 (szeroko)

        private Dictionary<Renderer, Coroutine> fadingObjects = new();
        private HashSet<Renderer> currentlyFaded = new();

        void Update()
        {
            if (target == null)
            {
                GameObject found = GameObject.FindWithTag(playerTag);
                if (found != null)
                {
                    target = found.transform;
                }
                else
                {
                    return;
                }
            }

            if (convertedTreesRoot == null) return;
            if (Camera.main == null) return;

            HashSet<Renderer> hitRenderers = new();

            Vector3 playerViewportPos = Camera.main.WorldToViewportPoint(target.position);

            Rect playerRect = new Rect(
                playerViewportPos.x - screenMargin,
                playerViewportPos.y - screenMargin,
                screenMargin * 2,
                screenMargin * 2
            );

            foreach (Renderer rend in convertedTreesRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (!rend.enabled || rend.bounds.size.magnitude <= 0.1f) continue;

                Bounds bounds = rend.bounds;

                Vector3[] checkPoints = new Vector3[]
                {
                    bounds.center,
                    bounds.min,
                    bounds.max
                };

                bool shouldFade = false;

                foreach (var point in checkPoints)
                {
                    Vector3 viewportPos = Camera.main.WorldToViewportPoint(point);
                    if (viewportPos.z < 0f) continue; // za kamerą

                    if (playerRect.Contains(viewportPos))
                    {
                        shouldFade = true;
                        break;
                    }
                }

                hitRenderers.Add(rend);

                if (shouldFade)
                {
                    if (!currentlyFaded.Contains(rend))
                    {
                        StartFade(rend, transparentAlpha);
                    }
                }
                else
                {
                    if (currentlyFaded.Contains(rend))
                    {
                        StartFade(rend, 1f);
                    }
                }
            }
        }

        void StartFade(Renderer rend, float targetAlpha)
        {
            Material[] mats = rend.materials;

            for (int i = 0; i < mats.Length; i++)
            {
                if (!mats[i].name.EndsWith("(Instance)"))
                {
                    mats[i] = new Material(mats[i]);
                    SetupURPTransparent(mats[i]);
                }
            }

            rend.materials = mats;

            if (fadingObjects.TryGetValue(rend, out Coroutine existing))
            {
                StopCoroutine(existing);
            }

            Coroutine c = StartCoroutine(FadeCoroutine(rend, targetAlpha));
            fadingObjects[rend] = c;
        }

        IEnumerator FadeCoroutine(Renderer rend, float targetAlpha)
        {
            currentlyFaded.Add(rend);

            Material[] mats = rend.materials;
            float time = 0f;
            float startAlpha = mats[0].color.a;

            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                float t = time / fadeDuration;
                float a = Mathf.Lerp(startAlpha, targetAlpha, t);

                foreach (var mat in mats)
                {
                    Color c = mat.color;
                    c.a = a;
                    mat.color = c;
                }

                yield return null;
            }

            foreach (var mat in mats)
            {
                Color c = mat.color;
                c.a = targetAlpha;
                mat.color = c;
            }

            if (Mathf.Approximately(targetAlpha, 1f))
            {
                currentlyFaded.Remove(rend);
            }
        }

        void SetupURPTransparent(Material mat)
        {
            if (mat.HasProperty("_Surface"))
                mat.SetFloat("_Surface", 1); // Transparent

            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
    }
}
