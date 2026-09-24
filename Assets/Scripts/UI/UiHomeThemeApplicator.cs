using UnityEngine;

namespace PracticeMath.UI
{
    /// <summary>Legacy component on older prefabs — forwards to <see cref="UiThemeSceneApplicator"/>.</summary>
    public sealed class UiHomeThemeApplicator : MonoBehaviour
    {
        private void Awake()
        {
            UiThemeSceneApplicator.EnsureOn(gameObject);
            Destroy(this);
        }
    }
}
