using UnityEngine;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Craftman")]
    public class GUICraftman : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("The prefab to use as the section, which corresponds to the inventory.")]
        public GUIInventory sectionPrefab;

        [Tooltip("A reference to the container for the section.")]
        public RectTransform sectionContainer;

        protected Craftman m_craftman;
        protected GUIInventory m_section;

        protected Inventory m_playerInventory =>
            Level.instance.player.inventory.instance;

        protected GUIInventory m_playerGUIInventory =>
            GUIWindowsManager.instance.GetInventory();

        protected GameAudio m_audio => GameAudio.instance;

        protected virtual void Awake()
        {
           // Debug.Log("[GUICraftman] Awake()");
        }

        protected virtual void OnEnable()
        {
           // Debug.Log("[GUICraftman] OnEnable()");
        }

        protected virtual void OnDisable()
        {
           // Debug.Log("[GUICraftman] OnDisable()");
            GUIWindowsManager.instance.inventoryWindow.Hide();
        }

        protected virtual void Start()
        {
           // Debug.Log("[GUICraftman] Start()");
        }

        protected virtual void InitializeSection()
        {
           // Debug.Log("[GUICraftman] InitializeSection()");
            if (m_craftman == null)
            {
                Debug.LogError("[GUICraftman] m_craftman is NULL!");
                return;
            }

            if (m_craftman.inventories == null)
            {
                Debug.LogError("[GUICraftman] m_craftman.inventories is NULL!");
                return;
            }

            if (string.IsNullOrEmpty(m_craftman.section.title))
            {
                Debug.LogWarning("[GUICraftman] Section title is empty.");
            }

            if (m_craftman.inventories.TryGetValue(m_craftman.section.title, out var inventory))
            {
               // Debug.Log($"[GUICraftman] Found inventory for section: {m_craftman.section.title}");
                m_section = Instantiate(sectionPrefab, sectionContainer);
                m_section.SetInventory(inventory);
                m_section.InitializeInventory();
                m_section.gameObject.SetActive(true);
               // Debug.Log("[GUICraftman] Section initialized and shown.");
            }
            else
            {
                Debug.LogError($"[GUICraftman] Could not find inventory for section: {m_craftman.section.title}");
            }
        }

        protected virtual void DestroySection()
        {
           // Debug.Log("[GUICraftman] DestroySection()");
            foreach (Transform child in sectionContainer)
            {
               // Debug.Log($"[GUICraftman] Destroying child: {child.name}");
                Destroy(child.gameObject);
            }
        }

        public virtual void SetCraftman(Craftman craftman)
        {
           // Debug.Log("[GUICraftman] SetCraftman()");
            if (m_craftman == craftman)
            {
               // Debug.Log("[GUICraftman] Already using this Craftman, skipping.");
                return;
            }

            m_craftman = craftman;

           // Debug.Log("[GUICraftman] Craftman set, refreshing section...");
            DestroySection();
            InitializeSection();
        }
    }
}
