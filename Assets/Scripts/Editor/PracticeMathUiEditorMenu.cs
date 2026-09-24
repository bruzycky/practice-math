#if UNITY_EDITOR
using PracticeMath.Navigation;
using PracticeMath.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace PracticeMath.Editor
{
    public static class PracticeMathUiEditorMenu
    {
        private const string HomePrefabPath = PracticeMathUiLayoutBuilder.PrefabFolder + "/HomeHubPanel.prefab";
        private const string TimesPrefabPath = PracticeMathUiLayoutBuilder.PrefabFolder + "/TimesTablesPanel.prefab";
        private const string McPrefabPath = PracticeMathUiLayoutBuilder.PrefabFolder + "/MultipleChoicePanel.prefab";

        [MenuItem("Practice Math/UI/1. Generate UI Prefabs (edit these in Project)")]
        public static void GeneratePrefabs()
        {
            EnsureFolder(PracticeMathUiLayoutBuilder.PrefabFolder);

            SavePrefab(PracticeMathUiLayoutBuilder.BuildHomePanel(), HomePrefabPath);
            SavePrefab(PracticeMathUiLayoutBuilder.BuildTimesTablesPanel(), TimesPrefabPath);
            SavePrefab(PracticeMathUiLayoutBuilder.BuildMultipleChoicePanel(), McPrefabPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "Practice Math UI",
                "Prefabs saved under Assets/Prefabs/UI/.\n\nOpen a prefab to change layout, fonts, colors, and spacing.",
                "OK");
        }

        [MenuItem("Practice Math/UI/2. Apply Prefabs To Learning Scenes")]
        public static void ApplyPrefabsToScenes()
        {
            if (!PrefabExists(HomePrefabPath))
            {
                EditorUtility.DisplayDialog("Practice Math UI", "Run \"Generate UI Prefabs\" first.", "OK");
                return;
            }

            ApplyToScene(GameScenes.Home, HomePrefabPath, typeof(HomeHubController));
            ApplyToScene(GameScenes.TimesTables, TimesPrefabPath, typeof(TimesTablePracticeController));
            ApplyToScene(GameScenes.MultipleChoice, McPrefabPath, typeof(MultipleChoiceActivityController));
            EnsurePracticeMathHomeButton();

            EditorUtility.DisplayDialog(
                "Practice Math UI",
                "Home, TimesTables, and MultipleChoice scenes now use editable prefab instances.\n\n" +
                "PracticeMath keeps its existing canvas; a Home button was added if missing.",
                "OK");
        }

        [MenuItem("Practice Math/UI/3. Regenerate Prefabs And Apply To Scenes")]
        public static void RegenerateAndApply()
        {
            GeneratePrefabs();
            ApplyPrefabsToScenes();
        }

        private static void ApplyToScene(string sceneName, string prefabPath, System.Type controllerType)
        {
            var scene = EditorSceneManager.OpenScene($"Assets/Scenes/{sceneName}.unity", OpenSceneMode.Single);
            RemoveLegacyRuntimeRoots(scene, controllerType);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
                return;

            EnsureEventSystemInScene();
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance != null)
                SceneManager.MoveGameObjectToScene(instance, scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void RemoveLegacyRuntimeRoots(Scene scene, System.Type controllerType)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name is "HomeHub" or "TimesTables" or "MultipleChoice" or "PracticeSceneOverlay")
                    Object.DestroyImmediate(root);
            }

            foreach (var canvasName in new[] { "HomeCanvas", "TimesTablesCanvas", "MultipleChoiceCanvas" })
            {
                var go = GameObject.Find(canvasName);
                if (go != null)
                    Object.DestroyImmediate(go);
            }

            foreach (var ctrl in Object.FindObjectsByType(controllerType, FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (ctrl is Component component)
                    Object.DestroyImmediate(component.gameObject);
            }
        }

        private static void EnsureEventSystemInScene()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
                return;

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        private static void EnsurePracticeMathHomeButton()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/PracticeMath.unity", OpenSceneMode.Single);
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (Object.FindFirstObjectByType<PracticeSceneOverlay>() == null)
            {
                var overlay = new GameObject("PracticeSceneOverlay");
                overlay.AddComponent<PracticeSceneOverlay>();
            }
            if (canvas != null)
            {
                var rt = canvas.GetComponent<RectTransform>();
                if (rt.Find("HomeNavButton") == null)
                {
                    var btn = UiRuntimeFactory.CreateButton(rt, "Home", new Vector2(180f, 64f), null);
                    btn.gameObject.name = "HomeNavButton";
                    var btnRt = btn.GetComponent<RectTransform>();
                    btnRt.anchorMin = new Vector2(0f, 1f);
                    btnRt.anchorMax = new Vector2(0f, 1f);
                    btnRt.pivot = new Vector2(0f, 1f);
                    btnRt.anchoredPosition = new Vector2(16f, -16f);
                    UnityEventTools.AddVoidPersistentListener(btn.onClick, GameSceneLoader.LoadHome);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void SavePrefab(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void EnsureFolder(string folder)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }

        private static bool PrefabExists(string path) => AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
    }
}
#endif
