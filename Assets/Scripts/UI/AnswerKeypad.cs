using System.Text;
using PracticeMath.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace PracticeMath.UI
{
    /// <summary>
    /// Wire ten UI Buttons (0–9) to <see cref="Digit0"/> … <see cref="Digit9"/>.
    /// Optional: Clear, Backspace, Submit. Assign <see cref="practice"/> and an answer label.
    /// </summary>
    public sealed class AnswerKeypad : MonoBehaviour
    {
        [SerializeField] private PracticeProblemController practice;
        [SerializeField] private TextMeshProUGUI answerDisplay;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [Tooltip("Maximum digits the learner can enter (avoids runaway strings).")]
        [SerializeField] private int maxDigits = 8;

        [Header("Events (optional)")]
        [SerializeField] private UnityEvent onAnswerCorrect;
        [SerializeField] private UnityEvent onAnswerIncorrect;

        private readonly StringBuilder _digits = new StringBuilder();

        private void OnEnable()
        {
            ClearInput();
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

        /// <summary>Clear all entered digits (wire a Clear button).</summary>
        public void ClearInput()
        {
            _digits.Clear();
            RefreshDisplay();
            ClearFeedback();
        }

        /// <summary>Remove the last digit (wire a Backspace button).</summary>
        public void Backspace()
        {
            if (_digits.Length <= 0)
                return;
            _digits.Length--;
            RefreshDisplay();
            ClearFeedback();
        }

        /// <summary>Check the typed value against the current problem (wire a Check / Enter button).</summary>
        public void SubmitAnswer()
        {
            if (practice == null)
                return;

            if (_digits.Length == 0)
            {
                SetFeedback("Enter a number");
                return;
            }

            if (!int.TryParse(_digits.ToString(), out int value))
            {
                SetFeedback("Invalid");
                return;
            }

            MathProblem problem = practice.CurrentProblem;
            if (value == problem.CorrectAnswer)
            {
                SetFeedback("Correct!");
                onAnswerCorrect?.Invoke();
                ClearInput();
                practice.ShowNewProblem();
            }
            else
            {
                SetFeedback("Try again");
                onAnswerIncorrect?.Invoke();
            }
        }

        private void AppendDigit(char c)
        {
            if (_digits.Length >= maxDigits)
                return;
            _digits.Append(c);
            RefreshDisplay();
            ClearFeedback();
        }

        private void RefreshDisplay()
        {
            if (answerDisplay == null)
                return;
            answerDisplay.text = _digits.Length > 0 ? _digits.ToString() : string.Empty;
        }

        private void SetFeedback(string message)
        {
            if (feedbackText != null)
                feedbackText.text = message;
        }

        private void ClearFeedback()
        {
            if (feedbackText != null)
                feedbackText.text = string.Empty;
        }
    }
}
