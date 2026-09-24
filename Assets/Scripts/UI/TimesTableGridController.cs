using System.Collections;
using TMPro;
using UnityEngine;

namespace PracticeMath.UI
{
    /// <summary>Interactive 1–12 multiplication chart with animated row/column highlights.</summary>
    public sealed class TimesTableGridController : MonoBehaviour
    {
        private const int MinFactor = 1;
        private const int MaxFactor = 12;

        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private TimesTableGridCellView[] cells;
        [SerializeField] private float stepSeconds = 0.1f;
        [SerializeField] private float pulsePeakScale = 1.18f;

        private readonly TimesTableGridCellView[,] _products = new TimesTableGridCellView[MaxFactor + 1, MaxFactor + 1];
        private readonly TimesTableGridCellView[] _rowHeaders = new TimesTableGridCellView[MaxFactor + 1];
        private readonly TimesTableGridCellView[] _colHeaders = new TimesTableGridCellView[MaxFactor + 1];

        private int _selectedRow = -1;
        private int _selectedColumn = -1;
        private Coroutine _stripRoutine;

        private void Start()
        {
            BuildLookup();
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
            ClearFeedback();
            RestartStripAnimation();
            RefreshInstruction();
        }

        private void SelectColumn(int column)
        {
            if (column < MinFactor || column > MaxFactor)
                return;

            _selectedColumn = column;
            UpdateHeaderHighlights();
            ClearFeedback();
            RestartStripAnimation();
            RefreshInstruction();
        }

        private void OnProductClicked(TimesTableGridCellView cell)
        {
            if (_selectedRow >= MinFactor && _selectedColumn >= MinFactor)
            {
                if (cell.RowFactor == _selectedRow && cell.ColumnFactor == _selectedColumn)
                {
                    ShowFeedback($"{_selectedRow} × {_selectedColumn} = {cell.Product}", false);
                    return;
                }

                ShowFeedback("Follow the highlighted row and column to where they meet.", true);
                cell.SetWrongFlash();
                StartCoroutine(ClearWrongAfter(cell, 0.35f));
                return;
            }

            _selectedRow = cell.RowFactor;
            _selectedColumn = cell.ColumnFactor;
            UpdateHeaderHighlights();
            RestartStripAnimation();
            ShowFeedback($"{cell.RowFactor} × {cell.ColumnFactor} = {cell.Product}", false);
            RefreshInstruction();
        }

        private void RestartStripAnimation()
        {
            if (_stripRoutine != null)
                StopCoroutine(_stripRoutine);

            ClearProductStripHighlights();

            if (_selectedColumn >= MinFactor && _selectedRow >= MinFactor)
                _stripRoutine = StartCoroutine(AnimateBothSelections());
            else if (_selectedColumn >= MinFactor)
                _stripRoutine = StartCoroutine(AnimateColumn(_selectedColumn, stopAtRow: -1));
            else if (_selectedRow >= MinFactor)
                _stripRoutine = StartCoroutine(AnimateRow(_selectedRow, stopAtColumn: -1));
        }

        private IEnumerator AnimateBothSelections()
        {
            yield return StartCoroutine(AnimateColumn(_selectedColumn, stopAtRow: -1));
            yield return StartCoroutine(AnimateRow(_selectedRow, stopAtColumn: _selectedColumn, finaleAtIntersection: true));
            ShowIntersectionAnswer();
        }

        private IEnumerator AnimateColumn(int column, int stopAtRow)
        {
            for (int row = MinFactor; row <= MaxFactor; row++)
            {
                if (stopAtRow >= MinFactor && row > stopAtRow)
                    break;

                var cell = _products[row, column];
                if (cell == null)
                    continue;

                bool isIntersection = _selectedRow == row && _selectedColumn == column;
                cell.SetStripHighlight(isIntersection ? TimesTableGridStripHighlight.Intersection : TimesTableGridStripHighlight.Column);
                yield return cell.PlayPulse(stepSeconds, pulsePeakScale, settleHighlighted: true);
            }
        }

