using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public class RotateItem: MonoBehaviour
    {
        public float rotationSpeed = 50f;

        void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}