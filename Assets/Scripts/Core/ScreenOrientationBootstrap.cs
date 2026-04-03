using UnityEngine;

namespace PracticeMath.Core
{
    /// <summary>
    /// Locks the app to landscape (both left and right) on builds where <see cref="Screen.orientation"/> applies.
    /// Complements Player Settings (Default Orientation + Supported Autorotation).
    /// </summary>
    public static class ScreenOrientationBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplyLandscapeOnly()
        {
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.AutoRotation;
        }
    }
}
