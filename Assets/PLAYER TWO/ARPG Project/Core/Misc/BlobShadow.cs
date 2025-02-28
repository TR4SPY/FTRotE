using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Misc/Blob Shadow")]
    public class BlobShadow : MonoBehaviour
    {
        [Header("Blob Shadow Settings")]
        [Tooltip("The source transform to cast the ray from.")]
        public Transform source;

        [Tooltip("The maximum distance to cast the ray.")]
        public float maxGroundDistance = 1f;

        [Tooltip("The offset from the ground.")]
        public float groundOffset = 0.25f;

        [Tooltip("The layer mask to use for the ground.")]
        public LayerMask groundLayer;

        private void Awake()
        {
            groundLayer = 1 << LayerMask.NameToLayer("Terrain");
        }

        private void LateUpdate()
        {
            if (source == null)
                return;

            Terrain terrain = Terrain.activeTerrain;
            bool foundGround = false;
            Vector3 newPosition = transform.position;

            if (terrain != null)
            {
                float terrainHeight = terrain.SampleHeight(source.position);
                newPosition = new Vector3(source.position.x, terrainHeight + groundOffset, source.position.z);
                foundGround = true;
            }

            if (Physics.Raycast(source.position, Vector3.down, out RaycastHit hit, maxGroundDistance, groundLayer))
            {
                newPosition = hit.point + Vector3.up * groundOffset;
                transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                foundGround = true;
            }

            if (foundGround)
            {
                transform.position = newPosition;

                if (!gameObject.activeSelf)
                    gameObject.SetActive(true);
            }
            else if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
