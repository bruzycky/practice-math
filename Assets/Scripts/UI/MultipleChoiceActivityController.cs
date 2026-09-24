using System.Collections.Generic;
using PracticeMath.Content;
using PracticeMath.Core;
using PracticeMath.Navigation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PracticeMath.UI
{
    public sealed class MultipleChoiceActivityController : MonoBehaviour
    {
        private const int QuestionsPerSession = 10;

        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _promptText;
        private TextMeshProUGUI _statusText;
        private TextMeshProUGUI _feedbackText;
        private RectTransform _optionsRoot;

        private readonly List<MultipleChoiceQuestion> _pool = new List<MultipleChoiceQuestion>();
        private readonly List<MultipleChoiceQuestion> _sessionQueue = new List<MultipleChoiceQuestion>();
        private readonly List<Button> _optionButtons = new List<Button>();

        private int _questionIndex;
        private MultipleChoiceQuestion _current;
        private bool _sessionComplete;

        private void Awake()
        {
            if (!UiRuntimeFactory.TryClaimBootstrapCanvas("MultipleChoiceCanvas", out RectTransform root))
            {
                Destroy(gameObject);
                return;
            }

            UiHomeNavButton.AddTo(root);

            _titleText = UiRuntimeFactory.CreateText(root, "Title", "Activity", 44f, TextAlignmentOptions.Top);
            StretchTopBand(_titleText.rectTransform, 24f, 1000f, 70f);

            _statusText = UiRuntimeFactory.CreateText(root, "Status", string.Empty, 28f, TextAlignmentOptions.Top);
            StretchTopBand(_statusText.rectTransform, 100f, 1000f, 50f);

            _promptText = UiRuntimeFactory.CreateText(root, "Prompt", string.Empty, 36f, TextAlignmentOptions.Top);
            StretchTopBand(_promptText.rectTransform, 170f, 980f, 160f);

            _optionsRoot = new GameObject("Options", typeof(RectTransform), typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
            _optionsRoot.SetParent(root, false);
            _optionsRoot.anchorMin = new Vector2(0.5f, 0.5f);
            _optionsRoot.anchorMax = new Vector2(0.5f, 0.5f);
            _optionsRoot.pivot = new Vector2(0.5f, 0.5f);
            _optionsRoot.sizeDelta = new Vector2(900f, 420f);
            _optionsRoot.anchoredPosition = new Vector2(0f, -40f);
            var layout = _optionsRoot.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 14f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            _feedbackText = UiRuntimeFactory.CreateText(root, "Feedback", string.Empty, 30f, TextAlignmentOptions.Center);
            StretchBottomBand(_feedbackText.rectTransform, 200f, 980f, 60f);

            var nextBtn = UiRuntimeFactory.CreateButton(root, "Next question", new Vector2(320f, 72f), OnNextPressed);
            StretchBottomBand(nextBtn.GetComponent<RectTransform>(), 100f, 320f, 72f);

            BuildOptionButtons();
            StartSession();
        }

        private static void StretchTopBand(RectTransform rt, float topOffset, float width, float height)
        {
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = new Vector2(0f, -topOffset);
        }

        private static void StretchBottomBand(RectTransform rt, float bottomOffset, float width, float height)
        {
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = new Vector2(0f, bottomOffset);
        }

        private void BuildOptionButtons()
        {
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                var btn = UiRuntimeFactory.CreateButton(_optionsRoot, "Option", new Vector2(900f, 80f), () => OnOptionSelected(index));
                var layoutElement = btn.gameObject.GetComponent<LayoutElement>();
                if (layoutElement == null)
                    layoutElement = btn.gameObject.AddComponent<LayoutElement>();
                layoutElement.minHeight = 80f;
                _optionButtons.Add(btn);
            }
        }

        private void StartSession()
        {
            var ctx = AppSessionContext.Instance;
            var module = ctx != null ? ctx.ActiveModule : LearningModule.Geometry;
            GradeLevel grade = ctx != null ? ctx.SelectedGrade : GradeLevel.Grade3;

            _titleText.text = TitleForModule(module);
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
            _feedbackText.text = string.Empty;
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
                _promptText.text = "Session complete!";
                _statusText.text = $"You finished {_sessionQueue.Count} questions.";
                SetOptionsVisible(false);
                return;
            }

            _current = _sessionQueue[_questionIndex];
            _promptText.text = _current.Prompt;
            _statusText.text = $"Question {_questionIndex + 1} of {_sessionQueue.Count}";
            _feedbackText.text = string.Empty;
            SetOptionsVisible(true);

            for (int i = 0; i < _optionButtons.Count; i++)
            {
                bool show = _current.Options != null && i < _current.Options.Length;
                _optionButtons[i].gameObject.SetActive(show);
                if (!show)
                    continue;
                var label = _optionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.text = _current.Options[i];
                _optionButtons[i].interactable = true;
            }
        }

        private void SetOptionsVisible(bool visible)
        {
            foreach (var btn in _optionButtons)
                btn.gameObject.SetActive(visible);
        }

        private void OnOptionSelected(int index)
        {
            if (_sessionComplete)
                return;

            if (_current.Options == null || index < 0 || index >= _current.Options.Length)
                return;

            bool correct = index == _current.CorrectIndex;
            _feedbackText.text = correct
                ? "Correct!"
                : $"Not quite. Answer: {_current.Options[_current.CorrectIndex]}";

            foreach (var btn in _optionButtons)
                btn.interactable = false;
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
