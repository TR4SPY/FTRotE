using UnityEngine;
using System.Text.RegularExpressions;

namespace PLAYERTWO.ARPGProject
{
    public class StringUtils
    {
        /// <summary>
        /// Returns a given string in Title Case.
        /// </summary>
        /// <param name="input">The string you want to convert to Title Case.</param>
        public static string ConvertToTitleCase(string input)
        {
            var result = Regex.Replace(input, @"([a-z])([A-Z])", "$1 $2");
            result = Regex.Replace(result, @"([A-Z])([A-Z][a-z])", "$1 $2");
            result = char.ToUpper(result[0]) + result.Substring(1);
            return result;
        }

        /// <summary>
        /// Returns a given string with a color in rich text.
        /// </summary>
        /// <param name="input">The string you want to set a color.</param>
        /// <param name="color">The color you want to set.</param>
        public static string StringWithColor(string input, Color color)
        {
            var hex = ColorUtility.ToHtmlStringRGB(color);
            return $"<color=#{hex}>{input}</color>";
        }
    }
}
