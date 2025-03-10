using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(QuestGiver))]
    [AddComponentMenu("PLAYER TWO/ARPG Project/Quest/Quest Marker")]
    public class QuestMarker : MonoBehaviour
    {
        [Header("World Markers")]
        public GameObject availableQuestMarker;
        public GameObject inProgressQuestMarker;

        [Header("Minimap Markers")]
        public MinimapIcon availableQuestIcon;
        public MinimapIcon inProgressQuestIcon;

        protected GameObject m_availableQuestMarker;
        protected GameObject m_inProgressQuestMarker;
        protected GameObject m_availableQuestIcon;
        protected GameObject m_inProgressQuestIcon;

        protected QuestGiver m_giver;

        protected virtual void Awake()
        {
            Initialize();
            InstantiateMarkers();
            InstantiateIcons();
        }

        protected virtual void Initialize()
        {
            m_giver = GetComponent<QuestGiver>();
            m_giver.onStateChange.AddListener(OnStateChange);
        }

        protected virtual void InstantiateMarkers()
        {
            InstantiateWorldMarker(ref m_availableQuestMarker, availableQuestMarker, Vector3.up * 3f);
            InstantiateWorldMarker(ref m_inProgressQuestMarker, inProgressQuestMarker, Vector3.up * 3f);
        }

        protected virtual void InstantiateIcons()
        {
            InstantiateMinimapIcon(ref m_availableQuestIcon, availableQuestIcon);
            InstantiateMinimapIcon(ref m_inProgressQuestIcon, inProgressQuestIcon);
        }

        protected virtual void InstantiateWorldMarker(ref GameObject instance, GameObject prefab, Vector3 offset)
        {
            if (instance || !prefab)
                return;

            instance = Instantiate(prefab, transform.position + offset, Quaternion.identity, transform);
            instance.SetActive(false);
        }

        protected virtual void InstantiateMinimapIcon(ref GameObject instance, MinimapIcon prefab)
        {
            if (instance || !prefab)
                return;

            instance = Instantiate(prefab.gameObject, transform.position, Quaternion.identity, transform);
            instance.SetActive(false);
        }

        protected virtual void DisableAll()
        {
            SetActive(
                false,
                m_availableQuestMarker,
                m_availableQuestIcon,
                m_inProgressQuestMarker,
                m_inProgressQuestIcon
            );
        }

        protected virtual void SetActive(bool active, params GameObject[] gameObjects)
        {
            foreach (var obj in gameObjects)
                if (obj) obj.SetActive(active);
        }

        protected virtual void OnStateChange(QuestGiver.State state)
        {
            DisableAll();

            var originalMinimapIcon = GetComponent<MinimapIcon>();

            switch (state)
            {
                default:
                case QuestGiver.State.None:
                    if (originalMinimapIcon) originalMinimapIcon.SetVisibility(true);
                    break;

                case QuestGiver.State.QuestAvailable:
                    SetActive(true, m_availableQuestMarker, m_availableQuestIcon);
                    if (originalMinimapIcon) originalMinimapIcon.SetVisibility(false);
                    break;

                case QuestGiver.State.QuestInProgress:
                    SetActive(true, m_inProgressQuestMarker, m_inProgressQuestIcon);
                    if (originalMinimapIcon) originalMinimapIcon.SetVisibility(false);
                    break;
            }
        }
    }
}
