using PracticeMath.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PracticeMath.Navigation
{
    /// <summary>Builds runtime UI whenever a scene loads (including SceneManager.LoadScene during play).</summary>
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

            switch (scene.name)
            {
                case GameScenes.Home:
                    if (Object.FindFirstObjectByType<HomeHubController>() == null)
                    {
                        var home = new GameObject("HomeHub");
                        home.AddComponent<HomeHubController>();
                    }
                    break;

                case GameScenes.TimesTables:
                    if (Object.FindFirstObjectByType<TimesTablePracticeController>() == null)
                    {
                        var times = new GameObject("TimesTables");
                        times.AddComponent<TimesTablePracticeController>();
                    }
                    break;

                case GameScenes.MultipleChoice:
                    if (Object.FindFirstObjectByType<MultipleChoiceActivityController>() == null)
                    {
                        var mc = new GameObject("MultipleChoice");
                        mc.AddComponent<MultipleChoiceActivityController>();
                    }
                    break;

                case GameScenes.PracticeMath:
                    if (Object.FindFirstObjectByType<PracticeSceneOverlay>() == null)
                    {
                        var overlay = new GameObject("PracticeSceneOverlay");
                        overlay.AddComponent<PracticeSceneOverlay>();
                    }
                    break;
            }
        }
    }
}
