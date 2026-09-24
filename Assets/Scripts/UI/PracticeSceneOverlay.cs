using PracticeMath.Navigation;
using UnityEngine;

namespace PracticeMath.UI
{
    /// <summary>Adds a Home button and applies hub grade to the practice scene.</summary>
    [DefaultExecutionOrder(-200)]
    public sealed class PracticeSceneOverlay : MonoBehaviour
    {
        private void Awake()
        {
            UiRuntimeFactory.EnsureEventSystem();

            var practice = FindFirstObjectByType<PracticeProblemController>(FindObjectsInactive.Include);
            practice?.ApplyHubSessionSettings();

            var existingCanvas = FindFirstObjectByType<Canvas>();
            if (existingCanvas == null)
                return;

            UiHomeNavButton.AddTo(existingCanvas.GetComponent<RectTransform>());
        }
    }
}
