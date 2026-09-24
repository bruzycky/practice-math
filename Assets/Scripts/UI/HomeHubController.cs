using PracticeMath.Core;
using PracticeMath.Navigation;
using TMPro;
using UnityEngine;

namespace PracticeMath.UI
{
    /// <summary>Hub grade picker and navigation tiles. Layout is authored on a Canvas prefab or in the Home scene.</summary>
    public sealed class HomeHubController : MonoBehaviour
    {
        [SerializeField] private GameObject headerRoot;
        [SerializeField] private GameObject scrollRoot;
        [SerializeField] private GameObject settingsGearButton;
        [SerializeField] private TMP_Dropdown gradeDropdown;
        [SerializeField] private HomeNavTileView[] navTiles;
        [SerializeField] private HomeHubSettingsView settingsView;

        private void Start()
        {
            var ctx = AppSessionContext.Instance;
            if (gradeDropdown != null)
            {
                if (ctx != null)
                {
                    int idx = (int)ctx.SelectedGrade - 1;
                    gradeDropdown.SetValueWithoutNotify(Mathf.Clamp(idx, 0, gradeDropdown.options.Count - 1));
                    gradeDropdown.RefreshShownValue();
                }

                gradeDropdown.onValueChanged.AddListener(OnGradeChanged);
                OnGradeChanged(gradeDropdown.value);
            }

            if (navTiles != null)
            {
                foreach (var tile in navTiles)
                {
                    if (tile != null)
                        tile.Bind(this);
                }
            }
        }

        private void OnDestroy()
        {
            if (gradeDropdown != null)
                gradeDropdown.onValueChanged.RemoveListener(OnGradeChanged);
        }

        public void LaunchModule(LearningModule module)
        {
            GameSceneLoader.Launch(module);
        }

        public void OpenSettings()
        {
            settingsView?.OpenSettings();
        }

        public void SetHubVisible(bool visible)
        {
            if (!visible)
                UiEventSystemUtility.ClearCurrentSelection();

            if (headerRoot != null)
                headerRoot.SetActive(visible);
            if (scrollRoot != null)
                scrollRoot.SetActive(visible);
            if (settingsGearButton != null)
                settingsGearButton.SetActive(visible);
        }

        private void OnGradeChanged(int index)
        {
            var ctx = AppSessionContext.Instance;
            if (ctx == null)
                return;
            ctx.SetSelectedGrade((GradeLevel)(index + 1));
        }
    }
}
