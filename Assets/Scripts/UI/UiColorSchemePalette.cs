using System;
using UnityEngine;

namespace PracticeMath.UI
{
    /// <summary>Semantic mapping from the four stored swatches (brightest → text, darkest → background).</summary>
    public readonly struct UiThemeResolvedRoles
    {
        public Color Background { get; }
        public Color Text { get; }
        /// <summary>Darker of the two mid tones — tile / primary buttons.</summary>
        public Color Button { get; }
        /// <summary>Lighter of the two mid tones — gear, scroll tint, highlights.</summary>
        public Color Accent { get; }

        public UiThemeResolvedRoles(Color background, Color text, Color button, Color accent)
        {
            Background = background;
            Text = text;
            Button = button;
            Accent = accent;
        }
    }

    [Serializable]
    public struct UiColorSchemePalette
    {
        public string displayName;
        public Color primary;
        public Color secondary;
        public Color tertiary;
        public Color accent;

        public static UiColorSchemePalette FromHex(string name, string primaryHex, string secondaryHex, string tertiaryHex, string accentHex)
        {
            return new UiColorSchemePalette
            {
                displayName = name,
                primary = ParseHex(primaryHex),
                secondary = ParseHex(secondaryHex),
                tertiary = ParseHex(tertiaryHex),
                accent = ParseHex(accentHex),
            };
        }

        /// <summary>Maps the four palette colors by relative brightness.</summary>
        public UiThemeResolvedRoles ResolveRoles()
        {
            var swatches = new[] { primary, secondary, tertiary, accent };
            Array.Sort(swatches, (a, b) => RelativeBrightness(a).CompareTo(RelativeBrightness(b)));

            Color background = swatches[0];
            Color text = swatches[3];
            Color button = swatches[1];
            Color accentRole = swatches[2];
            return new UiThemeResolvedRoles(background, text, button, accentRole);
        }

        internal static float RelativeBrightness(Color c)
        {
            return 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
        }

        private static Color ParseHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return Color.white;
            hex = hex.Trim().TrimStart('#');
            if (hex.Length == 6 &&
                ColorUtility.TryParseHtmlString("#" + hex, out var c))
                return c;
            return Color.magenta;
        }
    }

    public static class UiColorSchemeCatalog
    {
        public const int SchemeCount = 3;

        public static UiColorSchemePalette Get(int index)
        {
            index = Mathf.Clamp(index, 0, SchemeCount - 1);
            return index switch
            {
                0 => UiColorSchemePalette.FromHex("Ocean Play", "425B9A", "76C0EC", "FFF6DC", "FF95A5"),
                1 => UiColorSchemePalette.FromHex("Sunset Bold", "DF301C", "FF9100", "FFF1D0", "0B7CDD"),
                _ => UiColorSchemePalette.FromHex("Warm Slate", "F5EBDD", "F2765E", "315B8C", "413333"),
            };
        }

        public static string GetDisplayName(int index) => Get(index).displayName;
    }
}
