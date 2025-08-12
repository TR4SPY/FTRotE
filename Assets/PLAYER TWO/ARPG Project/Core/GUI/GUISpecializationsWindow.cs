using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Specializations Window")]
    public class GUISpecializationsWindow : GUIWindow
    {
        [Header("Specialization Settings")]
        [Tooltip("Optional manual override. If empty, family is resolved from current character.")]
        public string classFamily;

        [Tooltip("Buttons representing the available specializations (in order 0,1,2).")]
        public Button[] specializationButtons = new Button[3];

        [Tooltip("Window to show after a specialization is selected.")]
        public GUIWindow masterSkillTreeWindow;

        [Tooltip("Decorative frame sprite overlaid on specialization buttons.")]
        public Sprite frameSprite;

        [Header("Debug")]
        public bool verboseDebug = true;
        public bool useChildIconImage = false;
        public string childIconName = "Icon";

        private bool _builtOnce;

        private void Awake()
        {
            if (verboseDebug) Debug.Log($"[GUI-Spec] Awake on {name}. Enabled={enabled} ActiveInHierarchy={gameObject.activeInHierarchy}", this);
        }

        protected override void Start()
        {
            base.Start();
            if (verboseDebug) Debug.Log($"[GUI-Spec] Start on {name}. Build fallback.", this);
            SafeBuild();
        }

        protected override void OnOpen()
        {
            if (verboseDebug) Debug.Log($"[GUI-Spec] OnOpen on {name}.", this);
            base.OnOpen();
            SafeBuild();
        }

        public override void Show()
        {
            if (verboseDebug) Debug.Log($"[GUI-Spec] Show() on {name}.", this);
            base.Show();
            SafeBuild();
        }

        public override void Hide()
        {
            if (verboseDebug) Debug.Log($"[GUI-Spec] Hide() on {name}.", this);
            base.Hide();
            _builtOnce = false;
        }

        [ContextMenu("Diagnostics/Rebuild Buttons Now")]
        private void RebuildButtonsNow_Context()
        {
            if (verboseDebug) Debug.Log($"[GUI-Spec] ContextMenu → manual rebuild on {name}", this);
            _builtOnce = false;
            SafeBuild();
        }

        private void SafeBuild()
        {
            if (_builtOnce) return;
            _builtOnce = true;
            BuildButtons();
        }

        private string ResolveFamily()
        {
            if (!string.IsNullOrWhiteSpace(classFamily))
                return classFamily;

            string className = Game.instance?.currentCharacter?.data?.name;

            if (string.IsNullOrWhiteSpace(className))
            {
                try { className = Game.instance?.currentCharacter?.GetName(); } catch { }
            }
            if (string.IsNullOrWhiteSpace(className))
            {
                var ent = Level.instance?.player;
                if (ent) className = ent.name?.Replace("(Clone)", "").Trim();
            }
            if (string.IsNullOrWhiteSpace(className))
                return null;

            bool TryGetBitCI(string name, out CharacterClassRestrictions bit)
            {
                bit = CharacterClassRestrictions.None;
                var map = ClassHierarchy.NameToBits;
                if (map == null) return false;

                foreach (var kv in map)
                {
                    if (string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase))
                    {
                        bit = kv.Value;
                        return true;
                    }
                }
                return false;
            }

            if (TryGetBitCI(className, out var classBit))
            {
                var families = ClassHierarchy.Families;
                if (families != null)
                {
                    foreach (var fam in families)
                    {
                        var tiers = fam.Tiers;
                        if (tiers == null) continue;

                        for (int i = 0; i < tiers.Length; i++)
                        {
                            if (tiers[i] == classBit)
                                return fam.FamilyName;
                        }
                    }
                }
            }

            return className;
        }

        private void BuildButtons()
        {
            var resolvedFamily = ResolveFamily();

            if (verboseDebug)
            {
                Debug.Log($"[GUI-Spec] BuildButtons() begin. classFamily(override)='{classFamily ?? "<null>"}', resolved='{resolvedFamily ?? "<null>"}'", this);
                if (GameDatabase.instance == null)
                    Debug.LogWarning("[GUI-Spec] GameDatabase.instance == NULL (czy obiekt GameDatabase jest w scenie?)", this);
                else if (GameDatabase.instance.gameData == null)
                    Debug.LogWarning("[GUI-Spec] GameDatabase.gameData == NULL (przypnij GameData w inspektorze)", this);
                else
                    Debug.Log($"[GUI-Spec] DB.specializations count = {GameDatabase.instance.gameData.specializations?.Count ?? -1}", this);
            }

            string familyToUse = !string.IsNullOrWhiteSpace(classFamily) ? classFamily : resolvedFamily;

            List<Specializations> defs = new List<Specializations>();
            if (!string.IsNullOrWhiteSpace(familyToUse))
            {
                defs = GameDatabase.instance
                    ? GameDatabase.instance.GetSpecializationsByFamily(familyToUse)
                    : Specializations.FindByFamily(familyToUse);

                if (!GameDatabase.instance)
                    defs.Sort((a, b) => a.tier.CompareTo(b.tier));
            }

            if (verboseDebug)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"[GUI-Spec] Using family='{familyToUse ?? "<none>"}' → defs.Count={defs.Count}");
                for (int i = 0; i < defs.Count; i++)
                {
                    var d = defs[i];
                    sb.AppendLine($"  [{i}] {(d ? $"id={d.id} name={d.name} tier={d.tier} icon={(d.icon ? d.icon.name : "<null>")}" : "<null>")}");
                }
                if (defs.Count == 0)
                    sb.AppendLine("[GUI-Spec] No specializations found — buttons will be transparent & non-interactable.");
                Debug.Log(sb.ToString(), this);
            }

            for (int i = 0; i < specializationButtons.Length; i++)
            {
                var btn = specializationButtons[i];
                if (!btn)
                {
                    Debug.LogWarning($"[GUI-Spec] Button[{i}] is NULL on {name}.", this);
                    continue;
                }

                var def = (i < defs.Count) ? defs[i] : null;

                Image targetImg = null;
                if (useChildIconImage)
                {
                    var child = btn.transform.Find(childIconName);
                    if (child) targetImg = child.GetComponent<Image>();
                    if (!targetImg)
                        Debug.LogWarning($"[GUI-Spec] Button[{i}] child '{childIconName}' Image not found. Falling back to Button Image.", this);
                }
                if (!targetImg) targetImg = btn.GetComponent<Image>();

                if (!targetImg)
                {
                    Debug.LogWarning($"[GUI-Spec] Button[{i}] has NO Image component. Add Image to the Button root or child '{childIconName}'.", this);
                }
                else
                {
                    targetImg.sprite = def ? def.icon : null;
                    targetImg.preserveAspect = true;

                    if (targetImg.sprite)
                    {
                        targetImg.enabled = true;
                        targetImg.color = Color.white;
                        if (verboseDebug)
                        {
                            var tex = targetImg.sprite.texture;
                            Debug.Log($"[GUI-Spec] Button[{i}] '{btn.name}': sprite='{targetImg.sprite.name}', enabled={targetImg.enabled}, color={targetImg.color}", this);
                            Debug.Log($"[GUI-Spec]   Sprite tex='{(tex ? tex.name : "<null>")}', rect={targetImg.sprite.rect}, pivot={targetImg.sprite.pivot}", this);
                        }
                    }
                    else
                    {
                        targetImg.enabled = false;
                        targetImg.color = Color.white;
                        if (verboseDebug)
                            Debug.Log($"[GUI-Spec] Button[{i}] '{btn.name}': no sprite → transparent.", this);
                    }
                }

                var frameTr = btn.transform.Find("Frame");
                var frame = frameTr ? frameTr.GetComponent<Image>() : null;
                if (frame)
                {
                    frame.sprite = frameSprite;
                    if (frame.sprite) frame.SetNativeSize();
                }

                btn.onClick.RemoveAllListeners();
                if (def)
                {
                    btn.interactable = true;
                    btn.onClick.AddListener(() => OnSelectSpecialization(def));
                }
                else
                {
                    btn.interactable = false;
                    btn.onClick.AddListener(() =>
                        Debug.LogWarning($"[GUI-Spec] Click Button[{i}] but def is NULL (family='{familyToUse ?? "<none>"}')", this));
                }
            }

            if (verboseDebug) Debug.Log("[GUI-Spec] BuildButtons() end.", this);
        }

        private void OnSelectSpecialization(Specializations def)
        {
            if (!def)
            {
                Debug.LogWarning("[GUI-Spec] Attempted to select a null specialization", this);
                return;
            }

            Game.instance?.currentCharacter?.SelectSpecialization(0, def);
            Hide();

            if (masterSkillTreeWindow != null)
                masterSkillTreeWindow.Show();
        }

        private void OnDisable()
        {
            if (verboseDebug) Debug.Log($"[GUI-Spec] OnDisable on {name}", this);
        }
        private void OnEnable()
        {
            if (verboseDebug) Debug.Log($"[GUI-Spec] OnEnable on {name}", this);
            closeWhenPlayerMove = false;
        }

    }
}
