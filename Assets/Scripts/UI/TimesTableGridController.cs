using System.Collections;
using PracticeMath.Analytics;
using PracticeMath.Core;
using PracticeMath.Navigation;
using TMPro;
using UnityEngine;

namespace PracticeMath.UI
{
    /// <summary>Interactive 1–12 multiplication chart with animated row/column highlights.</summary>
    public sealed class TimesTableGridController : MonoBehaviour
    {
        private enum ChartAnimationPath
        {
            None = 0,
            ColumnOnly = 1,
            RowOnly = 2,
            PairColumnFirst = 3,
            PairRowFirst = 4,
            ProductRadiate = 5
        }

        private const int MinFactor = 1;
        private const int MaxFactor = 12;
        private const float RetractFadeStepScale = 0.275f;
        private const float RetractWavePauseScale = 0.85f;

        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private TimesTableGridCellView[] cells;
        [SerializeField] private float stepSeconds = 0.1f;
        [SerializeField] private float pulsePeakScale = 1.18f;
        [SerializeField] private float trimFadeSeconds = 0.2f;

        private readonly TimesTableGridCellView[,] _products = new TimesTableGridCellView[MaxFactor + 1, MaxFactor + 1];
        private readonly TimesTableGridCellView[] _rowHeaders = new TimesTableGridCellView[MaxFactor + 1];
        private readonly TimesTableGridCellView[] _colHeaders = new TimesTableGridCellView[MaxFactor + 1];

        private int _selectedRow = -1;
        private int _selectedColumn = -1;
        private bool _columnStripShown;
        private bool _rowStripShown;
        private ChartAnimationPath _animationPath = ChartAnimationPath.None;
        private Coroutine _stripRoutine;
        private bool _inputLocked;
        private TimesTableGridCellView _pendingChoiceCell;

        private void OnEnable()
        {
            if (AppThemeContext.Instance != null)
                AppThemeContext.Instance.Changed += ApplyTheme;
        }

        private void OnDisable()
        {
            if (AppThemeContext.Instance != null)
                AppThemeContext.Instance.Changed -= ApplyTheme;

            StopStripRoutine();
            StopAllCoroutines();
            _inputLocked = false;
            UiEventSystemUtility.ClearSelectionIfUnder(transform);
        }

        public void ApplyTheme()
        {
            if (cells == null)
                return;

            foreach (var cell in cells)
                cell?.ReapplyThemeFromProvider();

            UpdateHeaderHighlights();
            if (instructionText != null)
                instructionText.color = AppThemeContext.Instance != null
                    ? AppThemeContext.Instance.CurrentRoles.Text
                    : UiColorSchemeCatalog.Get(0).ResolveRoles().Text;
        }

        private void Start()
        {
            BuildLookup();
            RefreshInstruction();
            ApplyTheme();
            if (feedbackText != null)
                feedbackText.text = string.Empty;
        }

        public void NotifyCellClicked(TimesTableGridCellView cell)
        {
            if (cell == null || _inputLocked)
                return;

            StartCoroutine(ProcessCellClicked(cell));
        }

        private IEnumerator ProcessCellClicked(TimesTableGridCellView cell)
        {
            _inputLocked = true;
            StopStripRoutine();

            if (ShouldReverseBefore(cell))
            {
                SetPendingChoiceCell(cell);
                yield return ReverseHighlightAnimation();
            }

            switch (cell.Role)
            {
                case TimesTableGridCellRole.RowHeader:
                    yield return ApplyRowSelection(cell.RowFactor);
                    break;
                case TimesTableGridCellRole.ColumnHeader:
                    yield return ApplyColumnSelection(cell.ColumnFactor);
                    break;
                case TimesTableGridCellRole.Product:
                    yield return ApplyProductSelection(cell);
                    break;
            }

            _inputLocked = false;
        }

        private bool ShouldReverseBefore(TimesTableGridCellView cell)
        {
            if (_animationPath == ChartAnimationPath.None)
                return false;

            if (!HasBothSelected())
            {
                if (cell.Role == TimesTableGridCellRole.ColumnHeader &&
                    _columnStripShown &&
                    cell.ColumnFactor != _selectedColumn)
                    return true;
                if (cell.Role == TimesTableGridCellRole.RowHeader &&
                    _rowStripShown &&
                    cell.RowFactor != _selectedRow)
                    return true;
                return false;
            }

            if (cell.Role == TimesTableGridCellRole.Product)
                return true;
            if (cell.Role == TimesTableGridCellRole.RowHeader && cell.RowFactor != _selectedRow)
                return true;
            if (cell.Role == TimesTableGridCellRole.ColumnHeader && cell.ColumnFactor != _selectedColumn)
                return true;
            return false;
        }

