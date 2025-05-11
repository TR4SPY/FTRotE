using UnityEngine;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    public class GameTags : MonoBehaviour
    {
        public static string Untagged = "Untagged";
        public static string Player = "Entity/Player";
        public static string Enemy = "Entity/Enemy";
        public static string Summoned = "Entity/Summoned";
        public static string Interactive = "Interactive";
        public static string Destructible = "Destructible";
        public static string Collectible = "Collectible";

        /// <summary>
        /// Returns true if the tag of a given Game Object matches the targets tags.
        /// </summary>
        public static bool IsTarget(GameObject gameObject) =>
            gameObject && (gameObject.CompareTag(Enemy) || gameObject.CompareTag(Destructible));

        /// <summary>
        /// Returns true if the tag of a given Game Object matches the interactives tags.
        /// </summary>
        public static bool IsInteractive(GameObject gameObject) =>
            gameObject && (gameObject.CompareTag(Interactive) || gameObject.CompareTag(Collectible));

        /// <summary>
        /// Returns true if the Game Object of a given Collider has a tag in the tag list.
        /// </summary>
        /// <param name="collider">The Collider you want to get read tag from.</param>
        /// <param name="list">The tag list you want to check.</param>
        public static bool InTagList(Collider collider, List<string> list) =>
            InTagList(collider.gameObject, list);

        /// <summary>
        /// Returns true if the Game Object has a tag in the tag list.
        /// </summary>
        /// <param name="gameObject">The Game Object you want to read the tag from.</param>
        /// <param name="list">The tag list you want to check.</param>
        public static bool InTagList(GameObject gameObject, List<string> list) =>
            list != null && list.Contains(gameObject.tag);

        /// <summary>
        /// Returns true if the Game Object is an Entity.
        /// </summary>
        /// <param name="gameObject">The Game Object you want to check.</param>
        /// <returns>Returns true if the Game Object is an Entity.</returns>
        public static bool IsEntity(GameObject gameObject) =>
            gameObject && (gameObject.CompareTag(Player) ||
            gameObject.CompareTag(Enemy) || gameObject.CompareTag(Summoned));
    }
}
