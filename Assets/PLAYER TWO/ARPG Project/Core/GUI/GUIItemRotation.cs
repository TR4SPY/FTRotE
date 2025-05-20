using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public class GUIItemRotation : MonoBehaviour
    {
        private Quaternion initialRotation;

        public Transform target;
        public float rotationSpeed = 360f;

        public bool isHovered = false;
        public bool isDragging = false;

        void Start()
        {
            if (target != null)
                initialRotation = target.rotation;
        }

        void Update()
        {
            if (target == null)
                return;

            if (isHovered && !isDragging)
            {
                target.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            }
            else
            {
                target.rotation = Quaternion.RotateTowards(
                    target.rotation,
                    initialRotation,
                    rotationSpeed * 2f * Time.deltaTime
                );
            }
        }
    }
}