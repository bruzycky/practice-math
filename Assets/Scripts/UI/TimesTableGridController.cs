using System.Collections;
using TMPro;
using UnityEngine;

namespace PracticeMath.UI
{
    /// <summary>Interactive 1–12 multiplication chart. Pick row and column factors, then tap the product cell.</summary>
    public sealed class TimesTableGridController : MonoBehaviour
    {
        private const int MinFactor = 1;
        private const int MaxFactor = 12;

        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private TimesTableGridCellView[] cells;

        private int _selectedRow = -1;
        private int _selectedColumn = -1;
        private Coroutine _feedbackResetRoutine;

        private void Start()
        {
            RefreshInstruction();
            if (feedbackText != null)
                feedbackText.text = string.Empty;
        }

        public void NotifyCellClicked(TimesTableGridCellView cell)
        {
            if (cell == null)
                return;

            switch (cell.Role)
            {
                case TimesTableGridCellRole.RowHeader:
                    SelectRow(cell.RowFactor);
                    break;
                case TimesTableGridCellRole.ColumnHeader:
                    SelectColumn(cell.ColumnFactor);
                    break;
                case TimesTableGridCellRole.Product:
                    OnProductClicked(cell);
                    break;
            }
        }

        private void SelectRow(int row)
        {
            if (row < MinFactor || row > MaxFactor)
                return;
            _selectedRow = row;
            UpdateHeaderHighlights();
            RefreshInstruction();
            ClearFeedback();
        }

        private void SelectColumn(int column)
        {
            if (column < MinFactor || column > MaxFactor)
                return;
            _selectedColumn = column;
            UpdateHeaderHighlights();
            RefreshInstruction();
            ClearFeedback();
        }

        private void OnProductClicked(TimesTableGridCellView cell)
        {
            int row = cell.RowFactor;
            int column = cell.ColumnFactor;
            int product = cell.Product;

            if (_selectedRow >= MinFactor && _selectedColumn >= MinFactor)
            {
                int expected = _selectedRow * _selectedColumn;
                if (row == _selectedRow && column == _selectedColumn)
                {
                    ShowFeedback($"{_selectedRow} × {_selectedColumn} = {product}", false);
                    HighlightProductCell(cell, false);
                    ScheduleResetSelection(1.2f);
                    return;
                }

                ShowFeedback("Not that cell — find where your row and column meet.", true);
                HighlightProductCell(cell, true);
                StartCoroutine(ClearProductHighlightAfterDelay(cell, 0.45f));
                return;
            }

            _selectedRow = row;
            _selectedColumn = column;
            UpdateHeaderHighlights();
            ShowFeedback($"{row} × {column} = {product}", false);
            HighlightProductCell(cell, false);
            RefreshInstruction();
        }

        private void RefreshInstruction()
        {
            if (instructionText == null)
                return;

            if (_selectedRow >= MinFactor && _selectedColumn >= MinFactor)
            {
                int answer = _selectedRow * _selectedColumn;
                instructionText.text =
                    $"{_selectedRow} × {_selectedColumn} = ? Tap {answer} where that row and column meet.";
                return;
            }

            if (_selectedRow >= MinFactor)
            {
                instructionText.text = $"Row {_selectedRow} selected. Now tap a number on the top row.";
                return;
            }

            if (_selectedColumn >= MinFactor)
            {
                instructionText.text = $"Column {_selectedColumn} selected. Now tap a number on the left.";
                return;
            }

            instructionText.text = "Tap a row number, then a column number, then tap the matching answer in the chart.";
        }

        private void UpdateHeaderHighlights()
        {
            if (cells == null)
                return;

            foreach (var cell in cells)
            {
                if (cell == null)
                    continue;
                if (cell.Role == TimesTableGridCellRole.RowHeader)
                    cell.SetHeaderSelected(cell.RowFactor == _selectedRow);
                else if (cell.Role == TimesTableGridCellRole.ColumnHeader)
                    cell.SetHeaderSelected(cell.ColumnFactor == _selectedColumn);
            }
        }

        private void HighlightProductCell(TimesTableGridCellView cell, bool wrong)
        {
            if (cells == null)
                return;
            foreach (var c in cells)
            {
                if (c != null && c.Role == TimesTableGridCellRole.Product)
                    c.SetProductHighlight(false);
            }

            cell?.SetProductHighlight(true, wrong);
        }

        private void ShowFeedback(string message, bool isTryAgain)
        {
            if (feedbackText == null)
                return;
            feedbackText.text = message;
            feedbackText.color = isTryAgain ? new Color(1f, 0.75f, 0.65f) : new Color(0.75f, 1f, 0.82f);
        }

        private void ClearFeedback()
        {
            if (_feedbackResetRoutine != null)
            {
                StopCoroutine(_feedbackResetRoutine);
                _feedbackResetRoutine = null;
            }

            if (feedbackText != null)
                feedbackText.text = string.Empty;
        }

        private IEnumerator ClearProductHighlightAfterDelay(TimesTableGridCellView cell, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            cell?.SetProductHighlight(false);
        }

        private void ScheduleResetSelection(float seconds)
        {
            if (_feedbackResetRoutine != null)
                StopCoroutine(_feedbackResetRoutine);
            _feedbackResetRoutine = StartCoroutine(ResetSelectionAfterDelayRoutine(seconds));
        }

        private IEnumerator ResetSelectionAfterDelayRoutine(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            _feedbackResetRoutine = null;
            ResetSelection();
        }

        private void ResetSelection()
        {
            _selectedRow = -1;
            _selectedColumn = -1;
            if (cells != null)
            {
                foreach (var cell in cells)
                {
                    if (cell != null)
                        cell.ResetVisuals();
                }
            }

            ClearFeedback();
            RefreshInstruction();
        }
    }
}
