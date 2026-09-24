using PracticeMath.Core;
using PracticeMath.Navigation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PracticeMath.UI
{
    public sealed class HomeHubController : MonoBehaviour
    {
        private TMP_Dropdown _gradeDropdown;

        private void Awake()
        {
            if (!UiRuntimeFactory.TryClaimBootstrapCanvas("HomeCanvas", out RectTransform root))
            {
                Destroy(gameObject);
                return;
            }

            var header = UiRuntimeFactory.CreateText(root, "Title", "Practice Math", 56f, TextAlignmentOptions.Top);
            var headerRt = header.rectTransform;
            headerRt.anchorMin = new Vector2(0f, 1f);
            headerRt.anchorMax = new Vector2(1f, 1f);
            headerRt.pivot = new Vector2(0.5f, 1f);
            headerRt.sizeDelta = new Vector2(0f, 90f);
            headerRt.anchoredPosition = new Vector2(0f, -24f);

            var subtitle = UiRuntimeFactory.CreateText(root, "Subtitle", "Choose what to practice", 32f, TextAlignmentOptions.Top);
            var subRt = subtitle.rectTransform;
            subRt.anchorMin = new Vector2(0f, 1f);
            subRt.anchorMax = new Vector2(1f, 1f);
            subRt.pivot = new Vector2(0.5f, 1f);
            subRt.sizeDelta = new Vector2(0f, 50f);
            subRt.anchoredPosition = new Vector2(0f, -120f);

            var ctx = AppSessionContext.Instance;
            int gradeIndex = ctx != null ? (int)ctx.SelectedGrade - 1 : 2;
            _gradeDropdown = UiRuntimeFactory.CreateGradeDropdown(root, gradeIndex);
            var dropRt = _gradeDropdown.GetComponent<RectTransform>();
            dropRt.anchorMin = new Vector2(0.5f, 1f);
            dropRt.anchorMax = new Vector2(0.5f, 1f);
            dropRt.pivot = new Vector2(0.5f, 1f);
            dropRt.anchoredPosition = new Vector2(0f, -200f);
            _gradeDropdown.onValueChanged.AddListener(OnGradeChanged);
            OnGradeChanged(_gradeDropdown.value);

            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollGo.transform.SetParent(root, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0.05f, 0.05f);
            scrollRt.anchorMax = new Vector2(0.95f, 0.78f);
            scrollRt.offsetMin = Vector2.zero;
            scrollRt.offsetMax = Vector2.zero;
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
            contentRt.offsetMin = new Vector2(0f, 0f);
            contentRt.offsetMax = new Vector2(0f, 0f);

            var layout = content.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 16f;
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            var fitter = content.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.content = contentRt;
            scroll.viewport = viewportRt;
            scroll.horizontal = false;
            scroll.vertical = true;

            AddTile(contentRt, "Practice (+ − × ÷)", "Endless problems at your grade level", () => GameSceneLoader.Launch(LearningModule.Practice));
            AddTile(contentRt, "10-Question Quiz", "Mixed quiz with score at the end", () => GameSceneLoader.Launch(LearningModule.Quiz));
            AddTile(contentRt, "Times Tables (0–12)", "Full multiplication table practice", () => GameSceneLoader.Launch(LearningModule.TimesTables));
            AddTile(contentRt, "Shapes & Space", "Geometry multiple choice", () => GameSceneLoader.Launch(LearningModule.Geometry));
            AddTile(contentRt, "Patterns", "What comes next?", () => GameSceneLoader.Launch(LearningModule.Patterns));
            AddTile(contentRt, "Money (Canada)", "Coins, bills, and change", () => GameSceneLoader.Launch(LearningModule.Money));
            AddTile(contentRt, "Charts & Data", "Graphs, tallies, and likelihood", () => GameSceneLoader.Launch(LearningModule.Data));
        }

        private void OnDestroy()
        {
            if (_gradeDropdown != null)
                _gradeDropdown.onValueChanged.RemoveListener(OnGradeChanged);
        }

        private void OnGradeChanged(int index)
        {
            var ctx = AppSessionContext.Instance;
            if (ctx == null)
                return;
            ctx.SetSelectedGrade((GradeLevel)(index + 1));
        }

        private static void AddTile(RectTransform parent, string title, string description, UnityEngine.Events.UnityAction onClick)
        {
            var tile = new GameObject("Tile", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            tile.transform.SetParent(parent, false);
            tile.GetComponent<Image>().color = new Color(0.18f, 0.35f, 0.58f, 1f);
            tile.GetComponent<LayoutElement>().minHeight = 120f;

            var btn = tile.GetComponent<Button>();
            btn.onClick.AddListener(onClick);

            var titleText = UiRuntimeFactory.CreateText(tile.GetComponent<RectTransform>(), "TileTitle", title, 34f, TextAlignmentOptions.TopLeft);
            var titleRt = titleText.rectTransform;
            titleRt.anchorMin = new Vector2(0f, 0.5f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.offsetMin = new Vector2(20f, 0f);
            titleRt.offsetMax = new Vector2(-20f, -12f);

            var descText = UiRuntimeFactory.CreateText(tile.GetComponent<RectTransform>(), "TileDesc", description, 24f, TextAlignmentOptions.TopLeft);
            descText.color = new Color(0.9f, 0.95f, 1f, 0.9f);
            var descRt = descText.rectTransform;
            descRt.anchorMin = new Vector2(0f, 0f);
            descRt.anchorMax = new Vector2(1f, 0.5f);
            descRt.offsetMin = new Vector2(20f, 12f);
            descRt.offsetMax = new Vector2(-20f, 0f);
        }
    }
}