        private IEnumerator ApplyRowSelection(int row)
        {
            if (row < MinFactor || row > MaxFactor)
                yield break;

            if (HasBothSelected() && row == _selectedRow)
                yield break;

            bool completingPair = _selectedColumn >= MinFactor && _columnStripShown && !HasBothSelected();
            _selectedRow = row;
            ClearPendingChoiceCell();
            UpdateHeaderHighlights();
            ClearFeedback();
            yield return RestartStripAnimation(completingPairFromColumn: completingPair);
            RefreshInstruction();
        }

        private IEnumerator ApplyColumnSelection(int column)
        {
            if (column < MinFactor || column > MaxFactor)
                yield break;

            if (HasBothSelected() && column == _selectedColumn)
                yield break;

            bool completingPair = _selectedRow >= MinFactor && _rowStripShown && !HasBothSelected();
            _selectedColumn = column;
            ClearPendingChoiceCell();
            UpdateHeaderHighlights();
            ClearFeedback();
            yield return RestartStripAnimation(completingPairFromRow: completingPair);
            RefreshInstruction();
        }

        private IEnumerator ApplyProductSelection(TimesTableGridCellView cell)
        {
            ClearPendingChoiceCell();
            ClearAllVisuals();

            _selectedRow = cell.RowFactor;
            _selectedColumn = cell.ColumnFactor;
            _columnStripShown = true;
            _rowStripShown = true;
            UpdateHeaderHighlights();
            ClearFeedback();
            yield return RunStripRoutine(AnimateRadiateFromProduct(cell));
            ShowFeedback(FormatMultiplicationFeedback(cell.RowFactor, cell.ColumnFactor), false);
            PracticeSessionAnalytics.Instance?.NotifyModuleExploration(LearningModule.TimesTableGrid);
            RefreshInstruction();
        }

        private bool HasBothSelected() =>
            _selectedRow >= MinFactor && _selectedColumn >= MinFactor;

        private void StopStripRoutine()
        {
            if (_stripRoutine != null)
            {
                StopCoroutine(_stripRoutine);
                _stripRoutine = null;
            }
        }

        private IEnumerator RunStripRoutine(IEnumerator routine)
        {
            StopStripRoutine();
            yield return routine;
        }

        private void SetPendingChoiceCell(TimesTableGridCellView cell)
        {
            ClearPendingChoiceCell();
            _pendingChoiceCell = cell;
            cell?.SetPendingChoice(true);
        }

        private void ClearPendingChoiceCell()
        {
            if (_pendingChoiceCell == null)
                return;

            _pendingChoiceCell.SetPendingChoice(false);
            _pendingChoiceCell = null;
        }

        private void ClearAllVisuals()
        {
            ClearProductStripHighlights(_pendingChoiceCell);
            for (int i = MinFactor; i <= MaxFactor; i++)
            {
                if (_rowHeaders[i] != null && _rowHeaders[i] != _pendingChoiceCell)
                    _rowHeaders[i].SetHeaderSelected(false);
                if (_colHeaders[i] != null && _colHeaders[i] != _pendingChoiceCell)
                    _colHeaders[i].SetHeaderSelected(false);
            }
        }

        private IEnumerator RestartStripAnimation(bool completingPairFromColumn = false, bool completingPairFromRow = false)
        {
            if (HasBothSelected() && completingPairFromColumn)
            {
                _rowStripShown = true;
                yield return RunStripRoutine(CompletePairAfterColumn());
                yield break;
            }

            if (HasBothSelected() && completingPairFromRow)
            {
                _columnStripShown = true;
                yield return RunStripRoutine(CompletePairAfterRow());
                yield break;
            }

            if (HasBothSelected())
            {
                ClearProductStripHighlights();
                _columnStripShown = true;
                _rowStripShown = true;
                yield return RunStripRoutine(AnimateFullPairFromScratch());
                yield break;
            }

            ClearProductStripHighlights();
            _columnStripShown = false;
            _rowStripShown = false;
            _animationPath = ChartAnimationPath.None;

            if (_selectedColumn >= MinFactor)
            {
                _columnStripShown = true;
                yield return RunStripRoutine(AnimateColumnStrip(_selectedColumn));
            }
            else if (_selectedRow >= MinFactor)
            {
                _rowStripShown = true;
                yield return RunStripRoutine(AnimateRowStrip(_selectedRow));
            }
        }

