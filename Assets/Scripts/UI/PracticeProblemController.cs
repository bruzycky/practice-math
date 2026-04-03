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
        [SerializeField] private TextMeshProUGUI promptText;
        [Tooltip("If set, options are filled with Grade 1–4 and changes update problem difficulty.")]
        [SerializeField] private TMP_Dropdown gradeDropdown;
        [SerializeField] private GradeLevel initialGrade = GradeLevel.Grade1;
        [Tooltip("Optional session stats; notified when each new question is shown.")]
        [SerializeField] private PracticeSessionAnalytics sessionAnalytics;
        [Header("A/B experiment")]
        [SerializeField] private AbExperimentMode abExperimentMode = AbExperimentMode.Off;

        private MathProblemGenerator _generator;
        private MathProblem _current;
        private GradeLevel _gradeLevel;
        private GeneratorSettings _settings;
        private bool _useVariantB;

        /// <summary>Currently selected grade (drives + − × ÷ ranges).</summary>
        public GradeLevel CurrentGrade => _gradeLevel;

        /// <summary>Whether the current on-screen problem was drawn from preset B (slightly harder caps).</summary>
        public bool CurrentProblemUsesVariantB => _useVariantB;

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
            _current = _generator.Next(_settings);
            if (promptText != null)
                promptText.text = _current.Prompt;
            sessionAnalytics?.NotifyNewProblem();
        }

        /// <summary>The problem currently shown (after the last <see cref="ShowNewProblem"/>).</summary>
        public MathProblem CurrentProblem => _current;
    }
}
