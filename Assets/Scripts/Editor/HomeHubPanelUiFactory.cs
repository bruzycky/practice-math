#if UNITY_EDITOR
using PracticeMath.Analytics;
using PracticeMath.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace PracticeMath.Editor
{
    /// <summary>Builds and links the home settings overlay prefab on HomeHubPanel.</summary>
    public static class HomeHubPanelUiFactory
    {
        public const string SettingsOverlayPrefabPath =
            PracticeMathUiLayoutBuilder.PrefabFolder + "/HomeHubSettingsOverlay.prefab";

        /// <summary>Default layout for <see cref="SettingsOverlayPrefabPath"/> (editable prefab asset).</summary>
        public static GameObject BuildSettingsOverlay()
        {
            var root = new GameObject(
                "HomeHubSettingsOverlay",
                typeof(RectTransform),
                typeof(HomeHubSettingsOverlayRoot),
                typeof(HomeHubSettingsView));
            StretchFull(root.GetComponent<RectTransform>());

            var overlayRt = root.GetComponent<RectTransform>();

            var gearBtn = CreateIconButton(overlayRt, "SettingsGearButton", "\u2699", new Vector2(72f, 72f), new Color(1f, 0.58f, 0.65f));
            var gearRt = gearBtn.GetComponent<RectTransform>();
            gearRt.anchorMin = new Vector2(1f, 1f);
            gearRt.anchorMax = new Vector2(1f, 1f);
            gearRt.pivot = new Vector2(1f, 1f);
            gearRt.anchoredPosition = new Vector2(-24f, -24f);

            var settingsPanel = CreateOverlayPanel(overlayRt, "SettingsPanel", "Settings", out var settingsBg);
            var adminPanel = CreateOverlayPanel(overlayRt, "AdminPanel", "Practice tracking", out var adminBg);

            UiRuntimeFactory.CreateText(settingsPanel.GetComponent<RectTransform>(), "ColorSchemeLabel", "Color scheme", 32f, TextAlignmentOptions.Top);
            var labelRt = settingsPanel.transform.Find("ColorSchemeLabel")?.GetComponent<RectTransform>();
            if (labelRt != null)
            {
                labelRt.anchorMin = new Vector2(0.5f, 1f);
                labelRt.anchorMax = new Vector2(0.5f, 1f);
                labelRt.pivot = new Vector2(0.5f, 1f);
                labelRt.anchoredPosition = new Vector2(0f, -200f);
                labelRt.sizeDelta = new Vector2(600f, 48f);
            }

            var schemeDropdown = UiRuntimeFactory.CreateLabeledDropdown(settingsPanel.GetComponent<RectTransform>(), "ColorSchemeDropdown", 0);
            var schemeRt = schemeDropdown.GetComponent<RectTransform>();
            schemeRt.anchorMin = new Vector2(0.5f, 1f);
            schemeRt.anchorMax = new Vector2(0.5f, 1f);
            schemeRt.pivot = new Vector2(0.5f, 1f);
            schemeRt.anchoredPosition = new Vector2(0f, -280f);

            var adminNavBtn = UiRuntimeFactory.CreateButton(settingsPanel.GetComponent<RectTransform>(), "Practice tracking (admin)", new Vector2(520f, 72f), null);
            var adminNavRt = adminNavBtn.GetComponent<RectTransform>();
            adminNavRt.anchorMin = new Vector2(0.5f, 0.5f);
            adminNavRt.anchorMax = new Vector2(0.5f, 0.5f);
            adminNavRt.anchoredPosition = new Vector2(0f, -80f);

            var settingsBackBtn = UiRuntimeFactory.CreateButton(settingsPanel.GetComponent<RectTransform>(), "Back", new Vector2(220f, 64f), null);
            PositionBottomCenter(settingsBackBtn.GetComponent<RectTransform>(), -120f);

            var adminBackBtn = UiRuntimeFactory.CreateButton(adminPanel.GetComponent<RectTransform>(), "Back to settings", new Vector2(320f, 64f), null);
            PositionBottomCenter(adminBackBtn.GetComponent<RectTransform>(), -120f);

            var analyticsGo = new GameObject("HomeAnalytics", typeof(PracticeSessionAnalytics), typeof(AnalyticsPanelView));
            analyticsGo.transform.SetParent(root.transform, false);

            var adminContent = BuildAdminScroll(adminPanel.transform);
            var analyticsView = analyticsGo.GetComponent<AnalyticsPanelView>();
            var analyticsSo = new SerializedObject(analyticsView);
            analyticsSo.FindProperty("analytics").objectReferenceValue = analyticsGo.GetComponent<PracticeSessionAnalytics>();
            analyticsSo.FindProperty("summaryText").objectReferenceValue = adminContent.summaryText;
            analyticsSo.FindProperty("scrollContentRoot").objectReferenceValue = adminContent.contentRoot;
            analyticsSo.FindProperty("panelRoot").objectReferenceValue = null;
            analyticsSo.ApplyModifiedPropertiesWithoutUndo();

            var resetSessionBtn = UiRuntimeFactory.CreateButton(adminPanel.GetComponent<RectTransform>(), "Reset session stats", new Vector2(360f, 56f), null);
            var resetSessionRt = resetSessionBtn.GetComponent<RectTransform>();
            resetSessionRt.anchorMin = new Vector2(0.5f, 0f);
            resetSessionRt.anchorMax = new Vector2(0.5f, 0f);
            resetSessionRt.pivot = new Vector2(0.5f, 0f);
            resetSessionRt.anchoredPosition = new Vector2(-100f, 180f);
            UnityEventTools.AddPersistentListener(resetSessionBtn.onClick, analyticsView.ResetStatsFromUi);

            var resetAllBtn = UiRuntimeFactory.CreateButton(adminPanel.GetComponent<RectTransform>(), "Reset all saved data", new Vector2(360f, 56f), null);
            var resetAllRt = resetAllBtn.GetComponent<RectTransform>();
            resetAllRt.anchorMin = new Vector2(0.5f, 0f);
            resetAllRt.anchorMax = new Vector2(0.5f, 0f);
            resetAllRt.pivot = new Vector2(0.5f, 0f);
            resetAllRt.anchoredPosition = new Vector2(-100f, 100f);
            UnityEventTools.AddPersistentListener(resetAllBtn.onClick, analyticsView.ResetAllPersistentFromUi);

            var settingsView = root.GetComponent<HomeHubSettingsView>();
            var settingsSo = new SerializedObject(settingsView);
            settingsSo.FindProperty("settingsPanelRoot").objectReferenceValue = settingsPanel;
            settingsSo.FindProperty("adminPanelRoot").objectReferenceValue = adminPanel;
            settingsSo.FindProperty("colorSchemeDropdown").objectReferenceValue = schemeDropdown;
            settingsSo.FindProperty("analyticsPanel").objectReferenceValue = analyticsView;
            settingsSo.ApplyModifiedPropertiesWithoutUndo();

            UnityEventTools.AddPersistentListener(adminNavBtn.onClick, settingsView.OpenAdminTracking);
            UnityEventTools.AddPersistentListener(settingsBackBtn.onClick, settingsView.CloseSettingsToHub);
            UnityEventTools.AddPersistentListener(adminBackBtn.onClick, settingsView.CloseAdminToSettings);

            var overlayRoot = root.GetComponent<HomeHubSettingsOverlayRoot>();
            var overlaySo = new SerializedObject(overlayRoot);
            overlaySo.FindProperty("settingsView").objectReferenceValue = settingsView;
            overlaySo.ApplyModifiedPropertiesWithoutUndo();

            gearBtn.transform.SetAsLastSibling();
            settingsPanel.transform.SetAsLastSibling();
            adminPanel.transform.SetAsLastSibling();

            return root;
        }

        /// <summary>Creates the settings overlay prefab if missing, then nests it under the home hub prefab.</summary>
        public static bool TryAddSettingsUi(GameObject panelRoot)
        {
            if (panelRoot == null)
                return false;

            var controller = panelRoot.GetComponent<HomeHubController>();
            if (controller == null)
                return false;

            EnsureCanvasBackgroundAndTheme(panelRoot);

            var existingOverlay = panelRoot.GetComponentInChildren<HomeHubSettingsOverlayRoot>(true);
            if (existingOverlay != null)
            {
                WireSettingsOverlayToHomeHub(panelRoot, existingOverlay.gameObject);
                return false;
            }

            if (HasLegacyEmbeddedSettings(panelRoot.transform))
                return false;

            if (!System.IO.File.Exists(SettingsOverlayPrefabPath))
                return false;

            var overlayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SettingsOverlayPrefabPath);
            if (overlayPrefab == null)
                return false;

            var instance = PrefabUtility.InstantiatePrefab(overlayPrefab, panelRoot.transform) as GameObject;
            if (instance == null)
                return false;

            WireSettingsOverlayToHomeHub(panelRoot, instance);
            return true;
        }

        public static void WireSettingsOverlayToHomeHub(GameObject homePanel, GameObject overlayInstance)
        {
            if (homePanel == null || overlayInstance == null)
                return;

            var controller = homePanel.GetComponent<HomeHubController>();
            var overlayRoot = overlayInstance.GetComponent<HomeHubSettingsOverlayRoot>();
            var settingsView = overlayRoot != null ? overlayRoot.SettingsView : overlayInstance.GetComponent<HomeHubSettingsView>();
            if (controller == null || settingsView == null)
                return;

            var gearBtn = overlayInstance.transform.Find("SettingsGearButton")?.GetComponent<Button>();
            var settingsPanel = overlayInstance.transform.Find("SettingsPanel")?.gameObject;
            var adminPanel = overlayInstance.transform.Find("AdminPanel")?.gameObject;
            var settingsBg = settingsPanel?.GetComponent<Image>();
            var adminBg = adminPanel?.GetComponent<Image>();

            var settingsSo = new SerializedObject(settingsView);
            settingsSo.FindProperty("hub").objectReferenceValue = controller;
            settingsSo.ApplyModifiedPropertiesWithoutUndo();

            if (gearBtn != null)
            {
                gearBtn.onClick.RemoveAllListeners();
                UnityEventTools.AddPersistentListener(gearBtn.onClick, controller.OpenSettings);
            }

            var header = homePanel.transform.Find("Header");
            var scroll = homePanel.transform.Find("Scroll");
            var themeApplicator = homePanel.GetComponent<UiHomeThemeApplicator>();
            if (themeApplicator == null)
                themeApplicator = homePanel.AddComponent<UiHomeThemeApplicator>();

            var title = header?.Find("Title")?.GetComponent<TextMeshProUGUI>();
            var subtitle = header?.Find("Subtitle")?.GetComponent<TextMeshProUGUI>();
            var gradeDropdown = header != null ? header.GetComponentInChildren<TMP_Dropdown>(true) : null;
            var scrollImg = scroll != null ? scroll.GetComponent<Image>() : null;
            var tiles = homePanel.GetComponentsInChildren<HomeNavTileView>(true);
            var canvasBg = homePanel.transform.Find("CanvasBackground")?.GetComponent<Image>();

            var themeSo = new SerializedObject(themeApplicator);
            themeSo.FindProperty("canvasBackground").objectReferenceValue = canvasBg;
            themeSo.FindProperty("scrollBackdrop").objectReferenceValue = scrollImg;
            themeSo.FindProperty("titleText").objectReferenceValue = title;
            themeSo.FindProperty("subtitleText").objectReferenceValue = subtitle;
            themeSo.FindProperty("gradeDropdown").objectReferenceValue = gradeDropdown;
            themeSo.FindProperty("navTiles").arraySize = tiles.Length;
            for (int i = 0; i < tiles.Length; i++)
                themeSo.FindProperty("navTiles").GetArrayElementAtIndex(i).objectReferenceValue = tiles[i];
            themeSo.FindProperty("settingsGearBackground").objectReferenceValue = gearBtn != null ? gearBtn.GetComponent<Image>() : null;
            themeSo.FindProperty("settingsPanelBackground").objectReferenceValue = settingsBg;
            themeSo.FindProperty("adminPanelBackground").objectReferenceValue = adminBg;
            themeSo.ApplyModifiedPropertiesWithoutUndo();

            var hubSo = new SerializedObject(controller);
            hubSo.FindProperty("headerRoot").objectReferenceValue = header != null ? header.gameObject : null;
            hubSo.FindProperty("scrollRoot").objectReferenceValue = scroll != null ? scroll.gameObject : null;
            hubSo.FindProperty("settingsGearButton").objectReferenceValue = gearBtn != null ? gearBtn.gameObject : null;
            hubSo.FindProperty("settingsView").objectReferenceValue = settingsView;
            hubSo.ApplyModifiedPropertiesWithoutUndo();

            overlayInstance.transform.SetAsLastSibling();
        }

        /// <summary>Removes old inline settings objects and nests the overlay prefab instance.</summary>
        public static bool ReplaceLegacyEmbeddedSettingsWithOverlayPrefab(GameObject homePanel)
        {
            if (homePanel == null || !HasLegacyEmbeddedSettings(homePanel.transform))
                return false;

            RemoveLegacyEmbeddedSettings(homePanel.transform);

            var overlayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SettingsOverlayPrefabPath);
            if (overlayPrefab == null)
                return false;

            var instance = PrefabUtility.InstantiatePrefab(overlayPrefab, homePanel.transform) as GameObject;
            if (instance == null)
                return false;

            WireSettingsOverlayToHomeHub(homePanel, instance);
            return true;
        }

        private static void EnsureCanvasBackgroundAndTheme(GameObject panelRoot)
        {
            var canvasRt = panelRoot.GetComponent<RectTransform>();
            if (panelRoot.transform.Find("CanvasBackground") == null)
            {
                var bgGo = new GameObject("CanvasBackground", typeof(RectTransform), typeof(Image));
                bgGo.transform.SetParent(canvasRt, false);
                bgGo.transform.SetAsFirstSibling();
                StretchFull(bgGo.GetComponent<RectTransform>());
                bgGo.GetComponent<Image>().raycastTarget = false;
            }

            if (panelRoot.GetComponent<UiHomeThemeApplicator>() == null)
                panelRoot.AddComponent<UiHomeThemeApplicator>();
        }

        private static bool HasLegacyEmbeddedSettings(Transform homeRoot)
        {
            if (homeRoot.GetComponentInChildren<HomeHubSettingsOverlayRoot>(true) != null)
                return false;
            return homeRoot.Find("SettingsPanel") != null || homeRoot.Find("SettingsGearButton") != null;
        }

        private static void RemoveLegacyEmbeddedSettings(Transform homeRoot)
        {
            DestroyChildIfExists(homeRoot, "SettingsPanel");
            DestroyChildIfExists(homeRoot, "AdminPanel");
            DestroyChildIfExists(homeRoot, "SettingsGearButton");
            DestroyChildIfExists(homeRoot, "HomeHubSettings");
            DestroyChildIfExists(homeRoot, "HomeAnalytics");
        }

        private static void DestroyChildIfExists(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
                Object.DestroyImmediate(child.gameObject);
        }

        private static GameObject CreateOverlayPanel(RectTransform parent, string name, string title, out Image background)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            StretchFull(panel.GetComponent<RectTransform>());
            background = panel.GetComponent<Image>();
            background.color = new Color(0.98f, 0.96f, 0.86f, 0.98f);

            var titleTmp = UiRuntimeFactory.CreateText(panel.GetComponent<RectTransform>(), "PanelTitle", title, 48f, TextAlignmentOptions.Top);
            var titleRt = titleTmp.rectTransform;
            titleRt.anchorMin = new Vector2(0.5f, 1f);
            titleRt.anchorMax = new Vector2(0.5f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -48f);
            titleRt.sizeDelta = new Vector2(800f, 64f);
            return panel;
        }

        private static (TMP_Text summaryText, RectTransform contentRoot) BuildAdminScroll(Transform adminPanel)
        {
            var scrollGo = new GameObject("AdminScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollGo.transform.SetParent(adminPanel, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0.08f, 0.22f);
            scrollRt.anchorMax = new Vector2(0.92f, 0.78f);
            scrollRt.offsetMin = Vector2.zero;
            scrollRt.offsetMax = Vector2.zero;
            scrollGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.12f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollGo.transform, false);
            StretchFull(viewport.GetComponent<RectTransform>());
            viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform), typeof(ContentSizeFitter), typeof(VerticalLayoutGroup));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            var summary = UiRuntimeFactory.CreateText(contentRt, "Summary", "Loading stats…", 22f, TextAlignmentOptions.TopLeft);
            summary.textWrappingMode = TextWrappingModes.Normal;
            var summaryRt = summary.rectTransform;
            summaryRt.sizeDelta = new Vector2(0f, 200f);
            var summaryLayout = summary.gameObject.AddComponent<LayoutElement>();
            summaryLayout.minHeight = 120f;
            summaryLayout.preferredWidth = 900f;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.content = contentRt;
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.horizontal = false;
            scroll.vertical = true;

            return (summary, contentRt);
        }

        private static Button CreateIconButton(RectTransform parent, string name, string icon, Vector2 size, Color bg)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.color = bg;
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;

            var label = UiRuntimeFactory.CreateText(rt, "Icon", icon, 40f, TextAlignmentOptions.Center);
            StretchFull(label.rectTransform);
            return btn;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void PositionBottomCenter(RectTransform rt, float y)
        {
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, y);
        }
    }
}
#endif