        private IEnumerator ReverseHighlightAnimation()
        {
            int row = _selectedRow;
            int col = _selectedColumn;
            var path = _animationPath;
            float fadeStep = RetractFadeStep;

            if (row >= MinFactor && col >= MinFactor &&
                path is ChartAnimationPath.ProductRadiate or ChartAnimationPath.PairColumnFirst or ChartAnimationPath.PairRowFirst)
            {
                var intersection = _products[row, col];
                if (intersection != null && intersection != _pendingChoiceCell)
                    yield return intersection.FadeToDefault(fadeStep);
            }

            switch (path)
            {
                case ChartAnimationPath.ProductRadiate:
                    yield return ReverseProductRadiate(row, col, fadeStep);
                    break;
                case ChartAnimationPath.PairColumnFirst:
                    yield return ReversePairColumnFirst(row, col, fadeStep);
                    break;
                case ChartAnimationPath.PairRowFirst:
                    yield return ReversePairRowFirst(row, col, fadeStep);
                    break;
                case ChartAnimationPath.ColumnOnly:
                    yield return ReverseColumnOnly(col, fadeStep);
                    break;
                case ChartAnimationPath.RowOnly:
                    yield return ReverseRowOnly(row, fadeStep);
                    break;
            }

            ClearAllVisuals();
            _animationPath = ChartAnimationPath.None;
            _selectedRow = -1;
            _selectedColumn = -1;
            _columnStripShown = false;
            _rowStripShown = false;
            ReassertPendingChoice();
        }

        private float RetractFadeStep => trimFadeSeconds * RetractFadeStepScale;

        private void ReassertPendingChoice()
        {
            if (_pendingChoiceCell != null)
                _pendingChoiceCell.SetPendingChoice(true);
        }

        private IEnumerator FadeRetractCell(TimesTableGridCellView cell, float fadeStep)
        {
            if (cell == null || cell == _pendingChoiceCell)
                yield break;
            yield return cell.FadeToDefault(fadeStep);
        }

        private IEnumerator ReversePairColumnFirst(int row, int col, float fadeStep)
        {
            yield return ReversePairArmsParallel(
                fadeStep,
                rowArmRow: row,
                rowArmColFrom: col - 1,
                rowArmColTo: MinFactor,
                colArmCol: col,
                colArmRowFrom: MaxFactor,
                colArmRowTo: MinFactor);

            TurnOffPairHeaders(row, col);
        }

        private IEnumerator ReversePairRowFirst(int row, int col, float fadeStep)
        {
            yield return ReversePairArmsParallel(
                fadeStep,
                rowArmRow: row,
                rowArmColFrom: MaxFactor,
                rowArmColTo: MinFactor,
                colArmCol: col,
                colArmRowFrom: row - 1,
                colArmRowTo: MinFactor);

            TurnOffPairHeaders(row, col);
        }

        private IEnumerator ReversePairArmsParallel(
            float fadeStep,
            int rowArmRow,
            int rowArmColFrom,
            int rowArmColTo,
            int colArmCol,
            int colArmRowFrom,
            int colArmRowTo)
        {
            int rowArmCol = rowArmColFrom;
            int colArmRow = colArmRowFrom;
            float wavePause = fadeStep * RetractWavePauseScale;

            while (rowArmCol >= rowArmColTo || colArmRow >= colArmRowTo)
            {
                TimesTableGridCellView alongRow = rowArmCol >= rowArmColTo ? _products[rowArmRow, rowArmCol] : null;
                TimesTableGridCellView alongCol = colArmRow >= colArmRowTo ? _products[colArmRow, colArmCol] : null;

                if (alongRow == null && alongCol == null)
                    break;

                StartRetractFade(alongRow, fadeStep);
                StartRetractFade(alongCol, fadeStep);

                if (rowArmCol >= rowArmColTo)
                    rowArmCol--;
                if (colArmRow >= colArmRowTo)
                    colArmRow--;

                yield return new WaitForSeconds(wavePause);
            }
        }

        private void StartRetractFade(TimesTableGridCellView cell, float fadeStep)
        {
            if (cell == null || cell == _pendingChoiceCell)
                return;
            StartCoroutine(cell.FadeToDefault(fadeStep));
        }

        private void TurnOffPairHeaders(int row, int col)
        {
            if (_rowHeaders[row] != null && _rowHeaders[row] != _pendingChoiceCell)
                _rowHeaders[row].SetHeaderSelected(false);
            if (_colHeaders[col] != null && _colHeaders[col] != _pendingChoiceCell)
                _colHeaders[col].SetHeaderSelected(false);
        }

