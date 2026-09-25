using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PracticeMath.UI
{
    /// <summary>Fixes admin stats scroll content so summary text is not clipped on the left.</summary>
    public static class AdminAnalyticsLayout
    {
        private const float HorizontalPadding = 24f;
        private const float VerticalPadding = 16f;

        public static void Apply(TMP_Text summaryText, RectTransform scrollContentRoot)
        {
            if (scrollContentRoot != null)
            {
                var contentRt = scrollContentRoot;
                contentRt.sizeDelta = new Vector2(0f, contentRt.sizeDelta.y);

                var vlg = contentRt.GetComponent<VerticalLayoutGroup>();
                if (vlg != null)
                {
                    vlg.padding = new RectOffset(
                        Mathf.RoundToInt(HorizontalPadding),
                        Mathf.RoundToInt(HorizontalPadding),
                        Mathf.RoundToInt(VerticalPadding),
                        Mathf.RoundToInt(VerticalPadding));
                    vlg.childAlignment = TextAnchor.UpperLeft;
                    vlg.childForceExpandWidth = true;
                    vlg.childControlWidth = true;
                }
            }

            if (summaryText == null)
                return;

            summaryText.alignment = TextAlignmentOptions.TopLeft;
            summaryText.textWrappingMode = TextWrappingModes.Normal;
            summaryText.overflowMode = TextOverflowModes.Overflow;
            summaryText.margin = new Vector4(4f, 0f, 4f, 0f);

            var layout = summaryText.GetComponent<LayoutElement>();
            if (layout != null)
            {
                layout.preferredWidth = -1f;
                layout.minWidth = 0f;
                layout.flexibleWidth = 1f;
            }

            var summaryRt = summaryText.rectTransform;
            summaryRt.anchorMin = new Vector2(0f, 1f);
            summaryRt.anchorMax = new Vector2(1f, 1f);
            summaryRt.pivot = new Vector2(0f, 1f);
        }
    }
}
