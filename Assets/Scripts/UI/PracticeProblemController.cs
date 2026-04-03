using System.Collections.Generic;
using PracticeMath.Core;
using TMPro;
using UnityEngine;

namespace PracticeMath.UI
{
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

        private MathProblemGenerator _generator;
        private MathProblem _current;
        private GradeLevel _gradeLevel;
        private GeneratorSettings _settings;

        /// <summary>Currently selected grade (drives + − × ÷ ranges).</summary>
        public GradeLevel CurrentGrade => _gradeLevel;

        private void Awake()
        {
            _generator = new MathProblemGenerator();
            _gradeLevel = initialGrade;
            ApplyGradeSettings();
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
            ApplyGradeSettings();
            ShowNewProblem();
        }

        private void ApplyGradeSettings()
        {
            _settings = GradeCurriculumSettings.ForGrade(_gradeLevel);
        }

        /// <summary>Shows a new random problem and updates the prompt text.</summary>
        public void ShowNewProblem()
        {
            _current = _generator.Next(_settings);
            if (promptText != null)
                promptText.text = _current.Prompt;
        }

        /// <summary>The problem currently shown (after the last <see cref="ShowNewProblem"/>).</summary>
        public MathProblem CurrentProblem => _current;
    }
}
