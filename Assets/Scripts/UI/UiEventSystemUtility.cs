using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace PracticeMath.UI
{
    /// <summary>Ensures a single EventSystem compatible with Project Settings → Input System package.</summary>
    public static class UiEventSystemUtility
    {
        private const string InputActionsResourceName = "InputSystem_Actions";

        public static void EnsureConfigured()
        {
            var systems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            EventSystem primary = null;

            for (int i = 0; i < systems.Length; i++)
            {
                if (primary == null)
                    primary = systems[i];
                else
                    Object.Destroy(systems[i].gameObject);
            }

            if (primary == null)
            {
                var go = new GameObject("EventSystem");
                primary = go.AddComponent<EventSystem>();
            }

            RemoveLegacyInputModules(primary.gameObject);
            ConfigureNewInputModule(primary.gameObject);
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
