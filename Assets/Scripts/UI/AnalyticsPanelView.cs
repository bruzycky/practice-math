using PracticeMath.Analytics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PracticeMath.UI
{
    /// <summary>
    /// Shows <see cref="PracticeSessionAnalytics"/> in a TMP label. Assign the same analytics instance
    /// as the practice / keypad. Optionally wire <see cref="TogglePanel"/> and <see cref="ResetStatsFromUi"/> to buttons.
    /// </summary>
    public sealed class AnalyticsPanelView : MonoBehaviour
    {
        [SerializeField] private PracticeSessionAnalytics analytics;
        [SerializeField] private TMP_Text summaryText;
        [Tooltip("Scroll view Content (RectTransform). If empty, uses the TMP’s parent. Rebuilt after text changes.")]
        [SerializeField] private RectTransform scrollContentRoot;
        [Tooltip("Optional root to show/hide with TogglePanel.")]
        [SerializeField] private GameObject panelRoot;

        private void OnEnable()
        {
            if (analytics != null)
                analytics.Changed += OnAnalyticsChanged;
            Refresh();
        }

        private void OnDisable()
        {
            if (analytics != null)
                analytics.Changed -= OnAnalyticsChanged;
        }

        private void OnAnalyticsChanged()
        {
            Refresh();
        }

        /// <summary>Call from a UI button to open/close the panel.</summary>
        public void TogglePanel()
        {
            if (panelRoot != null)
                panelRoot.SetActive(!panelRoot.activeSelf);
            Refresh();
        }

        /// <summary>Force-refresh the label (e.g. when opening the panel).</summary>
        public void Refresh()
        {
            if (summaryText == null || analytics == null)
                return;
            summaryText.text = analytics.GetFormattedSummary();
            summaryText.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();

            RectTransform content = scrollContentRoot != null
                ? scrollContentRoot
                : summaryText.transform.parent as RectTransform;
            if (content != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            LayoutRebuilder.ForceRebuildLayoutImmediate(summaryText.rectTransform);
        }

        /// <summary>Wire to a “Reset stats” or “New session” button.</summary>
        public void ResetStatsFromUi()
        {
            analytics?.ResetSession();
            Refresh();
        }

        /// <summary>Wire to a hidden admin control — deletes the JSON save file contents.</summary>
        public void ResetAllPersistentFromUi()
        {
            analytics?.ResetAllPersistentData();
            Refresh();
        }
    }
}
