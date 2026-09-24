using PracticeMath.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PracticeMath.Navigation
{
    /// <summary>Ensures input works when a scene has UI but no EventSystem (e.g. after editing prefabs).</summary>
    public static class SceneUiBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterSceneLoaded()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public static void BootstrapActiveScene()
        {
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UiEventSystemUtility.EnsureConfigured();
        }
    }
}
