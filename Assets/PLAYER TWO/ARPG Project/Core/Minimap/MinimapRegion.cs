using UnityEngine;
using UnityEngine.UI;
namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Minimap/Minimap Region")]
    public class MinimapRegion : MonoBehaviour
    {
        [Tooltip("The Text component for the region label.")]
        public Text regionLabel;
        Transform target;

        void Start()
        {
            target = Level.instance.player.transform;
        }

        void LateUpdate()
        {
            if (regionLabel == null || target == null) return;

            var zone = AI_DDA.Assets.Scripts.ZoneTrigger.GetCurrentRegion(target.position);
            string zoneName = zone != null ? zone.zoneName : "Wilderness";

            Vector3 pos = target.position;
            int z = Mathf.RoundToInt(pos.z);
            int x = Mathf.RoundToInt(pos.x);
            int y = Mathf.RoundToInt(pos.y);
            string ns = z >= 0 ? $"{z}N" : $"{-z}S";
            string ew = x >= 0 ? $"{x}E" : $"{-x}W";

            regionLabel.text = $"{zoneName} | ({ns}, {ew}, {y}m)";
        }
    }
}
