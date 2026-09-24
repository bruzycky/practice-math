using UnityEngine;

namespace PracticeMath.UI
{
    /// <summary>Shared UI art references. Asset lives at Resources/PracticeMathUiAssets.asset.</summary>
    [CreateAssetMenu(fileName = "PracticeMathUiAssets", menuName = "Practice Math/UI Assets")]
    public sealed class PracticeMathUiAssets : ScriptableObject
    {
        [SerializeField] private Sprite screenBackground;

        private static PracticeMathUiAssets _instance;

        public static PracticeMathUiAssets Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                _instance = Resources.Load<PracticeMathUiAssets>("PracticeMathUiAssets");
                return _instance;
            }
        }

        public Sprite ScreenBackground => screenBackground;
    }
}
