using TMPro;
using UnityEngine;

namespace PracticeMath.UI
{
    /// <summary>
    /// Sets a TMP label to the player-facing version from <b>Edit → Project Settings → Player → Version</b>
    /// (<see cref="Application.version"/>).
    /// </summary>
    public sealed class GameVersionLabel : MonoBehaviour
    {
        [SerializeField] private TMP_Text versionText;
        [Tooltip("Shown before Application.version, e.g. \"v\" or \"Version \".")]
        [SerializeField] private string prefix = "v";
        [SerializeField] private string suffix = string.Empty;

        private void Awake()
        {
            Apply();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (versionText != null)
                Apply();
        }
#endif

        private void Apply()
        {
            if (versionText == null)
                return;
            versionText.text = $"{prefix}{Application.version}{suffix}";
        }
    }
}