        private IEnumerator ReverseProductRadiate(int row, int col, float fadeStep)
        {
            if (_rowHeaders[row] != null && _rowHeaders[row] != _pendingChoiceCell)
                _rowHeaders[row].SetHeaderSelected(false);
            if (_colHeaders[col] != null && _colHeaders[col] != _pendingChoiceCell)
                _colHeaders[col].SetHeaderSelected(false);

            float wavePause = fadeStep * RetractWavePauseScale;
            for (int step = 0; step < MaxFactor; step++)
            {
                int fadeCol = col - 1 - step;
                int fadeRow = row - 1 - step;
                TimesTableGridCellView leftCell = fadeCol >= MinFactor ? _products[row, fadeCol] : null;
                TimesTableGridCellView upCell = fadeRow >= MinFactor ? _products[fadeRow, col] : null;

                if (leftCell == null && upCell == null)
                    break;

                StartRetractFade(leftCell, fadeStep);
                StartRetractFade(upCell, fadeStep);

                yield return new WaitForSeconds(wavePause);
            }
        }

        private IEnumerator ReverseColumnOnly(int col, float fadeStep)
        {
            for (int r = MaxFactor; r >= MinFactor; r--)
                yield return FadeRetractCell(_products[r, col], fadeStep);

            if (_colHeaders[col] != null && _colHeaders[col] != _pendingChoiceCell)
                _colHeaders[col].SetHeaderSelected(false);
        }

        private IEnumerator ReverseRowOnly(int row, float fadeStep)
        {
            for (int c = MaxFactor; c >= MinFactor; c--)
                yield return FadeRetractCell(_products[row, c], fadeStep);

            if (_rowHeaders[row] != null && _rowHeaders[row] != _pendingChoiceCell)
                _rowHeaders[row].SetHeaderSelected(false);
        }

        private IEnumerator CompletePairAfterColumn()
        {
            yield return AnimateRowStrip(_selectedRow, stopAtColumn: _selectedColumn, finaleAtIntersection: true);
            yield return FadeOutBeyondIntersection();
            ShowIntersectionAnswer();
            _animationPath = ChartAnimationPath.PairColumnFirst;
        }

        private IEnumerator CompletePairAfterRow()
        {
            yield return AnimateColumnStrip(_selectedColumn, stopAtRow: _selectedRow, finaleAtIntersection: true);
            yield return FadeOutBeyondIntersection();
            ShowIntersectionAnswer();
            _animationPath = ChartAnimationPath.PairRowFirst;
        }

        private IEnumerator AnimateFullPairFromScratch()
        {
            yield return AnimateColumnStrip(_selectedColumn);
            yield return AnimateRowStrip(_selectedRow, stopAtColumn: _selectedColumn, finaleAtIntersection: true);
            yield return FadeOutBeyondIntersection();
            ShowIntersectionAnswer();
            _animationPath = ChartAnimationPath.PairColumnFirst;
        }

        private IEnumerator AnimateColumnStrip(int column, int stopAtRow = -1, bool finaleAtIntersection = false)
        {
            for (int row = MinFactor; row <= MaxFactor; row++)
            {
                if (stopAtRow >= MinFactor && row > stopAtRow)
                    break;

                var cell = _products[row, column];
                if (cell == null)
                    continue;

                bool isIntersection = finaleAtIntersection && row == _selectedRow && column == _selectedColumn;
                cell.SetStripHighlight(isIntersection ? TimesTableGridStripHighlight.Intersection : TimesTableGridStripHighlight.Column);
                float peak = isIntersection ? pulsePeakScale * 1.12f : pulsePeakScale;
                yield return cell.PlayPulse(stepSeconds, peak);
            }

            if (stopAtRow < 0 && !finaleAtIntersection)
                _animationPath = ChartAnimationPath.ColumnOnly;
        }

        private IEnumerator AnimateRowStrip(int row, int stopAtColumn = -1, bool finaleAtIntersection = false)
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
                else
                    cell.SetStripHighlight(TimesTableGridStripHighlight.Row);

                float peak = isIntersection ? pulsePeakScale * 1.12f : pulsePeakScale;
                yield return cell.PlayPulse(stepSeconds, peak);
            }

