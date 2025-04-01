using UnityEngine;
using TMPro;

namespace PLAYERTWO.ARPGProject
{
    public class ChatOverhead : MonoBehaviour
    {
        public TMP_Text text;
        public Vector3 offset = new Vector3(0, 2.2f, 0);

        private Transform target;
        private float lifetime;
        private Camera cam;

        public static void Show(Transform who, string msg, float duration = 3f)
        {
            var prefab = Resources.Load<GameObject>("UI/ChatOverhead"); // W Resources/UI/ChatOverhead.prefab
            if (prefab == null) return;

            var instance = Instantiate(prefab);
            var bubble = instance.GetComponent<ChatOverhead>();
            bubble.Attach(who, msg, duration);
        }

        public void Attach(Transform who, string msg, float duration)
        {
            target = who;
            text.text = msg;
            lifetime = duration;
            cam = Camera.main;
        }

        private void Update()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            lifetime -= Time.deltaTime;
            if (lifetime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            transform.position = target.position + offset;

            if (cam != null)
                transform.forward = cam.transform.forward;
        }
    }
}
