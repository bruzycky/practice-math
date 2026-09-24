using UnityEngine;

namespace PracticeMath.UI
{
    /// <summary>Applies hub grade when the practice scene loads. Add a Home button on the Canvas in the editor.</summary>
    [DefaultExecutionOrder(-200)]
    public sealed class PracticeSceneOverlay : MonoBehaviour
    {
        private void Awake()
        {
            var practice = FindFirstObjectByType<PracticeProblemController>(FindObjectsInactive.Include);
            practice?.ApplyHubSessionSettings();
        }
    }
}
