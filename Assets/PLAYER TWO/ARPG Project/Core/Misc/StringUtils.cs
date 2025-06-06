using UnityEngine;
using System.Text.RegularExpressions;

namespace PLAYERTWO.ARPGProject
{
using System.Text.RegularExpressions;
using UnityEngine;

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

        /// <summary>
        /// Returns a given string with a color and optional styles (bold/italic).
        /// </summary>
        /// <param name="input">The string you want to format.</param>
        /// <param name="color">The color to apply to the string.</param>
        /// <param name="bold">If true, the text will be bold.</param>
        /// <param name="italic">If true, the text will be italic.</param>
        public static string StringWithColorAndStyle(string input, Color color, bool bold = false, bool italic = false)
        {
            var hex = ColorUtility.ToHtmlStringRGB(color);
            var result = $"<color=#{hex}>{input}</color>";

            if (bold)
                result = $"<b>{result}</b>";

            if (italic)
                result = $"<i>{result}</i>";

            return result;
        }

        public static string GetTMPElementSpriteName(MagicElement element)
        {
            return element switch
            {
                MagicElement.Fire => "magic_element_fire",
                MagicElement.Ice => "magic_element_ice",
                MagicElement.Water => "magic_element_water",
                MagicElement.Earth => "magic_element_earth",
                MagicElement.Air => "magic_element_air",
                MagicElement.Lightning => "magic_element_lightning",
                MagicElement.Shadow => "magic_element_shadow",
                MagicElement.Light => "magic_element_light",
                MagicElement.Arcane => "magic_element_arcane",
                _ => ""
            };
        }

        /// <summary>
        /// Applies multiple styles to a string.
        /// </summary>
        /// <param name="input">The string you want to format.</param>
        /// <param name="bold">If true, the text will be bold.</param>
        /// <param name="italic">If true, the text will be italic.</param>
        public static string ApplyStyles(string input, bool bold = false, bool italic = false)
        {
            if (bold)
                input = $"<b>{input}</b>";

            if (italic)
                input = $"<i>{input}</i>";

            return input;
        }
    }
}