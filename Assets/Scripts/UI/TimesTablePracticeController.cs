using System.Collections.Generic;
using System.Text;
using PracticeMath.Navigation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PracticeMath.UI
{
    /// <summary>Full 0–12 times table drill with optional single-table focus.</summary>
    public sealed class TimesTablePracticeController : MonoBehaviour
    {
        private const int MinFactor = 0;
        private const int MaxFactor = 12;

        private TMP_Dropdown _tableDropdown;
        private TextMeshProUGUI _promptText;
        private TextMeshProUGUI _answerText;
        private TextMeshProUGUI _feedbackText;
        private readonly StringBuilder _digits = new StringBuilder();
        private readonly HashSet<string> _askedKeys = new HashSet<string>();

        private int _left;
        private int _right;
        private int _correctAnswer;

        private void Awake()
        {
            if (!UiRuntimeFactory.TryClaimBootstrapCanvas("TimesTablesCanvas", out RectTransform root))
            {
                Destroy(gameObject);
                return;
            }

            UiHomeNavButton.AddTo(root);

            var title = UiRuntimeFactory.CreateText(root, "Title", "Times Tables (0–12)", 48f, TextAlignmentOptions.Top);
            StretchTopBand(title.rectTransform, 0f, 80f, 70f);

            _tableDropdown = CreateTableDropdown(root);
            _tableDropdown.onValueChanged.AddListener(_ => { _askedKeys.Clear(); NextProblem(); });

            _promptText = UiRuntimeFactory.CreateText(root, "Prompt", "?", 64f, TextAlignmentOptions.Center);
            StretchTopBand(_promptText.rectTransform, 320f, 980f, 120f);

            _answerText = UiRuntimeFactory.CreateText(root, "Answer", string.Empty, 52f, TextAlignmentOptions.Center);
            StretchTopBand(_answerText.rectTransform, 460f, 600f, 80f);

            _feedbackText = UiRuntimeFactory.CreateText(root, "Feedback", string.Empty, 32f, TextAlignmentOptions.Center);
            StretchTopBand(_feedbackText.rectTransform, 560f, 900f, 60f);

            BuildKeypad(root);
            NextProblem();
        }

        private void OnDestroy()
        {
            if (_tableDropdown != null)
                _tableDropdown.onValueChanged.RemoveAllListeners();
        }

        private TMP_Dropdown CreateTableDropdown(RectTransform root)
        {
            var dropdown = UiRuntimeFactory.CreateGradeDropdown(root, 0);
            dropdown.options.Clear();
            dropdown.options.Add(new TMP_Dropdown.OptionData("Mixed (all tables)"));
            for (int i = MinFactor; i <= MaxFactor; i++)
                dropdown.options.Add(new TMP_Dropdown.OptionData($"Table ×{i}"));
            dropdown.value = 0;
            dropdown.RefreshShownValue();
            var rt = dropdown.GetComponent<RectTransform>();
            StretchTopBand(rt, 160f, 420f, 70f);
            return dropdown;
        }

        private static void StretchTopBand(RectTransform rt, float topOffset, float width, float height)
        {
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = new Vector2(0f, -topOffset);
        }

        private void BuildKeypad(RectTransform root)
        {
            var panel = new GameObject("Keypad", typeof(RectTransform), typeof(GridLayoutGroup));
            panel.transform.SetParent(root, false);
            var panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0f);
            panelRt.anchorMax = new Vector2(0.5f, 0f);
            panelRt.pivot = new Vector2(0.5f, 0f);
            panelRt.sizeDelta = new Vector2(520f, 420f);
            panelRt.anchoredPosition = new Vector2(0f, 120f);
            var grid = panel.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(150f, 90f);
            grid.spacing = new Vector2(12f, 12f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            for (int d = 1; d <= 9; d++)
            {
                int digit = d;
                UiRuntimeFactory.CreateButton(panelRt, digit.ToString(), new Vector2(150f, 90f), () => AppendDigit((char)('0' + digit)));
            }

            UiRuntimeFactory.CreateButton(panelRt, "Clear", new Vector2(150f, 90f), ClearInput);
            UiRuntimeFactory.CreateButton(panelRt, "0", new Vector2(150f, 90f), () => AppendDigit('0'));
            UiRuntimeFactory.CreateButton(panelRt, "⌫", new Vector2(150f, 90f), Backspace);
            UiRuntimeFactory.CreateButton(panelRt, "Check", new Vector2(150f, 90f), Submit);
        }

        private void NextProblem()
        {
            int tableFocus = _tableDropdown != null && _tableDropdown.value > 0
                ? _tableDropdown.value - 1
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
                _promptText.text = $"{_left} × {_right} =";
                ClearInput();
                _feedbackText.text = string.Empty;
                return;
            }

            _askedKeys.Clear();
            NextProblem();
        }

        private void AppendDigit(char c)
        {
            if (_digits.Length >= 4)
                return;
            _digits.Append(c);
            RefreshAnswer();
            _feedbackText.text = string.Empty;
        }

        private void Backspace()
        {
            if (_digits.Length == 0)
                return;
            _digits.Length--;
            RefreshAnswer();
        }

        private void ClearInput()
        {
            _digits.Clear();
            RefreshAnswer();
        }

        private void RefreshAnswer()
        {
            _answerText.text = _digits.Length > 0 ? _digits.ToString() : string.Empty;
        }

        private void Submit()
        {
            if (_digits.Length == 0)
            {
                _feedbackText.text = "Enter a number";
                return;
            }

            if (!int.TryParse(_digits.ToString(), out int value))
            {
                _feedbackText.text = "Invalid";
                return;
            }

            if (value == _correctAnswer)
            {
                _feedbackText.text = "Correct!";
                ClearInput();
                NextProblem();
            }
            else
            {
                _feedbackText.text = "Try again";
            }
        }
    }
}
