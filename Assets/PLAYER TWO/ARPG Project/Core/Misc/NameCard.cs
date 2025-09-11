using UnityEngine;
using UnityEngine.UI;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(Billboard))]
    public class NameCard : MonoBehaviour
    {
        [Header("Name")]
        [SerializeField] private Text m_nameText;

        [Header("Faction")]
        [SerializeField] private GameObject m_factionRoot;
        [SerializeField] private Text m_factionText;
        [SerializeField] private Image m_factionIcon;

        private void Awake()
        {
            if (m_nameText == null)
                m_nameText = GetComponentInChildren<Text>();

            if (m_nameText != null)
            {
                if (m_factionRoot == null)
                {
                    var parent = m_nameText.transform.parent;
                    var factionTransform = parent != null ? parent.Find("Faction") : null;
                    if (factionTransform != null)
                        m_factionRoot = factionTransform.gameObject;
                }

                if (m_factionRoot != null)
                {
                    if (m_factionText == null)
                        m_factionText = m_factionRoot.GetComponentInChildren<Text>(true);
                    if (m_factionIcon == null)
                        m_factionIcon = m_factionRoot.GetComponentInChildren<Image>(true);
                }
            }
        }

        private void Start()
        {
            if (m_nameText == null || m_factionRoot == null || m_factionText == null || m_factionIcon == null)
                return;

            var factionMember = GetComponentInParent<FactionMember>();
            var faction = factionMember != null ? factionMember.faction : Faction.None;
            if (faction != Faction.None)
            {
                m_factionRoot.SetActive(true);
                m_factionText.text = FactionUtility.GetDisplayName(faction);
                var icon = FactionUtility.GetIcon(faction);
                if (icon != null)
                {
                    m_factionIcon.sprite = icon;
                    m_factionIcon.gameObject.SetActive(true);
                }
                else
                {
                    m_factionIcon.gameObject.SetActive(false);
                }
            }
            else
            {
                m_factionRoot.SetActive(false);
            }
        }
    }
}
