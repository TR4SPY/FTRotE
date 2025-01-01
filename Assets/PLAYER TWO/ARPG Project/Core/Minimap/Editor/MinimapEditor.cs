using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace PLAYERTWO.ARPGProject
{
    [CustomEditor(typeof(Minimap))]
    public class MinimapEditor : Editor
    {
        private Minimap m_minimap;
        private string m_path;

        private void OnEnable() => m_minimap = (Minimap)target;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("The generated texture will be saved" +
                "in the same folder of your scene, encoded to PNG, " +
                "using the Capture Settings.", MessageType.Info);

            if (GUILayout.Button("Generate Minimap Texture"))
            {
                UpdatePath();
                CaptureShot();
            }
        }

        private void UpdatePath()
        {
            m_path = SceneManager.GetActiveScene().path;
            m_path = m_path.Replace(".unity", "");

            if (!Directory.Exists(m_path))
            {
                Directory.CreateDirectory(m_path);
            }
        }

        private void CaptureShot()
        {
            var minimapCamera = new GameObject().AddComponent<Camera>();
            var originalLightCount = QualitySettings.pixelLightCount;

            QualitySettings.pixelLightCount = 9999;

            minimapCamera.orthographic = true;
            minimapCamera.clearFlags = CameraClearFlags.SolidColor;
            minimapCamera.backgroundColor = m_minimap.backgroundColor;
            minimapCamera.cullingMask = m_minimap.cullingMask;
            minimapCamera.orthographicSize = m_minimap.length * 0.5f;
            minimapCamera.transform.eulerAngles = new Vector3(90, 0, 0);
            minimapCamera.transform.position = m_minimap.center + Vector3.up * m_minimap.height * 0.5f;

            var cameraRender = new RenderTexture(m_minimap.resolution.x, m_minimap.resolution.y, 32);
            minimapCamera.targetTexture = cameraRender;

            var renderTexture = RenderTexture.active;
            RenderTexture.active = minimapCamera.targetTexture;

            minimapCamera.Render();

            var targetTexture = minimapCamera.targetTexture;
            Texture2D texture = new Texture2D(targetTexture.width, targetTexture.height, TextureFormat.RGB24, false);

            texture.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);
            texture.Apply();

            RenderTexture.active = renderTexture;

            minimapCamera.targetTexture = null;

            var path = $"{m_path}/{m_minimap.fileName}.png";

            File.WriteAllBytes(path, texture.EncodeToPNG());
            AssetDatabase.ImportAsset(path);

            m_minimap.minimapTexture = (Texture)AssetDatabase.LoadAssetAtPath(path, typeof(Texture));

            Debug.Log($"Minimap image saved in {m_path}!");

            QualitySettings.pixelLightCount = originalLightCount;

            DestroyImmediate(minimapCamera.gameObject);
            DestroyImmediate(renderTexture);
            DestroyImmediate(targetTexture);
            DestroyImmediate(cameraRender);
        }
    }
}
