using PracticeMath.Navigation;
using UnityEngine;

namespace PracticeMath.UI
{
    /// <summary>Ensures practice bg + theme on any learning canvas (hub, activities, practice scene).</summary>
    [DisallowMultipleComponent]
    public sealed class UiThemeSceneApplicator : MonoBehaviour
    {
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
            UiPracticeBackgroundView.Ensure(transform);
            var roles = AppThemeContext.Instance != null
                ? AppThemeContext.Instance.CurrentRoles
                : UiColorSchemeCatalog.Get(0).ResolveRoles();
            UiThemeStyler.ApplyCanvas(transform, roles);
        }

        public static void EnsureOn(GameObject canvasRoot)
        {
            if (canvasRoot == null || canvasRoot.GetComponent<Canvas>() == null)
                return;
            UiPracticeBackgroundView.Ensure(canvasRoot.transform);
            if (canvasRoot.GetComponent<UiThemeSceneApplicator>() == null)
                canvasRoot.AddComponent<UiThemeSceneApplicator>();
        }
    }
}
