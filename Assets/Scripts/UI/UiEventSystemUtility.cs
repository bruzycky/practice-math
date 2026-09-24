using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace PracticeMath.UI
{
    /// <summary>Single persisted EventSystem; scene-local copies are removed at runtime.</summary>
    public static class UiEventSystemUtility
    {
        private const string PersistedObjectName = "[Persisted] EventSystem";
        private const string InputActionsResourceName = "InputSystem_Actions";

        private static EventSystem s_Persisted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            s_Persisted = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapAfterFirstScene()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            EnsureConfigured();
        }

        private static void OnActiveSceneChanged(Scene previous, Scene next)
        {
            ClearCurrentSelection();
        }

        /// <summary>Clears UI selection so Selectables can disable safely during scene changes.</summary>
        public static void ClearCurrentSelection()
        {
            if (s_Persisted != null)
                s_Persisted.SetSelectedGameObject(null);

            var es = EventSystem.current;
            if (es != null && es != s_Persisted)
                es.SetSelectedGameObject(null);
        }

        public static void ClearSelectionIfUnder(Transform root)
        {
            if (root == null)
                return;

            var es = EventSystem.current ?? s_Persisted;
            if (es == null || es.currentSelectedGameObject == null)
                return;

            if (es.currentSelectedGameObject.transform.IsChildOf(root))
                es.SetSelectedGameObject(null);
        }

        public static void EnsureConfigured()
        {
            var primary = EnsurePersistedEventSystem();

            var systems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < systems.Length; i++)
            {
                var sys = systems[i];
                if (sys == null || sys == primary)
                    continue;

                sys.SetSelectedGameObject(null);
                Object.Destroy(sys.gameObject);
            }

            RemoveLegacyInputModules(primary.gameObject);
            ConfigureNewInputModule(primary.gameObject);
        }

        private static EventSystem EnsurePersistedEventSystem()
        {
            if (s_Persisted != null)
                return s_Persisted;

            var existing = GameObject.Find(PersistedObjectName);
            if (existing != null)
            {
                s_Persisted = existing.GetComponent<EventSystem>();
                if (s_Persisted != null)
                    return s_Persisted;
            }

            var go = new GameObject(PersistedObjectName);
            s_Persisted = go.AddComponent<EventSystem>();
            Object.DontDestroyOnLoad(go);
            return s_Persisted;
        }

        private static void RemoveLegacyInputModules(GameObject eventSystemGo)
        {
            var standalones = eventSystemGo.GetComponents<StandaloneInputModule>();
            for (int i = 0; i < standalones.Length; i++)
                Object.Destroy(standalones[i]);
        }

        private static void ConfigureNewInputModule(GameObject eventSystemGo)
        {
#if ENABLE_INPUT_SYSTEM
            var module = eventSystemGo.GetComponent<InputSystemUIInputModule>();
            if (module == null)
                module = eventSystemGo.AddComponent<InputSystemUIInputModule>();

            if (module.actionsAsset != null)
                return;

            var asset = Resources.Load<InputActionAsset>(InputActionsResourceName);
            if (asset == null)
                return;

            module.actionsAsset = asset;
            var ui = asset.FindActionMap("UI", throwIfNotFound: true);
            module.point = InputActionReference.Create(ui.FindAction("Point", throwIfNotFound: true));
            module.leftClick = InputActionReference.Create(ui.FindAction("Click", throwIfNotFound: true));
            module.rightClick = InputActionReference.Create(ui.FindAction("RightClick", throwIfNotFound: true));
            module.middleClick = InputActionReference.Create(ui.FindAction("MiddleClick", throwIfNotFound: true));
            module.scrollWheel = InputActionReference.Create(ui.FindAction("ScrollWheel", throwIfNotFound: true));
            module.move = InputActionReference.Create(ui.FindAction("Navigate", throwIfNotFound: true));
            module.submit = InputActionReference.Create(ui.FindAction("Submit", throwIfNotFound: true));
            module.cancel = InputActionReference.Create(ui.FindAction("Cancel", throwIfNotFound: true));
#endif
        }
    }
}
