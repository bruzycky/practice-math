using System.Collections;
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

        [Tooltip("Shown at random when the answer is correct. Edit in the Inspector or leave defaults.")]
        [SerializeField] private string[] positiveFeedbackMessages =
        {
            "Good!",
            "Nice!",
            "Way to go!",
            "Awesome!",
            "Great job!",
            "You got it!",
        };

        [Header("Wrong answer")]
        [Tooltip("After \"Try again\", wait this long then clear the typed answer (if the student has not edited it).")]
        [SerializeField] private float wrongAnswerClearDelaySeconds = 1f;

        [Header("Events (optional)")]
        [SerializeField] private UnityEvent onAnswerCorrect;
        [SerializeField] private UnityEvent onAnswerIncorrect;

        private readonly StringBuilder _digits = new StringBuilder();
        private Coroutine _wrongAnswerClearRoutine;

        private void OnEnable()
        {
            ClearInput();
        }

        private void OnDisable()
        {
            StopWrongAnswerClearRoutine();
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
            StopWrongAnswerClearRoutine();
            ClearDigitsOnly();
            ClearFeedback();
        }

        /// <summary>Clears the typed answer only; does not change feedback text.</summary>
        private void ClearDigitsOnly()
        {
            _digits.Clear();
            RefreshDisplay();
        }

        /// <summary>Remove the last digit (wire a Backspace button).</summary>
        public void Backspace()
        {
            if (_digits.Length <= 0)
                return;
            StopWrongAnswerClearRoutine();
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
                StopWrongAnswerClearRoutine();
                string praise = GetRandomPositiveFeedback();
                SetFeedback(praise);
                onAnswerCorrect?.Invoke();
                ClearDigitsOnly();
                practice.ShowNewProblem();
            }
            else
            {
                SetFeedback("Try again");
                onAnswerIncorrect?.Invoke();
                StopWrongAnswerClearRoutine();
                _wrongAnswerClearRoutine = StartCoroutine(ClearWrongAnswerAfterDelay());
            }
        }

        private IEnumerator ClearWrongAnswerAfterDelay()
        {
            yield return new WaitForSeconds(wrongAnswerClearDelaySeconds);
            _wrongAnswerClearRoutine = null;
            ClearDigitsOnly();
        }

        private void StopWrongAnswerClearRoutine()
        {
            if (_wrongAnswerClearRoutine == null)
                return;
            StopCoroutine(_wrongAnswerClearRoutine);
            _wrongAnswerClearRoutine = null;
        }

        private void AppendDigit(char c)
        {
            if (_digits.Length >= maxDigits)
                return;
            StopWrongAnswerClearRoutine();
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

        private string GetRandomPositiveFeedback()
        {
            if (positiveFeedbackMessages == null || positiveFeedbackMessages.Length == 0)
                return "Good!";
            int i = Random.Range(0, positiveFeedbackMessages.Length);
            string msg = positiveFeedbackMessages[i];
            return string.IsNullOrWhiteSpace(msg) ? "Good!" : msg;
        }
    }
}
