using UnityEngine;
using System.Collections.Generic;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Level/Level Zones")]
    public class LevelZones : Singleton<LevelZones>
    {
        private readonly List<ZoneTrigger> m_zones = new();

        /// <summary>
        /// Read-only list of registered zones.
        /// </summary>
        public IReadOnlyList<ZoneTrigger> zones => m_zones;

        /// <summary>
        /// Registers a zone in the current level.
        /// </summary>
        /// <param name="zone">The zone to register.</param>
        public void RegisterZone(ZoneTrigger zone)
        {
            if (zone && !m_zones.Contains(zone))
                m_zones.Add(zone);
        }

        /// <summary>
        /// Unregisters a zone from the current level.
        /// </summary>
        /// <param name="zone">The zone to unregister.</param>
        public void UnregisterZone(ZoneTrigger zone)
        {
            if (zone)
                m_zones.Remove(zone);
        }

        /// <summary>
        /// Returns the closest registered zone to a given position.
        /// </summary>
        public ZoneTrigger GetClosestZone(Vector3 position)
        {
            ZoneTrigger closest = null;
            float closestDistance = float.PositiveInfinity;
            foreach (var zone in m_zones)
            {
                float distance = Vector3.Distance(position, zone.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = zone;
                }
            }
            return closest;
        }

        /// <summary>
        /// Determines the most appropriate region for a given position.
        /// </summary>
        public ZoneTrigger GetCurrentRegion(Vector3 position)
        {
            ZoneTrigger exact = null;
            float exactDistance = float.PositiveInfinity;

            foreach (var z in m_zones)
            {
                if (!z.requireInside) continue;
                var bounds = z.GetComponent<Collider>().bounds;
                bounds.Expand(1f);
                if (!bounds.Contains(position)) continue;
                float dist = Vector3.Distance(position, z.transform.position);
                if (z.priority > (exact?.priority ?? int.MinValue) ||
                    (z.priority == (exact?.priority ?? int.MinValue) && dist < exactDistance))
                {
                    exact = z;
                    exactDistance = dist;
                }
            }

            if (exact != null)
                return exact;

            ZoneTrigger nearby = null;
            float nearbyDistance = float.PositiveInfinity;

            foreach (var z in m_zones)
            {
                if (z.requireInside) continue;
                float dist = Vector3.Distance(position, z.transform.position);
                if (dist > z.influenceRadius) continue;
                if (z.priority > (nearby?.priority ?? int.MinValue) ||
                    (z.priority == (nearby?.priority ?? int.MinValue) && dist < nearbyDistance))
                {
                    nearby = z;
                    nearbyDistance = dist;
                }
            }

            return nearby;
        }
    }
}
