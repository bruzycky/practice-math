using PracticeMath.Analytics;
using PracticeMath.Navigation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PracticeMath.UI
{
    /// <summary>Settings and admin sub-panels on the home hub.</summary>
    public sealed class HomeHubSettingsView : MonoBehaviour
    {
        [SerializeField] private HomeHubController hub;
        [SerializeField] private GameObject settingsPanelRoot;
        [SerializeField] private GameObject adminPanelRoot;
        [SerializeField] private TMP_Dropdown colorSchemeDropdown;
        [SerializeField] private AnalyticsPanelView analyticsPanel;

        private void Start()
        {
            if (colorSchemeDropdown != null)
            {
                colorSchemeDropdown.options.Clear();
                for (int i = 0; i < UiColorSchemeCatalog.SchemeCount; i++)
                    colorSchemeDropdown.options.Add(new TMP_Dropdown.OptionData(UiColorSchemeCatalog.GetDisplayName(i)));

                var theme = AppThemeContext.Instance;
                if (theme != null)
                {
                    int idx = theme.SelectedSchemeIndex;
                    colorSchemeDropdown.SetValueWithoutNotify(idx);
                    colorSchemeDropdown.RefreshShownValue();
                }

                colorSchemeDropdown.onValueChanged.AddListener(OnColorSchemeChanged);
            }

            if (!Application.isPlaying)
                return;

            if (settingsPanelRoot != null)
                settingsPanelRoot.SetActive(false);
            if (adminPanelRoot != null)
                adminPanelRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (colorSchemeDropdown != null)
                colorSchemeDropdown.onValueChanged.RemoveListener(OnColorSchemeChanged);
        }

        public void OpenSettings()
        {
            UiEventSystemUtility.ClearCurrentSelection();
            hub?.SetHubVisible(false);
            if (adminPanelRoot != null)
                adminPanelRoot.SetActive(false);
            if (settingsPanelRoot != null)
                settingsPanelRoot.SetActive(true);
        }

        public void CloseSettingsToHub()
        {
            UiEventSystemUtility.ClearCurrentSelection();
            if (settingsPanelRoot != null)
                settingsPanelRoot.SetActive(false);
            if (adminPanelRoot != null)
                adminPanelRoot.SetActive(false);
            hub?.SetHubVisible(true);
        }

        public void OpenAdminTracking()
        {
            if (settingsPanelRoot != null)
                settingsPanelRoot.SetActive(false);
            if (adminPanelRoot != null)
                adminPanelRoot.SetActive(true);
            analyticsPanel?.Refresh();
        }

        public void CloseAdminToSettings()
        {
            if (adminPanelRoot != null)
                adminPanelRoot.SetActive(false);
            if (settingsPanelRoot != null)
                settingsPanelRoot.SetActive(true);
        }

        private void OnColorSchemeChanged(int index)
        {
            if (colorSchemeDropdown != null)
                colorSchemeDropdown.Hide();
            UiEventSystemUtility.ClearCurrentSelection();
            AppThemeContext.Instance?.SetSelectedScheme(index);
        }
    }
}
