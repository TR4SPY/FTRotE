using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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
        public RectTransform crestListContainer;
        public RectTransform fileListContainer;
        public RectTransform scrollListContainer;
        public GameObject fileEntryPrefab;
        public GameObject scrollEntryPrefab;

        [Header("Preview & Actions")]
        public Image crestPreview;
        public Button resetButton;
        public Button acceptButton;


        [Header("Guild Creation Cost")]
        [Tooltip("Container used to display the guild creation price.")]
        public Transform createPriceContainer;

        [Tooltip("Prefab with 'Name'(Text) and 'Icon'(Image).")]
        public GameObject priceTagPrefab;

        [Header("Currency Icons")]
        public Sprite solmireIcon;
        public Sprite lunarisIcon;
        public Sprite amberlingsIcon;

        private readonly Dictionary<string, Texture2D> m_cache = new();
        private string m_selectedFile;
        private Sprite m_selectedSprite;
        private Sprite m_defaultPreviewSprite;

        private static readonly int k_CreateCost = Currency.ConvertToAmberlings(5, CurrencyType.Solmire);

        protected Guildmaster m_guildmaster;

        private InputActionMap gameplayMap;
        private InputActionMap guiMap;
        private bool wasFocusedLastFrame;

        protected override void Start()
        {
            base.Start();

            if (crestPreview)
                m_defaultPreviewSprite = crestPreview.sprite;

            gameplayMap = Game.instance.gameplayActions?.FindActionMap("Gameplay");
            guiMap = Game.instance.guiActions?.FindActionMap("GUI");

            if (gameplayMap == null)
                Debug.LogError("[GUIGuildmaster] Gameplay input action map not found.");

            if (guiMap == null)
                Debug.LogError("[GUIGuildmaster] GUI input action map not found.");

            wasFocusedLastFrame = false;

            if (guildNameInput)
            {
                guildNameInput.characterLimit = 20;
                guildNameInput.onValueChanged.AddListener(_ => UpdateAcceptButton());
            }

            if (fileToggle) fileToggle.onValueChanged.AddListener(OnFileToggleChanged);
            if (scrollToggle) scrollToggle.onValueChanged.AddListener(OnScrollToggleChanged);

            if (resetButton) resetButton.onClick.AddListener(ResetSelection);
            if (acceptButton) acceptButton.onClick.AddListener(Accept);

            if (crestPreview)
            {
                crestPreview.type = Image.Type.Simple;
                crestPreview.preserveAspect = true;
            }

            OnFileToggleChanged(fileToggle?.isOn ?? true);
            OnScrollToggleChanged(scrollToggle?.isOn ?? false);
            UpdateAcceptButton();
        }

        private RectTransform GetActiveListContainer()
        {
            return fileToggle != null && fileToggle.isOn ? fileListContainer : scrollListContainer;
        }

        private void OnFileToggleChanged(bool isOn)
        {
            if (fileListContainer)
                fileListContainer.gameObject.SetActive(isOn);

            if (isOn)
                PopulateCrestFileList();
        }

        private void OnScrollToggleChanged(bool isOn)
        {
            if (scrollListContainer)
                scrollListContainer.gameObject.SetActive(isOn);

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
                
                if (crestPreview)
                {
                    crestPreview.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    crestPreview.preserveAspect = true;
                }
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
            if (crestPreview)
            {
                crestPreview.sprite = sprite;
                crestPreview.preserveAspect = true;
            }
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
            if (crestPreview) crestPreview.sprite = m_defaultPreviewSprite;

            m_selectedFile = null;
            m_selectedSprite = null;
            UpdateAcceptButton();
        }

        private void UpdateAcceptButton()
        {
            if (acceptButton)
               {
                bool hasNameAndCrest = !string.IsNullOrWhiteSpace(guildNameInput?.text) && (m_selectedSprite != null || !string.IsNullOrEmpty(m_selectedFile));

                var money = Level.instance?.player?.inventory?.instance?.money ?? 0;
                bool canAfford = money >= k_CreateCost;

                acceptButton.interactable = hasNameAndCrest && canAfford;
            }
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

            string guildName = guildNameInput.text.Trim();
            TMP_SpriteAsset crestAsset = CreateTMPAssetFromSprite(crest);

            var inventory = Level.instance.player.inventory.instance;
            if (inventory.money < k_CreateCost)
                return;

            inventory.SpendMoney(k_CreateCost);

            GuildManager.CreateGuild(guildName, crest, crestAsset);
            Hide();
        }

        private void Update()
        {
            bool isFocused = guildNameInput != null && guildNameInput.isFocused;

            if (isFocused && !wasFocusedLastFrame)
            {
                gameplayMap?.Disable();
                guiMap?.Disable();
            }
            else if (!isFocused && wasFocusedLastFrame)
            {
                gameplayMap?.Enable();
                guiMap?.Enable();
            }

            wasFocusedLastFrame = isFocused;
        }

        public virtual void SetGuildmaster(Guildmaster guildmaster)
        {
            m_guildmaster = guildmaster;
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            ShowPriceTags(createPriceContainer, k_CreateCost);
            UpdateAcceptButton();
        }
        
        private void OnDisable()
        {
            gameplayMap?.Enable();
            guiMap?.Enable();
        }

        private void ClearPriceTags(Transform container)
        {
            if (!container) return;
            foreach (Transform child in container)
                Destroy(child.gameObject);
        }

        private void AddPriceTag(Transform container, int amount, Sprite icon)
        {
            var go = Instantiate(priceTagPrefab, container);

            var textObj = go.transform.Find("Name")?.GetComponent<Text>();
            if (textObj) textObj.text = amount.ToString();

            var imageObj = go.transform.Find("Icon")?.GetComponent<Image>();
            if (imageObj && icon)
                imageObj.sprite = icon;
        }

        private void ShowPriceTags(Transform container, int totalAmberlings)
        {
            ClearPriceTags(container);

            if (totalAmberlings <= 0) return;

            var c = new Currency();
            c.SetFromTotalAmberlings(totalAmberlings);

            if (c.solmire > 0)
                AddPriceTag(container, c.solmire, solmireIcon);
            if (c.lunaris > 0)
                AddPriceTag(container, c.lunaris, lunarisIcon);
            if (c.amberlings > 0)
                AddPriceTag(container, c.amberlings, amberlingsIcon);
        }

        private static TMP_SpriteAsset CreateTMPAssetFromSprite(Sprite sprite)
        {
            if (sprite == null) return null;

            var asset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
            asset.name = $"GuildTMPAsset_{sprite.name}";

            var spriteInfo = new TMP_Sprite
            {
                name = sprite.name,
                sprite = sprite,
                unicode = 0xE000
            };

            asset.spriteInfoList = new System.Collections.Generic.List<TMP_Sprite> { spriteInfo };
            asset.UpdateLookupTables();
            return asset;
        }
    }
}
