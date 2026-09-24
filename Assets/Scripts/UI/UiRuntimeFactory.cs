using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PracticeMath.UI
{
    /// <summary>Builds a consistent Canvas + EventSystem for bootstrap scenes.</summary>
    public static class UiRuntimeFactory
    {
        public const float ReferenceWidth = 1200f;
        public const float ReferenceHeight = 1400f;

        public static void EnsureEventSystem()
        {
            UiEventSystemUtility.EnsureConfigured();
        }

        /// <summary>Returns false if a bootstrap canvas with this name already exists (prevents duplicate UI).</summary>
        public static bool TryClaimBootstrapCanvas(string canvasName, out RectTransform root)
        {
            var existing = GameObject.Find(canvasName);
            if (existing != null)
            {
                root = existing.GetComponent<RectTransform>();
                return false;
            }

            CreateScreenCanvas(canvasName, out root);
            return true;
        }

        public static Canvas CreateScreenCanvas(string name, out RectTransform root)
        {
            var canvasGo = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            root = canvasGo.GetComponent<RectTransform>();
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            UiPracticeBackgroundView.Ensure(canvasGo.transform);
            UiThemeSceneApplicator.EnsureOn(canvasGo);
            return canvas;
        }

        public static TextMeshProUGUI CreateText(RectTransform parent, string name, string text, float fontSize, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            if (TMP_Settings.defaultFontAsset != null)
                tmp.font = TMP_Settings.defaultFontAsset;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        public static Button CreateButton(RectTransform parent, string label, Vector2 size, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.color = new Color(0.2f, 0.45f, 0.75f, 1f);

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null)
                btn.onClick.AddListener(onClick);

            var text = CreateText(rt, "Label", label, 32f, TextAlignmentOptions.Center);
            var textRt = text.rectTransform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(8f, 8f);
            textRt.offsetMax = new Vector2(-8f, -8f);

            return btn;
        }

        public static TMP_Dropdown CreateGradeDropdown(RectTransform parent, int selectedIndex)
        {
            var template = CreateDropdownTemplate(parent);
            var go = new GameObject("GradeDropdown", typeof(RectTransform), typeof(Image), typeof(TMP_Dropdown));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(420f, 70f);

            var bg = go.GetComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            if (TMP_Settings.defaultFontAsset != null)
                label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = 28f;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = new Vector2(20f, 6f);
            labelRt.offsetMax = new Vector2(-40f, -6f);

            var arrowGo = new GameObject("Arrow", typeof(RectTransform));
            arrowGo.transform.SetParent(go.transform, false);
            var arrow = arrowGo.AddComponent<TextMeshProUGUI>();
            if (TMP_Settings.defaultFontAsset != null)
                arrow.font = TMP_Settings.defaultFontAsset;
            arrow.text = "▼";
            arrow.fontSize = 24f;
            arrow.alignment = TextAlignmentOptions.Center;
            var arrowRt = arrowGo.GetComponent<RectTransform>();
            arrowRt.anchorMin = new Vector2(1f, 0f);
            arrowRt.anchorMax = new Vector2(1f, 1f);
            arrowRt.sizeDelta = new Vector2(36f, 0f);
            arrowRt.anchoredPosition = new Vector2(-18f, 0f);

            var dropdown = go.GetComponent<TMP_Dropdown>();
            dropdown.targetGraphic = bg;
            dropdown.captionText = label;
            dropdown.template = template.GetComponent<RectTransform>();
            var itemLabel = template.transform.Find("Viewport/Content/Item/Item Label")?.GetComponent<TextMeshProUGUI>();
            if (itemLabel != null)
                dropdown.itemText = itemLabel;
            dropdown.options.Clear();
            dropdown.options.Add(new TMP_Dropdown.OptionData("Grade 1"));
            dropdown.options.Add(new TMP_Dropdown.OptionData("Grade 2"));
            dropdown.options.Add(new TMP_Dropdown.OptionData("Grade 3"));
            dropdown.options.Add(new TMP_Dropdown.OptionData("Grade 4"));
            dropdown.value = Mathf.Clamp(selectedIndex, 0, 3);
            dropdown.RefreshShownValue();
            return dropdown;
        }

        /// <summary>Dropdown with empty options — fill before use.</summary>
        public static TMP_Dropdown CreateLabeledDropdown(RectTransform parent, string objectName, int selectedIndex)
        {
            var template = CreateDropdownTemplate(parent);
            var go = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(TMP_Dropdown));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(420f, 70f);

            var bg = go.GetComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            if (TMP_Settings.defaultFontAsset != null)
                label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = 28f;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = new Vector2(20f, 6f);
            labelRt.offsetMax = new Vector2(-40f, -6f);

            var arrowGo = new GameObject("Arrow", typeof(RectTransform));
            arrowGo.transform.SetParent(go.transform, false);
            var arrow = arrowGo.AddComponent<TextMeshProUGUI>();
            if (TMP_Settings.defaultFontAsset != null)
                arrow.font = TMP_Settings.defaultFontAsset;
            arrow.text = "▼";
            arrow.fontSize = 24f;
            arrow.alignment = TextAlignmentOptions.Center;
            var arrowRt = arrowGo.GetComponent<RectTransform>();
            arrowRt.anchorMin = new Vector2(1f, 0f);
            arrowRt.anchorMax = new Vector2(1f, 1f);
            arrowRt.sizeDelta = new Vector2(36f, 0f);
            arrowRt.anchoredPosition = new Vector2(-18f, 0f);

            var dropdown = go.GetComponent<TMP_Dropdown>();
            dropdown.targetGraphic = bg;
            dropdown.captionText = label;
            dropdown.template = template.GetComponent<RectTransform>();
            var itemLabel = template.transform.Find("Viewport/Content/Item/Item Label")?.GetComponent<TextMeshProUGUI>();
            if (itemLabel != null)
                dropdown.itemText = itemLabel;
            dropdown.value = Mathf.Max(0, selectedIndex);
            dropdown.RefreshShownValue();
            return dropdown;
        }

        private static GameObject CreateDropdownTemplate(RectTransform parent)
        {
            var template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            template.transform.SetParent(parent, false);
            template.SetActive(false);
            var templateRt = template.GetComponent<RectTransform>();
            templateRt.anchorMin = new Vector2(0f, 0f);
            templateRt.anchorMax = new Vector2(1f, 0f);
            templateRt.pivot = new Vector2(0.5f, 1f);
            templateRt.sizeDelta = new Vector2(0f, 240f);
            template.GetComponent<Image>().color = new Color(0.12f, 0.12f, 0.16f, 1f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(template.transform, false);
            var viewportRt = viewport.GetComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;
            viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = new Vector2(0f, 240f);

            var item = new GameObject("Item", typeof(RectTransform), typeof(Toggle));
            item.transform.SetParent(content.transform, false);
            var itemRt = item.GetComponent<RectTransform>();
            itemRt.anchorMin = new Vector2(0f, 0.5f);
            itemRt.anchorMax = new Vector2(1f, 0.5f);
            itemRt.sizeDelta = new Vector2(0f, 48f);

            var itemBg = new GameObject("Item Background", typeof(RectTransform), typeof(Image));
            itemBg.transform.SetParent(item.transform, false);
            var itemBgRt = itemBg.GetComponent<RectTransform>();
            itemBgRt.anchorMin = Vector2.zero;
            itemBgRt.anchorMax = Vector2.one;
            itemBgRt.offsetMin = Vector2.zero;
            itemBgRt.offsetMax = Vector2.zero;
            itemBg.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 1f);

            var itemLabelGo = new GameObject("Item Label", typeof(RectTransform));
            itemLabelGo.transform.SetParent(item.transform, false);
            var itemLabel = itemLabelGo.AddComponent<TextMeshProUGUI>();
            if (TMP_Settings.defaultFontAsset != null)
                itemLabel.font = TMP_Settings.defaultFontAsset;
            itemLabel.fontSize = 26f;
            itemLabel.alignment = TextAlignmentOptions.MidlineLeft;
            var itemLabelRt = itemLabelGo.GetComponent<RectTransform>();
            itemLabelRt.anchorMin = Vector2.zero;
            itemLabelRt.anchorMax = Vector2.one;
            itemLabelRt.offsetMin = new Vector2(16f, 0f);
            itemLabelRt.offsetMax = new Vector2(-8f, 0f);

            var toggle = item.GetComponent<Toggle>();
            toggle.targetGraphic = itemBg.GetComponent<Image>();
            toggle.graphic = null;

            var scroll = template.GetComponent<ScrollRect>();
            scroll.content = contentRt;
            scroll.viewport = viewportRt;
            scroll.horizontal = false;
            scroll.vertical = true;

            return template;
        }
    }
}
