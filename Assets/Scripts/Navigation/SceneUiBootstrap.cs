using PracticeMath.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
            EnsureHomeNavButtons(scene);
            EnsureThemedCanvases(scene);
        }

        private static void EnsureThemedCanvases(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
                {
                    if (canvas.renderMode != RenderMode.ScreenSpaceOverlay &&
                        canvas.renderMode != RenderMode.ScreenSpaceCamera)
                        continue;

                    UiThemeSceneApplicator.EnsureOn(canvas.gameObject);
                }
            }
        }

        private static void EnsureHomeNavButtons(Scene scene)
        {
            if (!scene.isLoaded)
                return;

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name != "HomeNavButton")
                        continue;
                    if (t.GetComponent<Button>() == null)
                        continue;
                    if (t.GetComponent<HomeNavButtonView>() == null)
                        t.gameObject.AddComponent<HomeNavButtonView>();
                }
            }
        }
    }
}
