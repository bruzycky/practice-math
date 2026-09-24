using UnityEngine;

namespace PracticeMath.UI
{
    /// <summary>
    /// Root marker for <c>Assets/Prefabs/UI/HomeHubSettingsOverlay.prefab</c>.
    /// Edit that prefab in the Project window — it is not built at runtime.
    /// </summary>
    public sealed class HomeHubSettingsOverlayRoot : MonoBehaviour
    {
        [SerializeField] private HomeHubSettingsView settingsView;

        public HomeHubSettingsView SettingsView => settingsView;

        private void Reset()
        {
            if (settingsView == null)
                settingsView = GetComponent<HomeHubSettingsView>();
        }
    }
}
