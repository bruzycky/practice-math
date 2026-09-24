using PracticeMath.Navigation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PracticeMath.UI
{
    /// <summary>Applies the active color scheme to the home hub canvas.</summary>
    public sealed class UiHomeThemeApplicator : MonoBehaviour
    {
        [SerializeField] private Image canvasBackground;
        [SerializeField] private Image scrollBackdrop;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private TMP_Dropdown gradeDropdown;
        [SerializeField] private HomeNavTileView[] navTiles;
        [SerializeField] private Image settingsGearBackground;
        [SerializeField] private Image settingsPanelBackground;
        [SerializeField] private Image adminPanelBackground;

        private void Awake()
        {
            if (gradeDropdown == null)
            {
                var header = transform.Find("Header");
                if (header != null)
                    gradeDropdown = header.GetComponentInChildren<TMP_Dropdown>(true);
            }
        }

        private void OnEnable()
        {
            if (AppThemeContext.Instance != null)
                AppThemeContext.Instance.Changed += Apply;
            Apply();
        }

        private void OnDisable()
        {
            if (AppThemeContext.Instance != null)
                AppThemeContext.Instance.Changed -= Apply;
        }

        public void Apply()
        {
            var palette = AppThemeContext.Instance != null
                ? AppThemeContext.Instance.CurrentPalette
                : UiColorSchemeCatalog.Get(0);
            var roles = palette.ResolveRoles();

            if (canvasBackground != null)
                canvasBackground.color = roles.Background;

            if (scrollBackdrop != null)
                scrollBackdrop.color = WithAlpha(roles.Accent, 0.28f);

            if (titleText != null)
                titleText.color = roles.Text;
            if (subtitleText != null)
                subtitleText.color = WithAlpha(roles.Text, 0.82f);

            ThemeGradeDropdown(roles);

            if (navTiles != null)
            {
                foreach (var tile in navTiles)
                {
                    if (tile == null)
                        continue;
                    var img = tile.GetComponent<Image>();
                    if (img != null)
                        img.color = roles.Button;

                    var title = tile.transform.Find("TileTitle")?.GetComponent<TMP_Text>();
                    if (title != null)
                        title.color = roles.Text;

                    var desc = tile.transform.Find("TileDesc")?.GetComponent<TMP_Text>();
                    if (desc != null)
                        desc.color = WithAlpha(roles.Text, 0.88f);
                }
            }

            if (settingsGearBackground != null)
                settingsGearBackground.color = roles.Accent;
            ThemeGearIcon(roles.Text);

            ThemeOverlayPanel(settingsPanelBackground, roles);
            ThemeOverlayPanel(adminPanelBackground, roles);
        }

        private void ThemeGradeDropdown(UiThemeResolvedRoles roles)
        {
            if (gradeDropdown == null)
                return;

            if (gradeDropdown.targetGraphic is Image dropBg)
                dropBg.color = roles.Button;
            if (gradeDropdown.captionText != null)
                gradeDropdown.captionText.color = roles.Text;
        }

        private void ThemeGearIcon(Color text)
        {
            if (settingsGearBackground == null)
                return;
            var icon = settingsGearBackground.transform.Find("Icon")?.GetComponent<TMP_Text>();
            if (icon != null)
                icon.color = text;
        }

        private static void ThemeOverlayPanel(Image panelBackground, UiThemeResolvedRoles roles)
        {
            if (panelBackground == null)
                return;

            panelBackground.color = roles.Background;
            var root = panelBackground.transform;

            foreach (var tmp in root.GetComponentsInChildren<TMP_Text>(true))
                tmp.color = tmp.color.a < 0.99f ? WithAlpha(roles.Text, tmp.color.a) : roles.Text;

            foreach (var img in root.GetComponentsInChildren<Image>(true))
            {
                if (img == panelBackground)
                    continue;

                var btn = img.GetComponent<Button>();
                if (btn != null && btn.targetGraphic == img)
                {
                    img.color = roles.Button;
                    continue;
                }

                if (img.GetComponent<ScrollRect>() != null)
                    img.color = WithAlpha(roles.Accent, 0.22f);
            }

            foreach (var dropdown in root.GetComponentsInChildren<TMP_Dropdown>(true))
            {
                if (dropdown.targetGraphic is Image ddBg)
                    ddBg.color = roles.Button;
                if (dropdown.captionText != null)
                    dropdown.captionText.color = roles.Text;
            }
        }

        private static Color WithAlpha(Color c, float a)
        {
            c.a = a;
            return c;
        }
    }
}
