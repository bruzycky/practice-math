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
        private const string TimesGridPrefabPath = PracticeMathUiLayoutBuilder.PrefabFolder + "/TimesTableGridPanel.prefab";
        private const string McPrefabPath = PracticeMathUiLayoutBuilder.PrefabFolder + "/MultipleChoicePanel.prefab";

        [MenuItem("Practice Math/UI/1. Generate Missing UI Prefabs")]
        public static void GeneratePrefabs()
        {
            EnsureFolder(PracticeMathUiLayoutBuilder.PrefabFolder);

            SavePrefabIfMissing(PracticeMathUiLayoutBuilder.BuildHomePanel(), HomePrefabPath);
            SavePrefabIfMissing(PracticeMathUiLayoutBuilder.BuildTimesTablesPanel(), TimesPrefabPath);
            SavePrefabIfMissing(PracticeMathUiLayoutBuilder.BuildTimesTableGridPanel(), TimesGridPrefabPath);
            SavePrefabIfMissing(PracticeMathUiLayoutBuilder.BuildMultipleChoicePanel(), McPrefabPath);

            EnsureHomeNavOnPrefabs();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "Practice Math UI",
                "Created any prefabs that were missing under Assets/Prefabs/UI/.\n\n" +
                "Existing prefabs were left unchanged. Open prefabs to edit layout.\n\n" +
                "Use \"Force Regenerate All UI Prefabs\" only if you want to discard prefab edits.",
                "OK");
        }

        [MenuItem("Practice Math/UI/1b. Force Regenerate All UI Prefabs (overwrites edits)")]
        public static void ForceGeneratePrefabs()
        {
            if (!EditorUtility.DisplayDialog(
                    "Overwrite UI prefabs?",
                    "This replaces every panel prefab with the default layout from code and erases visual edits you made in the Project window.",
                    "Overwrite prefabs",
                    "Cancel"))
            {
                return;
            }

            EnsureFolder(PracticeMathUiLayoutBuilder.PrefabFolder);

            SavePrefab(PracticeMathUiLayoutBuilder.BuildHomePanel(), HomePrefabPath);
            SavePrefab(PracticeMathUiLayoutBuilder.BuildTimesTablesPanel(), TimesPrefabPath);
            SavePrefab(PracticeMathUiLayoutBuilder.BuildTimesTableGridPanel(), TimesGridPrefabPath);
            SavePrefab(PracticeMathUiLayoutBuilder.BuildMultipleChoicePanel(), McPrefabPath);

            EnsureHomeNavOnPrefabs();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Practice Math UI", "All UI prefabs were regenerated from code.", "OK");
        }

        [MenuItem("Practice Math/UI/2. Apply Prefabs To Learning Scenes")]
        public static void ApplyPrefabsToScenes()
        {
            if (!PrefabExists(HomePrefabPath))
            {
                EditorUtility.DisplayDialog("Practice Math UI", "Run \"Generate Missing UI Prefabs\" first.", "OK");
                return;
            }

            EnsureLearningSceneFile(GameScenes.TimesTableGrid);
            EnsureHomeNavOnPrefabs();

            ApplyToScene(GameScenes.Home, HomePrefabPath, typeof(HomeHubController));
            ApplyToScene(GameScenes.TimesTables, TimesPrefabPath, typeof(TimesTablePracticeController));
            if (PrefabExists(TimesGridPrefabPath))
                ApplyToScene(GameScenes.TimesTableGrid, TimesGridPrefabPath, typeof(TimesTableGridController));
            ApplyToScene(GameScenes.MultipleChoice, McPrefabPath, typeof(MultipleChoiceActivityController));
            EnsurePracticeMathHomeButton();
            EnsureSceneInBuildSettings(GameScenes.TimesTableGrid);

            EditorUtility.DisplayDialog(
                "Practice Math UI",
                "Scenes now reference your prefabs.\n\n" +
                "Existing prefab instances in a scene were kept (scene overrides preserved).\n" +
                "Missing panels were added from prefabs.",
                "OK");
        }

        [MenuItem("Practice Math/UI/3. Sync Scenes With Prefabs (keeps prefab edits)")]
        public static void SyncScenesWithPrefabs()
        {
            ApplyPrefabsToScenes();
        }

        private static void ApplyToScene(string sceneName, string prefabPath, System.Type controllerType)
        {
            var scene = EditorSceneManager.OpenScene($"Assets/Scenes/{sceneName}.unity", OpenSceneMode.Single);

            var existing = FindPrefabInstanceRoot(prefabPath, controllerType);
            if (existing != null)
            {
                EnsureHomeNavButtonView(existing.transform);
                EnsureEventSystemInScene();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                return;
            }

            RemoveLegacyRuntimeRoots(scene, controllerType);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
                return;

            EnsureEventSystemInScene();
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance != null)
            {
                SceneManager.MoveGameObjectToScene(instance, scene);
                EnsureHomeNavButtonView(instance.transform);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static GameObject FindPrefabInstanceRoot(string prefabPath, System.Type controllerType)
        {
            foreach (var ctrl in Object.FindObjectsByType(controllerType, FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (ctrl is not Component component)
                    continue;

                var root = PrefabUtility.GetNearestPrefabInstanceRoot(component.gameObject);
                if (root == null)
                    continue;

                var assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root);
                if (assetPath == prefabPath)
                    return root;
            }

            return null;
        }

        private static void RemoveLegacyRuntimeRoots(Scene scene, System.Type controllerType)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name is "HomeHub" or "TimesTables" or "MultipleChoice" or "PracticeSceneOverlay")
                    Object.DestroyImmediate(root);
            }

            foreach (var canvasName in new[] { "HomeCanvas", "TimesTablesCanvas", "TimesTableGridCanvas", "MultipleChoiceCanvas" })
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
                var home = rt.Find("HomeNavButton");
                if (home == null)
                {
                    var btn = UiRuntimeFactory.CreateButton(rt, "Home", new Vector2(180f, 64f), null);
                    btn.gameObject.name = "HomeNavButton";
                    var btnRt = btn.GetComponent<RectTransform>();
                    btnRt.anchorMin = new Vector2(0f, 1f);
                    btnRt.anchorMax = new Vector2(0f, 1f);
                    btnRt.pivot = new Vector2(0f, 1f);
                    btnRt.anchoredPosition = new Vector2(16f, -16f);
                    home = btn.transform;
                }

                if (home.GetComponent<HomeNavButtonView>() == null)
                    home.gameObject.AddComponent<HomeNavButtonView>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureHomeNavOnPrefabs()
        {
            EnsureHomeNavOnPrefab(HomePrefabPath);
            EnsureHomeNavOnPrefab(TimesPrefabPath);
            EnsureHomeNavOnPrefab(TimesGridPrefabPath);
            EnsureHomeNavOnPrefab(McPrefabPath);
        }

        private static void EnsureHomeNavOnPrefab(string prefabPath)
        {
            if (!PrefabExists(prefabPath))
                return;

            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                if (EnsureHomeNavButtonView(root.transform))
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static bool EnsureHomeNavButtonView(Transform root)
        {
            var home = FindChildRecursive(root, "HomeNavButton");
            if (home == null)
                return false;

            if (home.GetComponent<HomeNavButtonView>() != null)
                return false;

            home.gameObject.AddComponent<HomeNavButtonView>();
            return true;
        }

        private static Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name)
                return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                var found = FindChildRecursive(parent.GetChild(i), name);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static void SavePrefabIfMissing(GameObject root, string path)
        {
            if (PrefabExists(path))
            {
                Object.DestroyImmediate(root);
                return;
            }

            SavePrefab(root, path);
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

        private static void EnsureLearningSceneFile(string sceneName)
        {
            var path = $"Assets/Scenes/{sceneName}.unity";
            if (System.IO.File.Exists(path))
                return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void EnsureSceneInBuildSettings(string sceneName)
        {
            var path = $"Assets/Scenes/{sceneName}.unity";
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var entry in scenes)
            {
                if (entry.path == path)
                    return;
            }

            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