        private IEnumerator AnimateRow(int row, int stopAtColumn, bool finaleAtIntersection = false)
        {
            for (int col = MinFactor; col <= MaxFactor; col++)
            {
                if (stopAtColumn >= MinFactor && col > stopAtColumn)
                    break;

                var cell = _products[row, col];
                if (cell == null)
                    continue;

                bool isIntersection = finaleAtIntersection && col == stopAtColumn && row == _selectedRow;
                if (isIntersection)
                    cell.SetStripHighlight(TimesTableGridStripHighlight.Intersection);
                else if (cell != null)
                    cell.SetStripHighlight(TimesTableGridStripHighlight.Row);

                float peak = isIntersection ? pulsePeakScale * 1.12f : pulsePeakScale;
                yield return cell.PlayPulse(stepSeconds, peak, settleHighlighted: true);
            }
        }

        private void ShowIntersectionAnswer()
        {
            if (_selectedRow < MinFactor || _selectedColumn < MinFactor)
                return;

            int product = _selectedRow * _selectedColumn;
            var cell = _products[_selectedRow, _selectedColumn];
            if (cell != null)
                cell.SetStripHighlight(TimesTableGridStripHighlight.Intersection);

            ShowFeedback($"{_selectedRow} × {_selectedColumn} = {product}", false);
            RefreshInstruction();
        }

        private void RefreshInstruction()
        {
            if (instructionText == null)
                return;

            if (_selectedRow >= MinFactor && _selectedColumn >= MinFactor)
            {
                instructionText.text = $"Watch the highlights meet at {_selectedRow} × {_selectedColumn}.";
                return;
            }

            if (_selectedRow >= MinFactor)
            {
                instructionText.text = $"Row {_selectedRow} selected. Now tap a number along the top.";
                return;
            }

            if (_selectedColumn >= MinFactor)
            {
                instructionText.text = $"Column {_selectedColumn} selected. Now tap a number on the left.";
                return;
            }

            instructionText.text = "Tap a number on the top, then on the left, to see where they meet.";
        }

        private void UpdateHeaderHighlights()
        {
            for (int i = MinFactor; i <= MaxFactor; i++)
            {
                if (_rowHeaders[i] != null)
                    _rowHeaders[i].SetHeaderSelected(i == _selectedRow);
                if (_colHeaders[i] != null)
                    _colHeaders[i].SetHeaderSelected(i == _selectedColumn);
            }
        }

        private void ClearProductStripHighlights()
        {
            for (int r = MinFactor; r <= MaxFactor; r++)
            {
                for (int c = MinFactor; c <= MaxFactor; c++)
                {
                    if (_products[r, c] != null)
                        _products[r, c].ResetVisuals();
                }
            }
        }

        private void BuildLookup()
        {
            for (int r = 0; r <= MaxFactor; r++)
            {
                for (int c = 0; c <= MaxFactor; c++)
                    _products[r, c] = null;
                _rowHeaders[r] = null;
                _colHeaders[r] = null;
            }

            if (cells == null)
                return;

            foreach (var cell in cells)
            {
                cell?.EnsureClickWired();
            }

            foreach (var cell in cells)
            {
                if (cell == null)
                    continue;

                switch (cell.Role)
                {
                    case TimesTableGridCellRole.Product:
                        if (cell.RowFactor >= MinFactor && cell.RowFactor <= MaxFactor &&
                            cell.ColumnFactor >= MinFactor && cell.ColumnFactor <= MaxFactor)
                            _products[cell.RowFactor, cell.ColumnFactor] = cell;
                        break;
                    case TimesTableGridCellRole.RowHeader:
                        if (cell.RowFactor >= MinFactor && cell.RowFactor <= MaxFactor)
                            _rowHeaders[cell.RowFactor] = cell;
                        break;
                    case TimesTableGridCellRole.ColumnHeader:
                        if (cell.ColumnFactor >= MinFactor && cell.ColumnFactor <= MaxFactor)
                            _colHeaders[cell.ColumnFactor] = cell;
                        break;
                }
            }
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
            if (feedbackText != null)
                feedbackText.text = string.Empty;
        }

        private IEnumerator ClearWrongAfter(TimesTableGridCellView cell, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (cell == null)
                yield break;
            if (_selectedRow >= MinFactor && _selectedColumn >= MinFactor)
                RestartStripAnimation();
            else
                cell.ResetVisuals();
        }

    }
}
