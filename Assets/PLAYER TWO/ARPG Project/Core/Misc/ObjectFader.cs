using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fade obiektów, które REALNIE zasłaniają gracza z perspektywy kamery.
/// Wykorzystuje Physics.SphereCastAll od kamery do gracza (XZ/Y – w 3D),
/// filtruje trafienia do dzieci "convertedRoot" i płynnie je ukrywa:
/// - preferencyjnie przez modyfikację _Cutoff (Alpha Clip),
/// - jeśli materiał nie ma _Cutoff, fallback na zmianę kanału alpha koloru.
/// Dodatkowo włącza/wyłącza child "Stump".
/// </summary>
[DisallowMultipleComponent]
public class CameraOcclusionFaderRaycast : MonoBehaviour
{
    [Header("Cel (gracz)")]
    public Transform target;
    public string playerTag = "Entity/Player";
    [Tooltip("Offset w górę, np. wysokość klatki piersiowej gracza (m).")]
    public float targetHeightOffset = 1.2f;

    [Header("Rodzic obiektów do chowania")]
    public Transform convertedRoot;                  // rodzic wszystkich drzew / obiektów
    [Tooltip("Nazwa childa pnia (opcjonalnie).")]
    public string stumpChildName = "Stump";

    [Header("Raycast")]
    [Tooltip("Warstwy uznawane za zasłaniające (drzewa/rośliny).")]
    public LayerMask occluderLayers;
    [Tooltip("Promień sferycznego raycastu (im większy, tym „grubsza” wiązka).")]
    public float sphereRadius = 0.25f;
    [Tooltip("Maksymalna liczba trafień branych pod uwagę.")]
    public int maxHits = 128;
    [Tooltip("Ile klatek utrzymać ukrycie po zniknięciu z wiązki (anty-migotanie).")]
    public int holdFramesAfterMiss = 3;

    [Header("Fade")]
    [Tooltip("Czas płynnego przejścia.")]
    public float fadeDuration = 0.25f;
    [Tooltip("Docelowa przeźroczystość przy ukryciu: 0 = maks. ukrycie, 1 = brak ukrycia.")]
    [Range(0f, 1f)] public float hiddenAlpha = 0f;

    [Header("Alpha Clip (Gaia/Procedural Worlds)")]
    [Tooltip("Preferowana nazwa progu wycinania w shaderze (PW: _Cutoff).")]
    public string cutoffPropertyName = "_Cutoff";
    [Tooltip("Próg w stanie normalnym (zgodny z materiałem).")]
    [Range(0f, 1f)] public float visibleCutoff = 0.5f;
    [Tooltip("Próg w stanie ukrycia (im wyżej, tym mniej liści).")]
    [Range(0f, 1f)] public float hiddenCutoff = 0.97f;

    [Header("Debug (opcjonalnie)")]
    public bool drawGizmos = false;
    public Color gizmoLine = new Color(0, 0.8f, 1f, 0.7f);
    public Color gizmoHit = new Color(1f, 0.2f, 0.2f, 0.8f);

    // ——————————————————————————————————————————————

    Camera _cam;
    int _cutoffID, _baseColorID, _colorID;

    // jeden „blok” = jedno dziecko convertedRoot (prefab drzewa/krzaka)
    readonly List<Block> _blocks = new List<Block>(256);
    // szybki lookup: collider -> block
    readonly Dictionary<Transform, Block> _transformToBlock = new Dictionary<Transform, Block>(256);
    // liczenie trzymania stanu po „miss” (anty-migotanie)
    readonly Dictionary<Block, int> _holdCounters = new Dictionary<Block, int>(128);

    readonly List<Block> _hitsThisFrame = new List<Block>(128);

    void Awake()
    {
        _cam = Camera.main;
        _cutoffID   = Shader.PropertyToID(cutoffPropertyName);
        _baseColorID= Shader.PropertyToID("_BaseColor");
        _colorID    = Shader.PropertyToID("_Color");
    }

    void Start()
    {
        if (!target)
        {
            var go = GameObject.FindWithTag(playerTag);
            if (go) target = go.transform;
        }

        RebuildCache();
    }

    void OnValidate()
    {
        _cutoffID = Shader.PropertyToID(cutoffPropertyName);
    }

