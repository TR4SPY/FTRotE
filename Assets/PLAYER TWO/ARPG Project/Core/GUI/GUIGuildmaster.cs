using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Guildmaster")]
    public class GUIGuildmaster : GUIWindow
    {
        [Header("Name Input")]
        public TMP_InputField guildNameInput;

        [Header("Toggle Group")]
        public Toggle fileToggle;
        public Toggle scrollToggle;

        [Header("Crest List")]
        public RectTransform crestListContainer; // "Crest Choice Selection"
        public GameObject fileEntryPrefab;
        public GameObject scrollEntryPrefab;

        [Header("Preview & Actions")]
        public Image crestPreview;
        public Button resetButton;
        public Button acceptButton;

        private readonly Dictionary<string, Texture2D> m_cache = new();
        private string m_selectedFile;
        private Sprite m_selectedSprite;

        protected Guildmaster m_guildmaster;


        protected override void Start()
        {
            base.Start();

            if (guildNameInput)
            {
                guildNameInput.characterLimit = 20;
                guildNameInput.onValueChanged.AddListener(_ => UpdateAcceptButton());
            }

            if (fileToggle) fileToggle.onValueChanged.AddListener(OnFileToggleChanged);
            if (scrollToggle) scrollToggle.onValueChanged.AddListener(OnScrollToggleChanged);

            if (resetButton) resetButton.onClick.AddListener(ResetSelection);
            if (acceptButton) acceptButton.onClick.AddListener(Accept);

            // Inicjalny stan
            OnFileToggleChanged(fileToggle?.isOn ?? true);
            UpdateAcceptButton();
        }

        private RectTransform GetActiveListContainer()
        {
            foreach (Transform child in crestListContainer)
            {
                if (child.gameObject.activeSelf)
                    return child as RectTransform;
            }
            return null;
        }

        private void OnFileToggleChanged(bool isOn)
        {
            if (isOn)
                PopulateCrestFileList();
        }

        private void OnScrollToggleChanged(bool isOn)
        {
            if (isOn)
                PopulateScrollList();
        }

        private void PopulateCrestFileList()
        {
            var container = GetActiveListContainer();
            if (!container || !fileEntryPrefab) return;

            foreach (Transform child in container)
                Destroy(child.gameObject);

            string folderPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Crests"));
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var files = Directory.GetFiles(folderPath, "*.*");
            var valid = new List<string>();
            foreach (var f in files)
            {
                var ext = Path.GetExtension(f).ToLower();
                if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
                    valid.Add(f);
            }

            if (valid.Count == 0)
            {
                var empty = Instantiate(fileEntryPrefab, container);
                var txt = empty.GetComponentInChildren<TextMeshProUGUI>();
                if (txt) txt.text = "No crest files found.";
                return;
            }

            foreach (var path in valid)
            {
                var entry = Instantiate(fileEntryPrefab, container);
                var txt = entry.GetComponentInChildren<TextMeshProUGUI>();
                if (txt) txt.text = Path.GetFileName(path);

                var btn = entry.GetComponent<Button>();
                if (btn)
                {
                    var p = path;
                    btn.onClick.AddListener(() => SelectFile(p));
                }
            }
        }

        private void PopulateScrollList()
        {
            var container = GetActiveListContainer();
            if (!container || !scrollEntryPrefab) return;

            foreach (Transform child in container)
                Destroy(child.gameObject);

            var inventory = Level.instance.player.inventory.instance;
            var sprites = new List<Sprite>();

            foreach (var kv in inventory.items)
            {
                if (kv.Key?.data is GuildCrestScroll scroll && scroll.crests != null)
                    sprites.AddRange(scroll.crests);
            }

            if (sprites.Count == 0)
            {
                var empty = Instantiate(scrollEntryPrefab, container);
                var txt = empty.GetComponentInChildren<TextMeshProUGUI>();
                if (txt) txt.text = "No crest scrolls found.";
                return;
            }

            foreach (var sprite in sprites)
            {
                if (!sprite) continue;

                var entry = Instantiate(scrollEntryPrefab, container);
                var img = entry.GetComponentInChildren<Image>();
                if (img) img.sprite = sprite;

                var btn = entry.GetComponent<Button>();
                if (btn)
                {
                    var s = sprite;
                    btn.onClick.AddListener(() => SelectSprite(s));
                }
            }
        }

        public void SelectFile(string path)
        {
            m_selectedFile = path;
            m_selectedSprite = null;

            var tex = LoadTexture(path);
            if (tex != null)
            {
                tex = ResizeTexture(tex, 256, 256);
                crestPreview.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                Debug.LogError($"Failed to load crest at {path}");
            }
            UpdateAcceptButton();
        }

        public void SelectSprite(Sprite sprite)
        {
            m_selectedSprite = sprite;
            m_selectedFile = null;
            if (crestPreview) crestPreview.sprite = sprite;
            UpdateAcceptButton();
        }

        private Texture2D LoadTexture(string path)
        {
            if (m_cache.TryGetValue(path, out var cached))
                return cached;

            try
            {
                var bytes = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2);
                if (tex.LoadImage(bytes))
                {
                    m_cache[path] = tex;
                    return tex;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading crest {path}: {e.Message}");
            }
            return null;
        }

        private static Texture2D ResizeTexture(Texture2D source, int width, int height)
        {
            var rt = RenderTexture.GetTemporary(width, height);
            Graphics.Blit(source, rt);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            return tex;
        }

        private void ResetSelection()
        {
            if (guildNameInput) guildNameInput.text = string.Empty;
            if (crestPreview) crestPreview.sprite = null;
            m_selectedFile = null;
            m_selectedSprite = null;
            UpdateAcceptButton();
        }

        private void UpdateAcceptButton()
        {
            if (acceptButton)
                acceptButton.interactable =
                    !string.IsNullOrWhiteSpace(guildNameInput?.text) &&
                    (m_selectedSprite != null || !string.IsNullOrEmpty(m_selectedFile));
        }

        private void Accept()
        {
            if (acceptButton && !acceptButton.interactable) return;

            Sprite crest = m_selectedSprite;
            if (crest == null && !string.IsNullOrEmpty(m_selectedFile))
            {
                var tex = LoadTexture(m_selectedFile);
                if (tex)
                    crest = Sprite.Create(ResizeTexture(tex, 256, 256), new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
            }

            GuildManager.CreateGuild(guildNameInput.text.Trim(), crest);
            Hide();
        }

        public virtual void SetGuildmaster(Guildmaster guildmaster)
        {
            m_guildmaster = guildmaster;
        }
    }
}
