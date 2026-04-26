using System.Collections.Generic;
using PracticeMath.Analytics;
using PracticeMath.Core;
using TMPro;
using UnityEngine;

namespace PracticeMath.UI
{
    /// <summary>A/B preset: control vs slightly harder caps from <see cref="GradeCurriculumSettings.ForGrade"/>.</summary>
    public enum AbExperimentMode
    {
        Off = 0,
        RandomPerProblem = 1,
        ForceA = 2,
        ForceB = 3
    }

    /// <summary>
    /// Attach to a GameObject in your practice scene. Shows generated problems on a TMP label.
    /// Optionally assign a <see cref="TMP_Dropdown"/> to pick Grade 1–4; difficulty follows <see cref="GradeCurriculumSettings"/>.
    /// </summary>
    public sealed class PracticeProblemController : MonoBehaviour
    {
        private const int QuizQuestionsPerSession = 10;

        [SerializeField] private TextMeshProUGUI promptText;
        [Tooltip("If set, options are filled with Grade 1–4 and changes update problem difficulty.")]
        [SerializeField] private TMP_Dropdown gradeDropdown;
        [Tooltip("Optional text for quiz progress and final score.")]
        [SerializeField] private TextMeshProUGUI quizStatusText;
        [SerializeField] private GradeLevel initialGrade = GradeLevel.Grade1;
        [Tooltip("Optional session stats; notified when each new question is shown.")]
        [SerializeField] private PracticeSessionAnalytics sessionAnalytics;
        [Header("A/B experiment")]
        [SerializeField] private AbExperimentMode abExperimentMode = AbExperimentMode.Off;

        /// <summary>
        /// Same instance used for new-question / grade notifications.
        /// <see cref="AnswerKeypad"/> uses this when its own analytics field is not assigned.
        /// </summary>
        public PracticeSessionAnalytics SessionAnalytics => sessionAnalytics;

        private MathProblemGenerator _generator;
        private MathProblem _current;
        private GradeLevel _gradeLevel;
        private GeneratorSettings _settings;
        private bool _useVariantB;
        private bool _isQuizActive;
        private int _quizQuestionsAnswered;
        private int _quizCorrectAnswers;
        private readonly HashSet<string> _askedProblemKeys = new HashSet<string>();
        private const int MaxUniqueGenerationAttempts = 256;

        /// <summary>Currently selected grade (drives + − × ÷ ranges).</summary>
        public GradeLevel CurrentGrade => _gradeLevel;

        /// <summary>Whether the current on-screen problem was drawn from preset B (slightly harder caps).</summary>
        public bool CurrentProblemUsesVariantB => _useVariantB;
        public bool IsQuizActive => _isQuizActive;
        public bool IsQuizCompleted => !_isQuizActive && _quizQuestionsAnswered >= QuizQuestionsPerSession;
        public int QuizQuestionCount => QuizQuestionsPerSession;
        public int QuizCorrectAnswers => _quizCorrectAnswers;

        private void Awake()
        {
            _generator = new MathProblemGenerator();
            _gradeLevel = initialGrade;
        }

        private void Start()
        {
            if (gradeDropdown != null)
            {
                gradeDropdown.ClearOptions();
                gradeDropdown.AddOptions(new List<string> { "Grade 1", "Grade 2", "Grade 3", "Grade 4" });
                int idx = (int)_gradeLevel - 1;
                if (idx >= 0 && idx < gradeDropdown.options.Count)
                    gradeDropdown.SetValueWithoutNotify(idx);
                gradeDropdown.onValueChanged.AddListener(OnGradeDropdownChanged);
            }

            sessionAnalytics?.NotifyActiveGrade(_gradeLevel);
            UpdateQuizStatusText(string.Empty);
            ShowNewProblem();
        }

        private void OnDestroy()
        {
            if (gradeDropdown != null)
                gradeDropdown.onValueChanged.RemoveListener(OnGradeDropdownChanged);
        }

        private void OnGradeDropdownChanged(int index)
        {
            _gradeLevel = (GradeLevel)(index + 1);
            sessionAnalytics?.NotifyActiveGrade(_gradeLevel);
            if (_isQuizActive)
                StartQuiz();
            else
                ShowNewProblem();
        }

        /// <summary>Starts a 10-question quiz for the currently selected grade.</summary>
        public void StartQuiz()
        {
            _isQuizActive = true;
            _quizQuestionsAnswered = 0;
            _quizCorrectAnswers = 0;
            _askedProblemKeys.Clear();
            UpdateQuizStatusText($"Quiz started: Question 1 of {QuizQuestionsPerSession}");
            ShowNewProblem();
        }

        /// <summary>Cancels the active quiz and returns to normal practice mode.</summary>
        public void CancelQuiz()
        {
            _isQuizActive = false;
            _quizQuestionsAnswered = 0;
            _quizCorrectAnswers = 0;
            UpdateQuizStatusText(string.Empty);
            ShowNewProblem();
        }

        /// <summary>Advances quiz progress after one submitted answer.</summary>
        public void RecordQuizAnswer(bool isCorrect)
        {
            if (!_isQuizActive)
                return;

            if (isCorrect)
                _quizCorrectAnswers++;

            _quizQuestionsAnswered++;

            if (_quizQuestionsAnswered >= QuizQuestionsPerSession)
            {
                _isQuizActive = false;
                UpdateQuizStatusText($"Quiz complete: {_quizCorrectAnswers} out of {QuizQuestionsPerSession} correct.");
                return;
            }

            int nextQuestionNumber = _quizQuestionsAnswered + 1;
            UpdateQuizStatusText($"Quiz in progress: Question {nextQuestionNumber} of {QuizQuestionsPerSession}");
            ShowNewProblem();
        }

        private void PickVariantForNextProblem()
        {
            switch (abExperimentMode)
            {
                case AbExperimentMode.Off:
                case AbExperimentMode.ForceA:
                    _useVariantB = false;
                    break;
                case AbExperimentMode.ForceB:
                    _useVariantB = true;
                    break;
                case AbExperimentMode.RandomPerProblem:
                    _useVariantB = Random.value >= 0.5f;
                    break;
            }
        }

        /// <summary>Shows a new random problem and updates the prompt text.</summary>
        public void ShowNewProblem()
        {
            PickVariantForNextProblem();
            _settings = GradeCurriculumSettings.ForGrade(_gradeLevel, _useVariantB);
            _current = NextUnaskedProblem(_settings);
            if (promptText != null)
                promptText.text = _current.Prompt;
            sessionAnalytics?.NotifyNewProblem();
        }

        private MathProblem NextUnaskedProblem(GeneratorSettings settings)
        {
            for (int attempt = 0; attempt < MaxUniqueGenerationAttempts; attempt++)
            {
                var candidate = _generator.Next(settings);
                if (_askedProblemKeys.Add(ProblemKey(candidate)))
                    return candidate;
            }

            // If the available pool is exhausted, return a problem anyway instead of blocking.
            var fallback = _generator.Next(settings);
            _askedProblemKeys.Add(ProblemKey(fallback));
            return fallback;
        }

        private static string ProblemKey(MathProblem problem)
        {
            return problem.Operation + ":" + problem.LeftOperand + ":" + problem.RightOperand;
        }

        private void UpdateQuizStatusText(string message)
        {
            if (quizStatusText != null)
                quizStatusText.text = message;
        }

        /// <summary>The problem currently shown (after the last <see cref="ShowNewProblem"/>).</summary>
        public MathProblem CurrentProblem => _current;
    }
}
