using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PracticeMath.UI
{
    /// <summary>Applies resolved theme roles to a canvas hierarchy (shared bg image stays visible).</summary>
    public static class UiThemeStyler
    {
        private static readonly string[] PanelOverlayNames = { "SettingsPanel", "AdminPanel" };

        public static void ApplyCanvas(Transform canvasRoot, UiThemeResolvedRoles roles)
        {
            if (canvasRoot == null)
                return;

            UiThemeGridColorsProvider.ApplyRoles(roles);

            foreach (var tmp in canvasRoot.GetComponentsInChildren<TMP_Text>(true))
            {
                if (IsUnderPracticeBackground(tmp.transform))
                    continue;
                if (IsButtonLabel(tmp))
                    tmp.color = roles.Text;
                else
                    tmp.color = WithAlpha(roles.Text, tmp.color.a < 0.99f ? tmp.color.a : 1f);
            }

            foreach (var dropdown in canvasRoot.GetComponentsInChildren<TMP_Dropdown>(true))
            {
                if (dropdown.targetGraphic is Image ddBg)
                    ddBg.color = roles.Button;
                if (dropdown.captionText != null)
                    dropdown.captionText.color = roles.Text;
            }

            foreach (var img in canvasRoot.GetComponentsInChildren<Image>(true))
            {
                if (ShouldSkipImage(img))
                    continue;

                if (IsPanelOverlay(img))
                {
                    img.color = WithAlpha(roles.Background, 0.94f);
                    continue;
                }

                var btn = img.GetComponent<Button>();
                if (btn != null && btn.targetGraphic == img)
                {
                    img.color = roles.Button;
                    continue;
                }

                if (img.GetComponent<ScrollRect>() != null)
                {
                    img.color = WithAlpha(roles.Accent, 0.26f);
                    continue;
                }

                if (img.GetComponentInParent<HomeNavTileView>(true) != null && img.GetComponent<HomeNavTileView>() != null)
                {
                    img.color = roles.Button;
                    continue;
                }

                if (img.name == "SettingsGearButton" || img.transform.parent?.name == "SettingsGearButton")
                {
                    if (img.gameObject.name == "SettingsGearButton")
                        img.color = roles.Accent;
                    continue;
                }

                if (img.name == "Scroll" || img.transform.parent?.name == "Scroll")
                {
                    if (img.gameObject.name == "Scroll")
                        img.color = WithAlpha(roles.Accent, 0.26f);
                }
            }

            foreach (var grid in canvasRoot.GetComponentsInChildren<TimesTableGridController>(true))
                grid.ApplyTheme();
        }

        private static bool ShouldSkipImage(Image img)
        {
            if (img == null)
                return true;
            if (img.GetComponent<UiPracticeBackgroundView>() != null)
                return true;
            if (img.gameObject.name == UiPracticeBackgroundView.ObjectName)
                return true;
            return false;
        }

        private static bool IsUnderPracticeBackground(Transform t)
        {
            return t != null && t.parent != null && t.parent.name == UiPracticeBackgroundView.ObjectName;
        }

        private static bool IsButtonLabel(TMP_Text tmp)
        {
            return tmp.GetComponentInParent<Button>(true) != null;
        }

        private static bool IsPanelOverlay(Image img)
        {
            foreach (var name in PanelOverlayNames)
            {
                if (img.gameObject.name == name)
                    return true;
            }

            return false;
        }

        private static Color WithAlpha(Color c, float a)
        {
            c.a = a;
            return c;
        }
    }
}
