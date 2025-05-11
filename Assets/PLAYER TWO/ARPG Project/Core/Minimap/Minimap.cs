using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Minimap/Minimap")]
    public class Minimap : Singleton<Minimap>
    {
        [Header("Minimap Texture")]
        [Tooltip("The texture that represents the entire map.")]
        public Texture minimapTexture;

        [Header("Map Volume")]
        [Tooltip("The center of the map.")]
        public Vector3 center;

        [Tooltip("The maximum height to capture the map.")]
        public float height = 40f;

        [Tooltip("The length of the map (X and Z size).")]
        public float length = 140f;

        [Header("Capture Settings")]
        [Tooltip("The name of the generated map texture file.")]
        public string fileName = "Minimap";

        [Tooltip("The resolution of the map texture.")]
        public Vector2Int resolution = new Vector2Int(1024, 1024);

        [Tooltip("The color of the background of the map (map borders).")]
        public Color backgroundColor = Color.black;

        [Tooltip("All the layers that will be captured by the map image.")]
        public LayerMask cullingMask;

        /// <summary>
        /// Return a point inside the map volume, in a -1 to 1 range.
        /// </summary>
        public virtual Vector2 WorldToMapPosition(Vector3 position)
        {
            var x = (position.x - center.x) / length;
            var z = (position.z - center.z) / length;

            return new Vector2(x, z);
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(center, new Vector3(length, height, length));
        }
#endif
    }
}
