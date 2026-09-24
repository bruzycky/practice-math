using PracticeMath.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PracticeMath.UI
{
    /// <summary>One hub tile; assign module and wire the button in the Inspector (or on the same GameObject).</summary>
    public sealed class HomeNavTileView : MonoBehaviour
    {
        [SerializeField] private LearningModule module;
        [SerializeField] private Button button;

        public LearningModule Module => module;

        private void Reset()
        {
            if (button == null)
                button = GetComponent<Button>();
        }

        public void Bind(HomeHubController hub)
        {
            if (button == null)
                button = GetComponent<Button>();
            if (button == null || hub == null)
                return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => hub.LaunchModule(module));
        }
    }
}
