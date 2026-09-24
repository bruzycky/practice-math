using UnityEngine;

namespace PracticeMath.UI
{
    public readonly struct UiThemeGridColors
    {
        public Color ProductCell { get; }
        public Color HeaderCell { get; }
        public Color SelectedHeader { get; }
        public Color ColumnStrip { get; }
        public Color RowStrip { get; }
        public Color Intersection { get; }
        public Color ProductText { get; }
        public Color IntersectionText { get; }
        public Color CornerCell { get; }
        public Color PendingProduct { get; }
        public Color PendingHeader { get; }
        public Color PendingOutline { get; }

        public UiThemeGridColors(
            Color productCell,
            Color headerCell,
            Color selectedHeader,
            Color columnStrip,
            Color rowStrip,
            Color intersection,
            Color productText,
            Color intersectionText,
            Color cornerCell,
            Color pendingProduct,
            Color pendingHeader,
            Color pendingOutline)
        {
            ProductCell = productCell;
            HeaderCell = headerCell;
            SelectedHeader = selectedHeader;
            ColumnStrip = columnStrip;
            RowStrip = rowStrip;
            Intersection = intersection;
            ProductText = productText;
            IntersectionText = intersectionText;
            CornerCell = cornerCell;
            PendingProduct = pendingProduct;
            PendingHeader = pendingHeader;
            PendingOutline = pendingOutline;
        }

        public static UiThemeGridColors FromRoles(UiThemeResolvedRoles roles)
        {
            Color header = roles.Accent;
            Color selected = Color.Lerp(roles.Accent, roles.Text, 0.45f);
            Color intersection = Color.Lerp(roles.Button, roles.Text, 0.55f);
            return new UiThemeGridColors(
                productCell: roles.Button,
                headerCell: header,
                selectedHeader: selected,
                columnStrip: Color.Lerp(roles.Accent, roles.Button, 0.35f),
                rowStrip: Color.Lerp(roles.Button, roles.Accent, 0.4f),
                intersection: intersection,
                productText: roles.Text,
                intersectionText: roles.Background,
                cornerCell: WithAlpha(roles.Background, 0.55f),
                pendingProduct: Color.Lerp(roles.Button, roles.Background, 0.25f),
                pendingHeader: Color.Lerp(roles.Accent, roles.Background, 0.2f),
                pendingOutline: WithAlpha(roles.Text, 0.92f));
        }

        private static Color WithAlpha(Color c, float a)
        {
            c.a = a;
            return c;
        }
    }

    public static class UiThemeGridColorsProvider
    {
        public static UiThemeGridColors Current { get; private set; } = UiThemeGridColors.FromRoles(
            UiColorSchemeCatalog.Get(0).ResolveRoles());

        public static void ApplyRoles(UiThemeResolvedRoles roles)
        {
            Current = UiThemeGridColors.FromRoles(roles);
        }
    }
}
