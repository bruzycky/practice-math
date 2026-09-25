using System.Collections.Generic;
using PracticeMath.Content;
using PracticeMath.Core;
using PracticeMath.Navigation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PracticeMath.UI
{
    /// <summary>Multiple-choice strand activity. Wire UI on the MultipleChoice scene Canvas.</summary>
    public sealed class MultipleChoiceActivityController : MonoBehaviour
    {
        private const int QuestionsPerSession = 10;

        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private Button[] optionButtons;
        [SerializeField] private Button nextButton;

        private readonly List<MultipleChoiceQuestion> _pool = new List<MultipleChoiceQuestion>();
        private readonly List<MultipleChoiceQuestion> _sessionQueue = new List<MultipleChoiceQuestion>();

        private int _questionIndex;
        private MultipleChoiceQuestion _current;
        private bool _sessionComplete;

        private void Start()
        {
            if (nextButton != null)
                nextButton.onClick.AddListener(OnNextPressed);

            if (optionButtons != null)
            {
                for (int i = 0; i < optionButtons.Length; i++)
                {
                    if (optionButtons[i] == null)
                        continue;
                    int index = i;
                    optionButtons[i].onClick.AddListener(() => OnOptionSelected(index));
                }
            }

            StartSession();
        }

        private void OnDestroy()
        {
            if (nextButton != null)
                nextButton.onClick.RemoveListener(OnNextPressed);
        }

        private void StartSession()
        {
            var ctx = AppSessionContext.Instance;
            var module = ctx != null ? ctx.ActiveModule : LearningModule.Geometry;
            GradeLevel grade = ctx != null ? ctx.SelectedGrade : GradeLevel.Grade3;

            if (titleText != null)
                titleText.text = TitleForModule(module);

            _pool.Clear();
            foreach (var q in BuiltInQuestionBanks.ForModule(module))
            {
                if (q.MatchesGrade(grade))
                    _pool.Add(q);
            }

            if (_pool.Count == 0)
            {
                foreach (var q in BuiltInQuestionBanks.ForModule(module))
                    _pool.Add(q);
            }

            _sessionQueue.Clear();
            var shuffled = new List<MultipleChoiceQuestion>(_pool);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            int count = Mathf.Min(QuestionsPerSession, shuffled.Count);
            for (int i = 0; i < count; i++)
                _sessionQueue.Add(shuffled[i]);

            _questionIndex = 0;
            _sessionComplete = false;
            if (feedbackText != null)
                feedbackText.text = string.Empty;
            ShowCurrentQuestion();
        }

        private static string TitleForModule(LearningModule module)
        {
            switch (module)
            {
                case LearningModule.Geometry: return "Shapes & Space";
                case LearningModule.Patterns: return "Patterns";
                case LearningModule.Money: return "Money (Canada)";
                case LearningModule.Data: return "Charts & Data";
                default: return "Practice";
            }
        }

        private void ShowCurrentQuestion()
        {
            if (_questionIndex >= _sessionQueue.Count)
            {
                _sessionComplete = true;
                if (promptText != null)
                    promptText.text = "Session complete!";
                if (statusText != null)
                    statusText.text = $"You finished {_sessionQueue.Count} questions.";
                SetOptionsVisible(false);
                return;
            }

            _current = _sessionQueue[_questionIndex].WithShuffledOptions();
            if (promptText != null)
                promptText.text = _current.Prompt;
            if (statusText != null)
                statusText.text = $"Question {_questionIndex + 1} of {_sessionQueue.Count}";
            if (feedbackText != null)
                feedbackText.text = string.Empty;
            SetOptionsVisible(true);

            if (optionButtons == null)
                return;

            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionButtons[i] == null)
                    continue;
                bool show = _current.Options != null && i < _current.Options.Length;
                optionButtons[i].gameObject.SetActive(show);
                if (!show)
                    continue;
                var label = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.text = _current.Options[i];
                optionButtons[i].interactable = true;
            }
        }

        private void SetOptionsVisible(bool visible)
        {
            if (optionButtons == null)
                return;
            foreach (var btn in optionButtons)
            {
                if (btn != null)
                    btn.gameObject.SetActive(visible);
            }
        }

        private void OnOptionSelected(int index)
        {
            if (_sessionComplete)
                return;

            if (_current.Options == null || index < 0 || index >= _current.Options.Length)
                return;

            bool correct = index == _current.CorrectIndex;
            if (feedbackText != null)
            {
                feedbackText.text = correct
                    ? "Correct!"
                    : $"Not quite. Answer: {_current.Options[_current.CorrectIndex]}";
            }

            if (optionButtons == null)
                return;
            foreach (var btn in optionButtons)
            {
                if (btn != null)
                    btn.interactable = false;
            }
        }

        private void OnNextPressed()
        {
            if (_sessionComplete)
            {
                StartSession();
                return;
            }

            _questionIndex++;
            ShowCurrentQuestion();
        }
    }
}
