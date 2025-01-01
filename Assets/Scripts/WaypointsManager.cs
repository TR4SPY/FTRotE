using UnityEngine;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    public class WaypointsManager : MonoBehaviour
    {
        private void Start()
        {
            AssignWaypointIDs();
        }

        private void AssignWaypointIDs()
        {
            var waypoints = GetComponentsInChildren<Waypoint>();
            for (int i = 0; i < waypoints.Length; i++)
            {
                waypoints[i].waypointID = i; // Przypisanie unikalnego ID na podstawie indeksu
                Debug.Log($"Assigned ID {i} to Waypoint '{waypoints[i].name}'");
            }
        }
    }
}
