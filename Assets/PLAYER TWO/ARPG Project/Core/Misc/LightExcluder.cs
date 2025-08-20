using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    /// <summary>
    /// Ensures global scene lights ignore the inventory preview layer so that
    /// only dedicated preview lights affect item renders.
    /// </summary>
    public class PreviewLayerLightExcluder : MonoBehaviour
    {
        [SerializeField]
        private string previewLayerName = "Model_Preview";

        private void Start()
        {
            ExcludePreviewLayerFromSceneLights();
        }

        /// <summary>
        /// Removes the preview layer from all scene lights except the generated
        /// preview lights themselves.
        /// </summary>
        public void ExcludePreviewLayerFromSceneLights()
        {
            int previewLayer = LayerMask.NameToLayer(previewLayerName);
            if (previewLayer < 0)
                return;

            foreach (var sceneLight in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (sceneLight.gameObject.name == "PreviewLight")
                    continue;

                sceneLight.cullingMask &= ~(1 << previewLayer);
            }
        }
    }
}
