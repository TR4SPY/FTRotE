using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Subtitles")]
    public class GUISubtitles : MonoBehaviour
    {
        public static GUISubtitles instance { get; private set; }

        public Text subtitleText;
        Coroutine m_currentRoutine;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (instance == null)
            {
                var go = new GameObject("GUISubtitles");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<GUISubtitles>();
            }
        }

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (!subtitleText)
            {
                var canvasGO = new GameObject("SubtitlesCanvas");
                canvasGO.transform.SetParent(transform);
                var canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 32767;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();

                var textGO = new GameObject("SubtitleText");
                textGO.transform.SetParent(canvasGO.transform);
                subtitleText = textGO.AddComponent<Text>();
                subtitleText.alignment = TextAnchor.LowerCenter;
                var rect = subtitleText.rectTransform;
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(1, 0);
                rect.pivot = new Vector2(0.5f, 0);
                rect.anchoredPosition = new Vector2(0, 20);
                subtitleText.text = string.Empty;
            }

            SetVisible(GameSettings.instance ? GameSettings.instance.GetSubtitlesEnabled() : false);
        }

        public void SetVisible(bool visible)
        {
            if (subtitleText)
                subtitleText.gameObject.SetActive(visible);
        }

        public void Show(string text, float duration)
        {
            if (!subtitleText) return;

            if (m_currentRoutine != null)
                StopCoroutine(m_currentRoutine);

            m_currentRoutine = StartCoroutine(ShowRoutine(text, duration));
        }

        IEnumerator ShowRoutine(string text, float duration)
        {
            SetVisible(true);
            subtitleText.text = text;
            yield return new WaitForSeconds(duration);
            subtitleText.text = string.Empty;
            if (!(GameSettings.instance && GameSettings.instance.GetSubtitlesEnabled()))
                SetVisible(false);
        }
    }
}
