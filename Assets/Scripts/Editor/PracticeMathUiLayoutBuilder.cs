#if UNITY_EDITOR
using PracticeMath.Core;
using PracticeMath.Navigation;
using PracticeMath.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PracticeMath.Editor
{
    /// <summary>Creates default hub/activity canvases for saving as prefabs or scene objects.</summary>
    public static class PracticeMathUiLayoutBuilder
    {
        public const string PrefabFolder = "Assets/Prefabs/UI";

        public static GameObject BuildHomePanel()
        {
            UiRuntimeFactory.CreateScreenCanvas("HomeCanvas", out RectTransform root);
            var controller = root.gameObject.AddComponent<HomeHubController>();

            const float headerHeight = 300f;

            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollGo.transform.SetParent(root, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0.05f, 0.04f);
            scrollRt.anchorMax = new Vector2(0.95f, 1f);
            scrollRt.offsetMin = Vector2.zero;
            scrollRt.offsetMax = new Vector2(0f, -headerHeight);
            scrollGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.15f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollGo.transform, false);
            var viewportRt = viewport.GetComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;
            viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);

            var layout = content.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 16f;
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.content = contentRt;
            scroll.viewport = viewportRt;
            scroll.horizontal = false;
            scroll.vertical = true;

            var tiles = new HomeNavTileView[7];
            tiles[0] = AddTile(contentRt, "Practice (+ − × ÷)", "Endless problems at your grade level", LearningModule.Practice);
            tiles[1] = AddTile(contentRt, "10-Question Quiz", "Mixed quiz with score at the end", LearningModule.Quiz);
            tiles[2] = AddTile(contentRt, "Times Tables (0–12)", "Full multiplication table practice", LearningModule.TimesTables);
            tiles[3] = AddTile(contentRt, "Shapes & Space", "Geometry multiple choice", LearningModule.Geometry);
            tiles[4] = AddTile(contentRt, "Patterns", "What comes next?", LearningModule.Patterns);
            tiles[5] = AddTile(contentRt, "Money (Canada)", "Coins, bills, and change", LearningModule.Money);
            tiles[6] = AddTile(contentRt, "Charts & Data", "Graphs, tallies, and likelihood", LearningModule.Data);

            var headerRoot = new GameObject("Header", typeof(RectTransform));
            headerRoot.transform.SetParent(root, false);
            var headerRootRt = headerRoot.GetComponent<RectTransform>();
            headerRootRt.anchorMin = new Vector2(0f, 1f);
            headerRootRt.anchorMax = new Vector2(1f, 1f);
            headerRootRt.pivot = new Vector2(0.5f, 1f);
            headerRootRt.sizeDelta = new Vector2(0f, headerHeight);

            UiRuntimeFactory.CreateText(headerRootRt, "Title", "Practice Math", 56f, TextAlignmentOptions.Top);
            UiRuntimeFactory.CreateText(headerRootRt, "Subtitle", "Choose what to practice", 28f, TextAlignmentOptions.Top);

            var dropdown = UiRuntimeFactory.CreateGradeDropdown(headerRootRt, 2);
            var dropRt = dropdown.GetComponent<RectTransform>();
            dropRt.anchorMin = new Vector2(0.5f, 1f);
            dropRt.anchorMax = new Vector2(0.5f, 1f);
            dropRt.pivot = new Vector2(0.5f, 1f);
            dropRt.anchoredPosition = new Vector2(0f, -150f);

            headerRoot.transform.SetAsLastSibling();

            BindHomeController(controller, dropdown, tiles);
            return root.gameObject;
        }

        public static GameObject BuildTimesTablesPanel()
        {
            UiRuntimeFactory.CreateScreenCanvas("TimesTablesCanvas", out RectTransform root);
            var controller = root.gameObject.AddComponent<TimesTablePracticeController>();
            UiHomeNavButton.AddTo(root);

            UiRuntimeFactory.CreateText(root, "Title", "Times Tables (0–12)", 48f, TextAlignmentOptions.Top);
            var dropdown = UiRuntimeFactory.CreateGradeDropdown(root, 0);
            dropdown.name = "TableDropdown";
            var tableDropdown = dropdown;
            tableDropdown.options.Clear();

            var prompt = UiRuntimeFactory.CreateText(root, "Prompt", "?", 64f, TextAlignmentOptions.Center);
            var answer = UiRuntimeFactory.CreateText(root, "Answer", string.Empty, 52f, TextAlignmentOptions.Center);
            var feedback = UiRuntimeFactory.CreateText(root, "Feedback", string.Empty, 32f, TextAlignmentOptions.Center);

            StretchTopBand(prompt.rectTransform, 320f, 980f, 120f);
            StretchTopBand(answer.rectTransform, 460f, 600f, 80f);
            StretchTopBand(feedback.rectTransform, 560f, 900f, 60f);
            StretchTopBand(tableDropdown.GetComponent<RectTransform>(), 160f, 420f, 70f);

            var panel = new GameObject("Keypad", typeof(RectTransform), typeof(GridLayoutGroup));
            panel.transform.SetParent(root, false);
            var panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0f);
            panelRt.anchorMax = new Vector2(0.5f, 0f);
            panelRt.pivot = new Vector2(0.5f, 0f);
            panelRt.sizeDelta = new Vector2(520f, 420f);
            panelRt.anchoredPosition = new Vector2(0f, 120f);
            var grid = panel.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(150f, 90f);
            grid.spacing = new Vector2(12f, 12f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            for (int d = 1; d <= 9; d++)
                WireKeypadButton(panelRt, d.ToString(), controller, d);
            WireKeypadButton(panelRt, "Clear", controller, -1);
            WireKeypadButton(panelRt, "0", controller, 0);
            WireKeypadButton(panelRt, "⌫", controller, -2);
            WireKeypadButton(panelRt, "Check", controller, -3);

            BindTimesTableController(controller, tableDropdown, prompt, answer, feedback);
            return root.gameObject;
        }

        public static GameObject BuildMultipleChoicePanel()
        {
            UiRuntimeFactory.CreateScreenCanvas("MultipleChoiceCanvas", out RectTransform root);
            var controller = root.gameObject.AddComponent<MultipleChoiceActivityController>();
            UiHomeNavButton.AddTo(root);

            var title = UiRuntimeFactory.CreateText(root, "Title", "Activity", 44f, TextAlignmentOptions.Top);
            var status = UiRuntimeFactory.CreateText(root, "Status", string.Empty, 28f, TextAlignmentOptions.Top);
            var prompt = UiRuntimeFactory.CreateText(root, "Prompt", string.Empty, 36f, TextAlignmentOptions.Top);
            StretchTopBand(title.rectTransform, 24f, 1000f, 70f);
            StretchTopBand(status.rectTransform, 100f, 1000f, 50f);
            StretchTopBand(prompt.rectTransform, 170f, 980f, 160f);

            var optionsRoot = new GameObject("Options", typeof(RectTransform), typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
            optionsRoot.SetParent(root, false);
            optionsRoot.anchorMin = new Vector2(0.5f, 0.5f);
            optionsRoot.anchorMax = new Vector2(0.5f, 0.5f);
            optionsRoot.pivot = new Vector2(0.5f, 0.5f);
            optionsRoot.sizeDelta = new Vector2(900f, 420f);
            optionsRoot.anchoredPosition = new Vector2(0f, -40f);
            var optLayout = optionsRoot.GetComponent<VerticalLayoutGroup>();
            optLayout.spacing = 14f;
            optLayout.childControlHeight = true;
            optLayout.childControlWidth = true;
            optLayout.childForceExpandHeight = false;
            optLayout.childForceExpandWidth = true;

            var optionButtons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                var btn = UiRuntimeFactory.CreateButton(optionsRoot, "Option", new Vector2(900f, 80f), null);
                var le = btn.gameObject.GetComponent<LayoutElement>() ?? btn.gameObject.AddComponent<LayoutElement>();
                le.minHeight = 80f;
                optionButtons[i] = btn;
            }

            var feedback = UiRuntimeFactory.CreateText(root, "Feedback", string.Empty, 30f, TextAlignmentOptions.Center);
            StretchBottomBand(feedback.rectTransform, 200f, 980f, 60f);
            var nextBtn = UiRuntimeFactory.CreateButton(root, "Next question", new Vector2(320f, 72f), null);
            StretchBottomBand(nextBtn.GetComponent<RectTransform>(), 100f, 320f, 72f);

            BindMultipleChoiceController(controller, title, prompt, status, feedback, optionButtons, nextBtn);
            return root.gameObject;
        }

        private static HomeNavTileView AddTile(RectTransform parent, string title, string description, LearningModule module)
        {
            var tile = new GameObject("Tile", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(HomeNavTileView));
            tile.transform.SetParent(parent, false);
            tile.GetComponent<Image>().color = new Color(0.18f, 0.35f, 0.58f, 1f);
            tile.GetComponent<LayoutElement>().minHeight = 100f;

            UiRuntimeFactory.CreateText(tile.GetComponent<RectTransform>(), "TileTitle", title, 34f, TextAlignmentOptions.TopLeft);
            var desc = UiRuntimeFactory.CreateText(tile.GetComponent<RectTransform>(), "TileDesc", description, 24f, TextAlignmentOptions.TopLeft);
            desc.color = new Color(0.9f, 0.95f, 1f, 0.9f);

            var view = tile.GetComponent<HomeNavTileView>();
            var so = new SerializedObject(view);
            so.FindProperty("module").enumValueIndex = (int)module;
            so.FindProperty("button").objectReferenceValue = tile.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        private static void BindHomeController(HomeHubController controller, TMP_Dropdown dropdown, HomeNavTileView[] tiles)
        {
            var so = new SerializedObject(controller);
            so.FindProperty("gradeDropdown").objectReferenceValue = dropdown;
            so.FindProperty("navTiles").arraySize = tiles.Length;
            for (int i = 0; i < tiles.Length; i++)
                so.FindProperty("navTiles").GetArrayElementAtIndex(i).objectReferenceValue = tiles[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BindTimesTableController(
            TimesTablePracticeController controller,
            TMP_Dropdown dropdown,
            TextMeshProUGUI prompt,
            TextMeshProUGUI answer,
            TextMeshProUGUI feedback)
        {
            var so = new SerializedObject(controller);
            so.FindProperty("tableDropdown").objectReferenceValue = dropdown;
            so.FindProperty("promptText").objectReferenceValue = prompt;
            so.FindProperty("answerText").objectReferenceValue = answer;
            so.FindProperty("feedbackText").objectReferenceValue = feedback;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BindMultipleChoiceController(
            MultipleChoiceActivityController controller,
            TextMeshProUGUI title,
            TextMeshProUGUI prompt,
            TextMeshProUGUI status,
            TextMeshProUGUI feedback,
            Button[] options,
            Button next)
        {
            var so = new SerializedObject(controller);
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("promptText").objectReferenceValue = prompt;
            so.FindProperty("statusText").objectReferenceValue = status;
            so.FindProperty("feedbackText").objectReferenceValue = feedback;
            so.FindProperty("nextButton").objectReferenceValue = next;
            so.FindProperty("optionButtons").arraySize = options.Length;
            for (int i = 0; i < options.Length; i++)
                so.FindProperty("optionButtons").GetArrayElementAtIndex(i).objectReferenceValue = options[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireKeypadButton(RectTransform parent, string label, TimesTablePracticeController controller, int code)
        {
            var btn = UiRuntimeFactory.CreateButton(parent, label, new Vector2(150f, 90f), null);
            UnityAction action = code switch
            {
                -3 => controller.Submit,
                -2 => controller.Backspace,
                -1 => controller.ClearInput,
                0 => controller.Digit0,
                1 => controller.Digit1,
                2 => controller.Digit2,
                3 => controller.Digit3,
                4 => controller.Digit4,
                5 => controller.Digit5,
                6 => controller.Digit6,
                7 => controller.Digit7,
                8 => controller.Digit8,
                9 => controller.Digit9,
                _ => controller.ClearInput
            };
            UnityEventTools.AddPersistentListener(btn.onClick, action);
        }

        private static void StretchTopBand(RectTransform rt, float topOffset, float width, float height)
        {
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = new Vector2(0f, -topOffset);
        }

        private static void StretchBottomBand(RectTransform rt, float bottomOffset, float width, float height)
        {
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = new Vector2(0f, bottomOffset);
        }
    }
}
#endif
