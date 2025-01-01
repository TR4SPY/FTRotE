using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    public class CharacterScenes
    {
        public class Scene
        {
            public string name;
            public int currentWaypointIndex;
            public Entity[] entities;
            public Waypoint[] waypoints;
            public Interactive[] interactives;
            public GameObject[] gameObjects;
        }

        public class Entity
        {
            public Vector3 position;
            public Vector3 rotation;
            public int health;
        }

        public class Waypoint
        {
            public string title;
            public bool active;
        }

        public class Interactive
        {
            public bool interactive;
        }

        public class GameObject
        {
            public bool active;
            public Vector3 position;
            public Vector3 rotation;
        }

        public List<Scene> scenes;

        public virtual void InitializeScenes()
        {
            if (scenes != null) return;

            scenes = new List<Scene>();
        }

        /// <summary>
        /// Updates all scene data from a given Level.
        /// </summary>
        /// <param name="level">The Level you want to update the scene data from.</param>
        public virtual void UpdateScene(Level level)
        {
            if (!level) return;

            var name = level.currentScene.name;
            var currentWaypointIndex = level.waypoints.currentWaypointIndex;

            var entities = level.entities.Select(e => new Entity()
            {
                position = e.position,
                rotation = e.transform.rotation.eulerAngles,
                health = e.stats.health
            }).ToArray();

            var waypoints = level.waypoints.waypoints.Select(w => new Waypoint()
            {
                title = w.title,
                active = w.active
            }).ToArray();

            var interactives = GetInteractives(level);
            var gameObjects = GetGameObjects(level);

            if (!scenes.Exists(scene => scene.name == name))
                scenes.Add(new Scene() { name = name });

            var scene = scenes.Find(scene => scene.name == name);

            scene.entities = entities;
            scene.currentWaypointIndex = currentWaypointIndex;
            scene.waypoints = waypoints;
            scene.interactives = interactives;
            scene.gameObjects = gameObjects;
        }

        /// <summary>
        /// Tries to get a Scene bu its name.
        /// </summary>
        /// <param name="name">The name of the scene you want to get.</param>
        /// <param name="scene">The instance of the Scene you want to find.</param>
        /// <returns>Returns true if it was able to get the scene.</returns>
        public virtual bool TryGetScene(string name, out Scene scene)
        {
            scene = scenes.Find(scene => scene.name == name);
            return scene != null;
        }

        protected virtual Interactive[] GetInteractives(Level level)
        {
            return level.interactives?.Select(i => new Interactive()
            {
                interactive = i.interactive
            }).ToArray();
        }

        protected virtual GameObject[] GetGameObjects(Level level)
        {
            return level.gameObjects?.Select(g => new GameObject()
            {
                position = g.transform.position,
                rotation = g.transform.rotation.eulerAngles,
                active = g.activeSelf
            }).ToArray();
        }

        public static CharacterScenes CreateFromSerializer(ScenesSerializer serializer)
        {
            if (serializer == null) return new CharacterScenes();

            return new CharacterScenes()
            {
                scenes = serializer.scenes.Select(scene => new Scene()
                {
                    name = scene.name,
                    currentWaypointIndex = scene.currentWaypointIndex,
                    entities = scene.entities.Select(e => new Entity()
                    {
                        position = e.position.ToUnity(),
                        rotation = e.rotation.ToUnity(),
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
                        position = g.position.ToUnity(),
                        rotation = g.rotation.ToUnity(),
                        active = g.active
                    }).ToArray()
                }).ToList()
            };
        }
    }
}
