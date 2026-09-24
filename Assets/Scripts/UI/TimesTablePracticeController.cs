using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace PracticeMath.UI
{
    /// <summary>Full 0–12 times table drill. Wire UI references on the TimesTables scene Canvas.</summary>
    public sealed class TimesTablePracticeController : MonoBehaviour
    {
        private const int MinFactor = 0;
        private const int MaxFactor = 12;

        [SerializeField] private TMP_Dropdown tableDropdown;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private TextMeshProUGUI answerText;
        [SerializeField] private TextMeshProUGUI feedbackText;

        private readonly StringBuilder _digits = new StringBuilder();
        private readonly HashSet<string> _askedKeys = new HashSet<string>();

        private int _left;
        private int _right;
        private int _correctAnswer;

        private void Start()
        {
            PopulateTableDropdown();
            if (tableDropdown != null)
                tableDropdown.onValueChanged.AddListener(_ => { _askedKeys.Clear(); NextProblem(); });
            NextProblem();
        }

        private void OnDestroy()
        {
            if (tableDropdown != null)
                tableDropdown.onValueChanged.RemoveAllListeners();
        }

        private void PopulateTableDropdown()
        {
            if (tableDropdown == null)
                return;

            tableDropdown.options.Clear();
            tableDropdown.options.Add(new TMP_Dropdown.OptionData("Mixed (all tables)"));
            for (int i = MinFactor; i <= MaxFactor; i++)
                tableDropdown.options.Add(new TMP_Dropdown.OptionData($"Table ×{i}"));
            tableDropdown.value = 0;
            tableDropdown.RefreshShownValue();
        }

        private void NextProblem()
        {
            if (promptText == null)
                return;

            int tableFocus = tableDropdown != null && tableDropdown.value > 0
                ? tableDropdown.value - 1
                : -1;

            for (int attempt = 0; attempt < 512; attempt++)
            {
                if (tableFocus >= MinFactor && tableFocus <= MaxFactor)
                {
                    _left = tableFocus;
                    _right = Random.Range(MinFactor, MaxFactor + 1);
                }
                else
                {
                    _left = Random.Range(MinFactor, MaxFactor + 1);
                    _right = Random.Range(MinFactor, MaxFactor + 1);
                }

                string key = _left + "x" + _right;
                if (!_askedKeys.Add(key))
                    continue;

                _correctAnswer = _left * _right;
                promptText.text = $"{_left} × {_right} =";
                ClearInput();
                if (feedbackText != null)
                    feedbackText.text = string.Empty;
                return;
            }

            _askedKeys.Clear();
            NextProblem();
        }

        public void Digit0() => AppendDigit('0');
        public void Digit1() => AppendDigit('1');
        public void Digit2() => AppendDigit('2');
        public void Digit3() => AppendDigit('3');
        public void Digit4() => AppendDigit('4');
        public void Digit5() => AppendDigit('5');
        public void Digit6() => AppendDigit('6');
        public void Digit7() => AppendDigit('7');
        public void Digit8() => AppendDigit('8');
        public void Digit9() => AppendDigit('9');

        public void Backspace()
        {
            if (_digits.Length == 0)
                return;
            _digits.Length--;
            RefreshAnswer();
        }

        public void ClearInput()
        {
            _digits.Clear();
            RefreshAnswer();
        }

        public void Submit()
        {
            if (_digits.Length == 0)
            {
                if (feedbackText != null)
                    feedbackText.text = "Enter a number";
                return;
            }

            if (!int.TryParse(_digits.ToString(), out int value))
            {
                if (feedbackText != null)
                    feedbackText.text = "Invalid";
                return;
            }

            if (value == _correctAnswer)
            {
                if (feedbackText != null)
                    feedbackText.text = "Correct!";
                ClearInput();
                NextProblem();
            }
            else if (feedbackText != null)
            {
                feedbackText.text = "Try again";
            }
        }

        private void AppendDigit(char c)
        {
            if (_digits.Length >= 4)
                return;
            _digits.Append(c);
            RefreshAnswer();
            if (feedbackText != null)
                feedbackText.text = string.Empty;
        }

        private void RefreshAnswer()
        {
            if (answerText == null)
                return;
            answerText.text = _digits.Length > 0 ? _digits.ToString() : string.Empty;
        }
    }
}
