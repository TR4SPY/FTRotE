using UnityEngine;
using System;
using System.Reflection;

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

        private Delegate _weatherLightSpawnHandler;

        private void Start()
        {
            ExcludePreviewLayerFromSceneLights();
            TryHookWeatherLightSpawn();
        }

        /// <summary>
        /// Attempts to subscribe to the weather manager light-spawn event if the
        /// Procedural Worlds weather system is present. Falls back to manual
        /// invocation if the event is unavailable.
        /// </summary>
        private void TryHookWeatherLightSpawn()
        {
            var weatherType = Type.GetType("ProceduralWorldsGlobalWeather");
            if (weatherType == null)
                return;

            var instanceProp = weatherType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            var instance = instanceProp?.GetValue(null);
            if (instance == null)
                return;

            var eventInfo = weatherType.GetEvent("OnLightSpawned") ?? weatherType.GetEvent("LightSpawned");
            if (eventInfo == null)
                return;

            try
            {
                var invokeParams = eventInfo.EventHandlerType.GetMethod("Invoke")?.GetParameters();
                if (invokeParams == null)
                    return;

                _weatherLightSpawnHandler = invokeParams.Length == 0
                    ? Delegate.CreateDelegate(eventInfo.EventHandlerType, this, nameof(OnWeatherLightSpawned))
                    : Delegate.CreateDelegate(eventInfo.EventHandlerType, this, nameof(OnWeatherLightSpawnedWithArgs));

                eventInfo.AddEventHandler(instance, _weatherLightSpawnHandler);
            }
            catch
            {
                _weatherLightSpawnHandler = null;
            }
        }

        private void OnDestroy()
        {
            var weatherType = Type.GetType("ProceduralWorldsGlobalWeather");
            if (weatherType == null || _weatherLightSpawnHandler == null)
                return;

            var instanceProp = weatherType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            var instance = instanceProp?.GetValue(null);
            var eventInfo = weatherType.GetEvent("OnLightSpawned") ?? weatherType.GetEvent("LightSpawned");
            if (instance != null && eventInfo != null)
            {
                eventInfo.RemoveEventHandler(instance, _weatherLightSpawnHandler);
            }
        }

        /// <summary>
        /// Allows external systems to notify that new lights were spawned.
        /// Can be wired to weather/time-of-day events or called manually.
        /// </summary>
        public void OnWeatherLightSpawned()
        {
            ExcludePreviewLayerFromSceneLights();
        }

        // Overload that accepts an unused argument to match common delegate signatures.
        public void OnWeatherLightSpawnedWithArgs(object _)
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

            foreach (var sceneLight in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (sceneLight.gameObject.name == "PreviewLight")
                    continue;

                sceneLight.cullingMask &= ~(1 << previewLayer);
            }
        }
    }
}