using PracticeMath.Navigation;
using UnityEngine;
using UnityEngine.UI;

namespace PracticeMath.UI
{
    /// <summary>Wires the panel Home button at runtime. Prefab onClick lists are often empty after layout generation.</summary>
    [RequireComponent(typeof(Button))]
    public sealed class HomeNavButtonView : MonoBehaviour
    {
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnHomeClicked);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnHomeClicked);
        }

        private static void OnHomeClicked() => GameSceneLoader.LoadHome();
    }
}