            if (stopAtColumn < 0 && !finaleAtIntersection)
                _animationPath = ChartAnimationPath.RowOnly;
        }

        private IEnumerator AnimateRadiateFromProduct(TimesTableGridCellView origin)
        {
            int row = origin.RowFactor;
            int col = origin.ColumnFactor;

            origin.SetStripHighlight(TimesTableGridStripHighlight.Intersection);
            yield return origin.PlayPulse(stepSeconds, pulsePeakScale * 1.12f);

            int leftCol = col - 1;
            int upRow = row - 1;
            while (leftCol >= MinFactor || upRow >= MinFactor)
            {
                TimesTableGridCellView leftCell = leftCol >= MinFactor ? _products[row, leftCol] : null;
                TimesTableGridCellView upCell = upRow >= MinFactor ? _products[upRow, col] : null;

                if (leftCell != null)
                {
                    leftCell.SetStripHighlight(TimesTableGridStripHighlight.Row);
                    leftCol--;
                }

                if (upCell != null)
                {
                    upCell.SetStripHighlight(TimesTableGridStripHighlight.Column);
                    upRow--;
                }

                yield return PlayPulseParallel(leftCell, upCell, pulsePeakScale);
            }

            var rowHeader = _rowHeaders[row];
            var colHeader = _colHeaders[col];
            if (rowHeader != null)
                rowHeader.SetHeaderSelected(true);
            if (colHeader != null)
                colHeader.SetHeaderSelected(true);
            yield return PlayPulseParallel(rowHeader, colHeader, pulsePeakScale);

            yield return FadeOutBeyondIntersection();
            _animationPath = ChartAnimationPath.ProductRadiate;
        }

        private IEnumerator PlayPulseParallel(TimesTableGridCellView a, TimesTableGridCellView b, float peak)
        {
            if (a == null && b == null)
                yield break;

            if (a != null)
                StartCoroutine(a.PlayPulse(stepSeconds, peak));
            if (b != null && b != a)
                StartCoroutine(b.PlayPulse(stepSeconds, peak));

            yield return new WaitForSeconds(stepSeconds);
        }

        private IEnumerator FadeOutBeyondIntersection()
        {
            if (!HasBothSelected())
                yield break;

            for (int r = _selectedRow + 1; r <= MaxFactor; r++)
            {
                var cell = _products[r, _selectedColumn];
                if (cell != null)
                    StartCoroutine(cell.FadeToDefault(trimFadeSeconds));
            }

            for (int c = _selectedColumn + 1; c <= MaxFactor; c++)
            {
                var cell = _products[_selectedRow, c];
                if (cell != null)
                    StartCoroutine(cell.FadeToDefault(trimFadeSeconds));
            }

            if (trimFadeSeconds > 0f)
                yield return new WaitForSeconds(trimFadeSeconds);
        }

        private void ShowIntersectionAnswer()
        {
            if (!HasBothSelected())
                return;

            int product = _selectedRow * _selectedColumn;
            var cell = _products[_selectedRow, _selectedColumn];
            if (cell != null)
                cell.SetStripHighlight(TimesTableGridStripHighlight.Intersection);

            ShowFeedback(FormatMultiplicationFeedback(_selectedRow, _selectedColumn), false);
            PracticeSessionAnalytics.Instance?.NotifyModuleExploration(LearningModule.TimesTableGrid);
            RefreshInstruction();
        }

        private static string FormatMultiplicationFeedback(int rowFactor, int columnFactor)
        {
            int product = rowFactor * columnFactor;
            string division = rowFactor == columnFactor
                ? $"{product} ÷ {columnFactor} = {rowFactor}"
                : $"{product} ÷ {columnFactor} = {rowFactor} or {product} ÷ {rowFactor} = {columnFactor}";
            return $"{rowFactor} × {columnFactor} = {product} ({division})";
        }

        private void RefreshInstruction()
        {
            if (instructionText == null)
                return;

            if (HasBothSelected())
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

            instructionText.text = "Tap a number on the top, then on the left, or tap an answer in the grid.";
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

        private void ClearProductStripHighlights(TimesTableGridCellView skipCell = null)
        {
            for (int r = MinFactor; r <= MaxFactor; r++)
            {
                for (int c = MinFactor; c <= MaxFactor; c++)
                {
                    var cell = _products[r, c];
                    if (cell != null && cell != skipCell)
                        cell.ResetVisuals();
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
                cell?.EnsureClickWired();

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
            var roles = AppThemeContext.Instance != null
                ? AppThemeContext.Instance.CurrentRoles
                : UiColorSchemeCatalog.Get(0).ResolveRoles();
            feedbackText.color = isTryAgain
                ? Color.Lerp(roles.Button, roles.Text, 0.35f)
                : roles.Text;
        }

        private void ClearFeedback()
        {
            if (feedbackText != null)
                feedbackText.text = string.Empty;
        }
    }
}
