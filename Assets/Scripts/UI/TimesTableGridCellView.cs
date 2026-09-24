using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PracticeMath.UI
{
    public enum TimesTableGridCellRole
    {
        Corner = 0,
        RowHeader = 1,
        ColumnHeader = 2,
        Product = 3
    }

    public enum TimesTableGridStripHighlight
    {
        None = 0,
        Column = 1,
        Row = 2,
        Intersection = 3
    }

    /// <summary>One cell in the 1–12 multiplication chart. Wire on the grid prefab or let the layout builder create instances.</summary>
    public sealed class TimesTableGridCellView : MonoBehaviour
    {
        [SerializeField] private TimesTableGridCellRole role;
        [SerializeField] private int rowFactor;
        [SerializeField] private int columnFactor;
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Image background;

        private TimesTableGridController _owner;
        private RectTransform _rectTransform;
        private Outline _choiceOutline;
        private bool _pendingChoice;
        private TimesTableGridStripHighlight _strip = TimesTableGridStripHighlight.None;

        private static readonly Color DefaultProduct = new Color(0.14f, 0.22f, 0.36f, 1f);
        private static readonly Color DefaultProductText = Color.white;
        private static readonly Color IntersectionText = Color.black;
        private static readonly Color HeaderColor = new Color(0.22f, 0.42f, 0.68f, 1f);
        private static readonly Color SelectedHeader = new Color(0.35f, 0.62f, 0.92f, 1f);
        private static readonly Color ColumnStripColor = new Color(0.2f, 0.48f, 0.72f, 1f);
        private static readonly Color RowStripColor = new Color(0.24f, 0.52f, 0.62f, 1f);
        private static readonly Color IntersectionColor = new Color(0.95f, 0.78f, 0.22f, 1f);
        private static readonly Color PendingProductTint = new Color(0.2f, 0.34f, 0.46f, 1f);
        private static readonly Color PendingHeaderTint = new Color(0.28f, 0.52f, 0.78f, 1f);
        private static readonly Color PendingOutlineColor = new Color(0.92f, 0.96f, 1f, 0.95f);

        public TimesTableGridCellRole Role => role;
        public bool IsPendingChoice => _pendingChoice;
        public int RowFactor => rowFactor;
        public int ColumnFactor => columnFactor;
        public int Product => rowFactor * columnFactor;
        public TimesTableGridStripHighlight StripHighlight => _strip;

        private void Awake()
        {
            Reset();
            WireClick();
        }

        private void Reset()
        {
            if (button == null)
                button = GetComponent<Button>();
            if (label == null)
                label = GetComponentInChildren<TextMeshProUGUI>();
            if (background == null)
                background = GetComponent<Image>();
            if (_rectTransform == null)
                _rectTransform = transform as RectTransform;
        }

        public void Configure(
            TimesTableGridController owner,
            TimesTableGridCellRole cellRole,
            int row,
            int column,
            string displayText)
        {
            _owner = owner;
            role = cellRole;
            rowFactor = row;
            columnFactor = column;
            Reset();

            if (label != null)
                label.text = displayText;

            ApplyBaseColor();
            RestoreProductLabelStyle();
            WireClick();
        }

        public void EnsureClickWired()
        {
            WireClick();
        }

        private void WireClick()
        {
            if (button == null)
                return;

            button.onClick.RemoveAllListeners();
            if (role == TimesTableGridCellRole.Corner)
            {
                button.interactable = false;
                return;
            }

            button.interactable = true;

            var owner = _owner != null ? _owner : GetComponentInParent<TimesTableGridController>();
            if (owner == null)
                return;

            _owner = owner;
            button.onClick.AddListener(() => owner.NotifyCellClicked(this));
        }

        private void ApplyBaseColor()
        {
            if (background == null)
                return;

            background.color = role switch
            {
                TimesTableGridCellRole.Corner => new Color(0.08f, 0.1f, 0.14f, 0.45f),
                TimesTableGridCellRole.RowHeader or TimesTableGridCellRole.ColumnHeader => HeaderColor,
                _ => DefaultProduct
            };
        }

        public void SetHeaderSelected(bool selected)
        {
            if (background == null)
                return;

            if (role is TimesTableGridCellRole.RowHeader or TimesTableGridCellRole.ColumnHeader)
            {
                if (_pendingChoice)
                    return;
                background.color = selected ? SelectedHeader : HeaderColor;
            }
        }

        public void SetPendingChoice(bool pending)
        {
            if (role == TimesTableGridCellRole.Corner)
                return;

            _pendingChoice = pending;
            EnsureChoiceOutline();

            if (_choiceOutline != null)
            {
                _choiceOutline.enabled = pending;
                _choiceOutline.effectColor = PendingOutlineColor;
                _choiceOutline.effectDistance = new Vector2(2.5f, -2.5f);
            }

            if (!pending)
            {
                if (_strip != TimesTableGridStripHighlight.None && role == TimesTableGridCellRole.Product)
                    SetStripHighlight(_strip);
                else if (role is TimesTableGridCellRole.RowHeader or TimesTableGridCellRole.ColumnHeader)
                    ApplyBaseColor();
                else
                    ApplyBaseColor();
                return;
            }

            if (background == null)
                return;

            background.color = role is TimesTableGridCellRole.RowHeader or TimesTableGridCellRole.ColumnHeader
                ? PendingHeaderTint
                : PendingProductTint;
        }

        private void EnsureChoiceOutline()
        {
            if (_choiceOutline != null || background == null)
                return;

            _choiceOutline = background.GetComponent<Outline>();
            if (_choiceOutline == null)
                _choiceOutline = background.gameObject.AddComponent<Outline>();
        }

        public void SetStripHighlight(TimesTableGridStripHighlight strip)
        {
            _strip = strip;
            _pendingChoice = false;
            if (_choiceOutline != null)
                _choiceOutline.enabled = false;

            if (background == null || role != TimesTableGridCellRole.Product)
                return;

            background.color = strip switch
            {
                TimesTableGridStripHighlight.Column => ColumnStripColor,
                TimesTableGridStripHighlight.Row => RowStripColor,
                TimesTableGridStripHighlight.Intersection => IntersectionColor,
                _ => DefaultProduct
            };

            ApplyProductLabelForStrip(strip);
        }

        private void ApplyProductLabelForStrip(TimesTableGridStripHighlight strip)
        {
            if (label == null || role != TimesTableGridCellRole.Product)
                return;

            if (strip == TimesTableGridStripHighlight.Intersection)
            {
                label.color = IntersectionText;
                label.fontStyle = FontStyles.Bold;
                return;
            }

            RestoreProductLabelStyle();
        }

        private void RestoreProductLabelStyle()
        {
            if (label == null || role != TimesTableGridCellRole.Product)
                return;

            label.color = DefaultProductText;
            label.fontStyle = FontStyles.Normal;
        }

        public IEnumerator PlayPulse(float stepSeconds, float peakScale)
        {
            if (_rectTransform == null)
                _rectTransform = transform as RectTransform;
            if (_rectTransform == null)
                yield break;

            float half = stepSeconds * 0.45f;
            yield return ScaleOverTime(1f, peakScale, half);
            yield return ScaleOverTime(peakScale, 1f, half);
        }

        public IEnumerator FadeToDefault(float duration)
        {
            if (background == null || role != TimesTableGridCellRole.Product)
                yield break;

            Color startBg = background.color;
            Color startText = label != null ? label.color : DefaultProductText;
            FontStyles startStyle = label != null ? label.fontStyle : FontStyles.Normal;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float u = duration > 0f ? Mathf.Clamp01(t / duration) : 1f;
                background.color = Color.Lerp(startBg, DefaultProduct, u);
                if (label != null)
                {
                    label.color = Color.Lerp(startText, DefaultProductText, u);
                    if (u > 0.5f && startStyle == FontStyles.Bold)
                        label.fontStyle = FontStyles.Normal;
                }

                yield return null;
            }

            _strip = TimesTableGridStripHighlight.None;
            background.color = DefaultProduct;
            RestoreProductLabelStyle();
            if (_rectTransform != null)
                _rectTransform.localScale = Vector3.one;
        }

        public void StopPulse()
        {
            if (_rectTransform != null)
                _rectTransform.localScale = Vector3.one;
        }

        public void ResetVisuals()
        {
            StopPulse();
            _strip = TimesTableGridStripHighlight.None;
            _pendingChoice = false;
            if (_choiceOutline != null)
                _choiceOutline.enabled = false;
            SetHeaderSelected(false);
            ApplyBaseColor();
            RestoreProductLabelStyle();
        }

        private IEnumerator ScaleOverTime(float from, float to, float duration)
        {
            if (_rectTransform == null)
                yield break;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float u = duration > 0f ? Mathf.Clamp01(t / duration) : 1f;
                float s = Mathf.Lerp(from, to, u);
                _rectTransform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }

            _rectTransform.localScale = new Vector3(to, to, 1f);
        }
    }
}
