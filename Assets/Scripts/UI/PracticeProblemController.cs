using PracticeMath.Core;
using TMPro;
using UnityEngine;

namespace PracticeMath.UI
{
    /// <summary>
    /// Attach to a GameObject in your practice scene. Shows generated problems on a TMP label.
    /// Do not add <see cref="MathProblemGenerator"/> as a component — use this script instead.
    /// </summary>
    public sealed class PracticeProblemController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private GeneratorSettings settings = GeneratorSettings.DefaultEarlyElementary();

        private MathProblemGenerator _generator;
        private MathProblem _current;

        private void Awake()
        {
            _generator = new MathProblemGenerator();
        }

        private void Start()
        {
            ShowNewProblem();
        }

        /// <summary>Shows a new random problem and updates the prompt text.</summary>
        public void ShowNewProblem()
        {
            _current = _generator.Next(settings);
            if (promptText != null)
                promptText.text = _current.Prompt;
        }

        /// <summary>The problem currently shown (after the last <see cref="ShowNewProblem"/>).</summary>
        public MathProblem CurrentProblem => _current;
    }
}
