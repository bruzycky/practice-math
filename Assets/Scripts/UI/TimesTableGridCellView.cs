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
        private Coroutine _pulseRoutine;
        private TimesTableGridStripHighlight _strip = TimesTableGridStripHighlight.None;

        private static readonly Color DefaultProduct = new Color(0.14f, 0.22f, 0.36f, 1f);
        private static readonly Color HeaderColor = new Color(0.22f, 0.42f, 0.68f, 1f);
        private static readonly Color SelectedHeader = new Color(0.35f, 0.62f, 0.92f, 1f);
        private static readonly Color ColumnStripColor = new Color(0.2f, 0.48f, 0.72f, 1f);
        private static readonly Color RowStripColor = new Color(0.24f, 0.52f, 0.62f, 1f);
        private static readonly Color IntersectionColor = new Color(0.95f, 0.78f, 0.22f, 1f);
        private static readonly Color WrongProduct = new Color(0.55f, 0.22f, 0.22f, 1f);

        public TimesTableGridCellRole Role => role;
        public int RowFactor => rowFactor;
        public int ColumnFactor => columnFactor;
        public int Product => rowFactor * columnFactor;

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
                background.color = selected ? SelectedHeader : HeaderColor;
        }

        public void SetStripHighlight(TimesTableGridStripHighlight strip)
        {
            _strip = strip;
            if (background == null || role != TimesTableGridCellRole.Product)
                return;

            background.color = strip switch
            {
                TimesTableGridStripHighlight.Column => ColumnStripColor,
                TimesTableGridStripHighlight.Row => RowStripColor,
                TimesTableGridStripHighlight.Intersection => IntersectionColor,
                _ => DefaultProduct
            };
        }

        public void SetWrongFlash()
        {
            if (background == null || role != TimesTableGridCellRole.Product)
                return;
            background.color = WrongProduct;
        }

        public IEnumerator PlayPulse(float stepSeconds, float peakScale, bool settleHighlighted)
        {
            if (_rectTransform == null)
                _rectTransform = transform as RectTransform;
            if (_rectTransform == null)
                yield break;

            float half = stepSeconds * 0.45f;
            yield return ScaleOverTime(1f, peakScale, half);
            yield return ScaleOverTime(peakScale, settleHighlighted ? 1.08f : 1f, half);
        }

        public void StopPulse()
        {
            if (_pulseRoutine != null)
            {
                StopCoroutine(_pulseRoutine);
                _pulseRoutine = null;
            }

            if (_rectTransform != null)
                _rectTransform.localScale = Vector3.one;
        }

        public void ResetVisuals()
        {
            StopPulse();
            _strip = TimesTableGridStripHighlight.None;
            SetHeaderSelected(false);
            ApplyBaseColor();
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
