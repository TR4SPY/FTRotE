using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(QuestGiver))]
    [AddComponentMenu("PLAYER TWO/ARPG Project/Quest/Quest Marker")]
    public class QuestMarker : MonoBehaviour
    {
        [Header("World Markers")]
        [Tooltip(
            "The Game Object that represents the marker when a Quest Giver has a Quest available."
        )]
        public GameObject availableQuestMarker;

        [Tooltip(
            "The Game Object that represents the marker when a Quest Giver has a Quest in progress."
        )]
        public GameObject inProgressQuestMarker;

        [Header("Minimap Markers")]
        [Tooltip(
            "The Minimap Icon that represents the marker on the Minimap when a Quest Giver has a Quest available."
        )]
        public MinimapIcon availableQuestIcon;

        [Tooltip(
            "The Minimap Icon that represents the marker on the Minimap when a Quest Giver has a Quest in progress."
        )]
        public MinimapIcon inProgressQuestIcon;

        protected GameObject m_availableQuestMarker;
        protected GameObject m_inProgressQuestMarker;
        protected GameObject m_availableQuestIcon;
        protected GameObject m_inProgressQuestIcon;

        protected QuestGiver m_giver;

        protected virtual void Awake()
        {
            Initialize();
            InstantiateIcons();
        }

        protected virtual void Initialize()
        {
            m_giver = GetComponent<QuestGiver>();
            m_giver.onStateChange.AddListener(OnStateChange);
        }

        protected virtual void InstantiateMarkers()
        {
            InstantiateObject(ref m_availableQuestMarker, availableQuestMarker);
            InstantiateObject(ref m_inProgressQuestMarker, inProgressQuestMarker);
        }

        protected virtual void InstantiateIcons()
        {
            InstantiateObject(ref m_availableQuestIcon, availableQuestIcon);
            InstantiateObject(ref m_inProgressQuestIcon, inProgressQuestIcon);
        }

        protected virtual void InstantiateObject(ref GameObject instance, Object go)
        {
            if (instance || !go)
                return;

            instance = (GameObject)Instantiate(go, Vector3.zero, Quaternion.identity);
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
                SetActive(obj, active);
        }

        protected virtual void SetActive(GameObject obj, bool active)
        {
            if (obj)
                obj.SetActive(active);
        }

        protected virtual void OnStateChange(QuestGiver.State state)
        {
            DisableAll();

            switch (state)
            {
                default:
                case QuestGiver.State.None:
                    break;
                case QuestGiver.State.QuestAvailable:
                    SetActive(true, m_availableQuestMarker, m_availableQuestIcon);
                    break;
                case QuestGiver.State.QuestInProgress:
                    SetActive(true, m_inProgressQuestMarker, m_inProgressQuestIcon);
                    break;
            }
        }
    }
}
