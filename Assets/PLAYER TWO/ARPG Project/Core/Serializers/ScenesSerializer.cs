using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [System.Serializable]
    public class ScenesSerializer
    {
        [System.Serializable]
        public class Scene
        {
            public string name;
            public int currentWaypointIndex;
            public Entity[] entities;
            public Waypoint[] waypoints;
            public Interactive[] interactives;
            public GameObject[] gameObjects;
        }

        [System.Serializable]
        public class Entity
        {
            public UnitySerializer.Vector3 position;
            public UnitySerializer.Vector3 rotation;
            public int health;
        }

        [System.Serializable]
        public class Waypoint
        {
            public string title;
            public bool active;
        }

        [System.Serializable]
        public class Interactive
        {
            public bool interactive;
        }

        [System.Serializable]
        public class GameObject
        {
            public bool active;
            public UnitySerializer.Vector3 position;
            public UnitySerializer.Vector3 rotation;
        }

        public List<Scene> scenes;

        public ScenesSerializer(CharacterScenes scenes)
        {
            this.scenes = new List<Scene>();

            if (scenes.scenes == null) return;

            foreach (var scene in scenes.scenes)
            {
                this.scenes.Add(new Scene()
                {
                    name = scene.name,
                    currentWaypointIndex = scene.currentWaypointIndex,
                    entities = scene.entities.Select(e => new Entity()
                    {
                        position = new UnitySerializer.Vector3(e.position),
                        rotation = new UnitySerializer.Vector3(e.rotation),
                        health = e.health
                    }).ToArray(),
                    waypoints = scene.waypoints.Select(w => new Waypoint()
                    {
                        title = w.title,
                        active = w.active
                    }).ToArray(),
                    interactives = scene.interactives?.Select(i => new Interactive()
                    {
                        interactive = i.interactive
                    }).ToArray(),
                    gameObjects = scene.gameObjects?.Select(g => new GameObject()
                    {
                        position = new UnitySerializer.Vector3(g.position),
                        rotation = new UnitySerializer.Vector3(g.rotation),
                        active = g.active
                    }).ToArray()
                });
            }
        }

        public virtual void ToJson() => JsonUtility.ToJson(this);

        public static ScenesSerializer FromJson(string json) =>
            JsonUtility.FromJson<ScenesSerializer>(json);
    }
}