    /// <summary>Zbiera bezpośrednie dzieci convertedRoot; mapuje ich collidery do bloków.</summary>
    public void RebuildCache()
    {
        _blocks.Clear();
        _transformToBlock.Clear();
        _holdCounters.Clear();

        if (!convertedRoot) return;

        for (int i = 0; i < convertedRoot.childCount; i++)
        {
            var root = convertedRoot.GetChild(i);
            if (!root) continue;

            var block = new Block(root, stumpChildName, _cutoffID, _baseColorID, _colorID, visibleCutoff, hiddenCutoff);
            // renderery (bez pnia)
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (!r) continue;
                if (block.stump && r.transform.IsChildOf(block.stump.transform)) continue;
                block.renderers.Add(r);
            }
            if (block.renderers.Count == 0) continue;

            // collidery
            var colliders = root.GetComponentsInChildren<Collider>(true);
            foreach (var c in colliders)
            {
                if (!c) continue;
                _transformToBlock[c.transform] = block;
            }

            _blocks.Add(block);
            _holdCounters[block] = 0;
        }
    }

    void Update()
    {
        if (!_cam || !target) return;

        _hitsThisFrame.Clear();

        Vector3 camPos = _cam.transform.position;
        Vector3 tgt = target.position + Vector3.up * targetHeightOffset;
        Vector3 dir = (tgt - camPos);
        float dist = dir.magnitude;
        if (dist <= 0.001f) return;
        dir /= dist;

        // SphereCastAll – realne zasłanianie między kamerą a graczem
        var hits = Physics.SphereCastAll(camPos, sphereRadius, dir, dist, occluderLayers, QueryTriggerInteraction.Ignore);
        int count = Mathf.Min(hits.Length, maxHits);

        for (int i = 0; i < count; i++)
        {
            var h = hits[i];
            var tr = h.collider ? h.collider.transform : null;
            if (!tr) continue;

            // podnieś do najbliższego dziecka convertedRoot
            var root = tr;
            while (root && root.parent != null && root.parent != convertedRoot) root = root.parent;

            if (!root) continue;

            if (_transformToBlock.TryGetValue(root, out var blk))
            {
                if (!_hitsThisFrame.Contains(blk))
                    _hitsThisFrame.Add(blk);
            }
        }

        // zastosuj fade: trafione -> hide; nietrafione -> show (z hold)
        for (int i = 0; i < _blocks.Count; i++)
        {
            var b = _blocks[i];

            if (_hitsThisFrame.Contains(b))
            {
                // trafiony – ukryj i ustaw hold
                b.FadeTo(hiddenAlpha, fadeDuration);
                b.ToggleStump(true);
                _holdCounters[b] = holdFramesAfterMiss;
            }
            else
            {
                // nietrafiony – odlicz hold
                if (_holdCounters[b] > 0)
                {
                    _holdCounters[b]--;
                }
                else
                {
                    b.FadeTo(1f, fadeDuration);
                    b.ToggleStump(false);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos || !_cam || !target) return;

        Vector3 camPos = _cam.transform.position;
        Vector3 tgt = target.position + Vector3.up * targetHeightOffset;
        Gizmos.color = gizmoLine;
        Gizmos.DrawLine(camPos, tgt);

        // kilka „kulek” wzdłuż wiązki dla podglądu promienia
        int steps = 8;
        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector3 p = Vector3.Lerp(camPos, tgt, t);
            Gizmos.DrawWireSphere(p, sphereRadius);
        }

        // zaznacz aktualnie trafione bloki
        Gizmos.color = gizmoHit;
        if (_hitsThisFrame != null)
        {
            foreach (var b in _hitsThisFrame)
            {
                if (b == null || !b.IsValid()) continue;
                var bounds = b.GetCombinedBounds();
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }
    }

    // ——————————————————————————————————————————————
    // Pojedynczy blok (jedno drzewo/obiekt)
    // ——————————————————————————————————————————————
    class Block
    {
        public Transform root;
        public readonly List<Renderer> renderers = new List<Renderer>(4);
        public GameObject stump;

        readonly int _cutoffID, _baseColorID, _colorID;
        readonly float _visibleCutoff, _hiddenCutoff;

        readonly Dictionary<Renderer, Coroutine> _running = new Dictionary<Renderer, Coroutine>();
        readonly Dictionary<Renderer, float> _startCutoff = new Dictionary<Renderer, float>();
        readonly Dictionary<Renderer, Color> _startColor  = new Dictionary<Renderer, Color>();
        readonly MaterialPropertyBlock _mpb = new MaterialPropertyBlock();

        public Block(Transform r, string stumpName, int cutoffID, int baseColorID, int colorID, float visibleCut, float hiddenCut)
        {
            root = r;
            _cutoffID     = cutoffID;
            _baseColorID  = baseColorID;
            _colorID      = colorID;
            _visibleCutoff= visibleCut;
            _hiddenCutoff = hiddenCut;

            var stumpTr = (!string.IsNullOrEmpty(stumpName)) ? r.Find(stumpName) : null;
            if (stumpTr) { stump = stumpTr.gameObject; stump.SetActive(false); }
        }

        public bool IsValid() => root && renderers.Count > 0;

        public Bounds GetCombinedBounds()
        {
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Count; i++) b.Encapsulate(renderers[i].bounds);
            return b;
        }

        public void ToggleStump(bool on)
        {
            if (stump && stump.activeSelf != on) stump.SetActive(on);
        }

        /// <summary> targetAlpha: 1 = widoczne, 0 = ukryte. </summary>
        public void FadeTo(float targetAlpha, float duration)
        {
            // wybór trybu per-renderer: Cutoff jeśli jest właściwość, w przeciwnym razie Color.a
            foreach (var r in renderers)
            {
                if (!r) continue;

                if (_running.TryGetValue(r, out var co) && co != null)
                    root.GetComponent<MonoBehaviour>()?.StopCoroutine(co);

                var runner = root.GetComponent<FadeRunner>() ?? root.gameObject.AddComponent<FadeRunner>();

                if (r.sharedMaterial && r.sharedMaterial.HasProperty(_cutoffID))
                {
                    float from = GetStartCutoff(r, _visibleCutoff);
                    float to   = Mathf.Lerp(_visibleCutoff, _hiddenCutoff, 1f - targetAlpha);
                    _running[r] = runner.StartCoroutine(FadeCutoff(r, from, to, duration));
                }
                else
                {
                    Color baseC = GetStartColor(r);
                    float fromA = baseC.a;
                    float toA   = targetAlpha;
                    _running[r] = runner.StartCoroutine(FadeAlpha(r, fromA, toA, duration, baseC));
                }
            }
        }

        float GetStartCutoff(Renderer r, float fallback)
        {
            if (!_startCutoff.TryGetValue(r, out var v))
            {
                v = (r.sharedMaterial && r.sharedMaterial.HasProperty(_cutoffID))
                    ? r.sharedMaterial.GetFloat(_cutoffID)
                    : fallback;
                _startCutoff[r] = v;
            }
            return v;
        }

        Color GetStartColor(Renderer r)
        {
            if (!_startColor.TryGetValue(r, out var c))
            {
                if (r.sharedMaterial && r.sharedMaterial.HasProperty(_baseColorID))
                    c = r.sharedMaterial.GetColor(_baseColorID);
                else if (r.sharedMaterial && r.sharedMaterial.HasProperty(_colorID))
                    c = r.sharedMaterial.GetColor(_colorID);
                else
                    c = Color.white;
                _startColor[r] = c;
            }
            return c;
        }

        IEnumerator FadeCutoff(Renderer r, float from, float to, float dur)
        {
            float t = 0f;
            while (t < dur && r)
            {
                t += Time.deltaTime;
                float v = Mathf.Lerp(from, to, t / dur);
                _mpb.SetFloat(_cutoffID, v);
                r.SetPropertyBlock(_mpb);
                yield return null;
            }
            if (r)
            {
                _mpb.SetFloat(_cutoffID, to);
                r.SetPropertyBlock(_mpb);
            }
        }

        IEnumerator FadeAlpha(Renderer r, float from, float to, float dur, Color baseC)
        {
            float t = 0f;
            while (t < dur && r)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(from, to, t / dur);
                var c = baseC; c.a = a;

                if (r.sharedMaterial && r.sharedMaterial.HasProperty(_baseColorID))
                    _mpb.SetColor(_baseColorID, c);
                else if (r.sharedMaterial && r.sharedMaterial.HasProperty(_colorID))
                    _mpb.SetColor(_colorID, c);

                r.SetPropertyBlock(_mpb);
                yield return null;
            }
            if (r)
            {
                var c = baseC; c.a = to;

                if (r.sharedMaterial && r.sharedMaterial.HasProperty(_baseColorID))
                    _mpb.SetColor(_baseColorID, c);
                else if (r.sharedMaterial && r.sharedMaterial.HasProperty(_colorID))
                    _mpb.SetColor(_colorID, c);

                r.SetPropertyBlock(_mpb);
            }
        }

        // host coroutine
        class FadeRunner : MonoBehaviour { }
    }
}
