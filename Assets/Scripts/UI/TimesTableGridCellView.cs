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
        private static readonly Color DefaultProduct = new Color(0.14f, 0.22f, 0.36f, 1f);
        private static readonly Color HeaderColor = new Color(0.22f, 0.42f, 0.68f, 1f);
        private static readonly Color SelectedHeader = new Color(0.35f, 0.62f, 0.92f, 1f);
        private static readonly Color HighlightProduct = new Color(0.28f, 0.55f, 0.38f, 1f);
        private static readonly Color WrongProduct = new Color(0.55f, 0.22f, 0.22f, 1f);

        public TimesTableGridCellRole Role => role;
        public int RowFactor => rowFactor;
        public int ColumnFactor => columnFactor;
        public int Product => rowFactor * columnFactor;

        private void Reset()
        {
            if (button == null)
                button = GetComponent<Button>();
            if (label == null)
                label = GetComponentInChildren<TextMeshProUGUI>();
            if (background == null)
                background = GetComponent<Image>();
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

            if (background != null)
            {
                background.color = role switch
                {
                    TimesTableGridCellRole.Corner => new Color(0.08f, 0.1f, 0.14f, 1f),
                    TimesTableGridCellRole.RowHeader or TimesTableGridCellRole.ColumnHeader => HeaderColor,
                    _ => DefaultProduct
                };
            }

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                if (role != TimesTableGridCellRole.Corner && _owner != null)
                    button.onClick.AddListener(() => _owner.NotifyCellClicked(this));
                button.interactable = role != TimesTableGridCellRole.Corner;
            }
        }

        public void SetHeaderSelected(bool selected)
        {
            if (background == null || role is not (TimesTableGridCellRole.RowHeader or TimesTableGridCellRole.ColumnHeader))
                return;
            background.color = selected ? SelectedHeader : HeaderColor;
        }

        public void SetProductHighlight(bool on, bool wrong = false)
        {
            if (background == null || role != TimesTableGridCellRole.Product)
                return;
            if (!on)
            {
                background.color = DefaultProduct;
                return;
            }

            background.color = wrong ? WrongProduct : HighlightProduct;
        }

        public void ResetVisuals()
        {
            SetHeaderSelected(false);
            SetProductHighlight(false);
        }
    }
}
