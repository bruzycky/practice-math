using UnityEngine;

namespace PracticeMath.UI
{
    /// <summary>Consistent top-left Home control for runtime-built screens.</summary>
    public static class UiHomeNavButton
    {
        public static void AddTo(RectTransform parent)
        {
            if (parent == null)
                return;

            if (parent.Find("HomeNavButton") != null)
                return;

            var btn = UiRuntimeFactory.CreateButton(parent, "Home", new Vector2(180f, 64f), null);
            btn.gameObject.name = "HomeNavButton";
            if (btn.GetComponent<HomeNavButtonView>() == null)
                btn.gameObject.AddComponent<HomeNavButtonView>();
            var rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(16f, -16f);
        }
    }
}
