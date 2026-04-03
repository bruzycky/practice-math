using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using PracticeMath.Core;
using UnityEngine;

namespace PracticeMath.Analytics
{
    /// <summary>
    /// Session + persisted practice stats (JSON under persistent data path). Wire from practice UI and keypad.
    /// </summary>
    public sealed class PracticeSessionAnalytics : MonoBehaviour
    {
        private const int MaxSolveSamples = 300;
        private const float IdleThresholdSeconds = 45f;
        private const int MaxSessionDayTokens = 100;

        [Header("Goals (stored in save file)")]
        [SerializeField] private int dailyGoalProblems = 10;
        [SerializeField] private int dailyGoalMinutes = 15;

        private PracticeStatsPersistedData _p;
        private float _sessionStartUnscaled;
        private float _problemStartUnscaled;
        private int _attemptsCurrentProblem;
        private int _totalSubmissions;
        private int _correctTotal;
        private int _incorrectTotal;
        private int _problemsSolved;
        private long _sumAttemptsWhenSolved;
        private int _currentStreak;
        private int _bestStreak;

        private readonly int[] _correctByGrade = new int[4];
        private readonly int[] _attemptsByGrade = new int[4];
        private readonly int[] _correctByOperation = new int[4];
        private readonly int[] _attemptsByOperation = new int[4];

        private int _activeGradeIndex;
        private float _lastSubmitUnscaled;
        private float _sessionActiveSeconds;
        private float _sessionIdleSeconds;
        private readonly float[] _sessionGradeSeconds = new float[4];

        private float _checkpointActive;
        private float _checkpointIdle;
        private readonly float[] _checkpointGrade = new float[4];

        public event Action Changed;

        public float SessionSecondsElapsed => Time.unscaledTime - _sessionStartUnscaled;
        public int ProblemsSolved => _problemsSolved;
        public int TotalSubmissions => _totalSubmissions;
        public int CorrectTotal => _correctTotal;
        public int IncorrectTotal => _incorrectTotal;
        public int AttemptsOnCurrentProblem => _attemptsCurrentProblem;
        public int CurrentStreak => _currentStreak;
        public int BestStreak => _bestStreak;
        public float SessionActiveSeconds => _sessionActiveSeconds;
        public float SessionIdleSeconds => _sessionIdleSeconds;
        public int LastWrongAnswer => _p.hasLastWrong != 0 ? _p.lastWrongAnswer : 0;
        public bool HasLastWrong => _p.hasLastWrong != 0;
        public int CurrentDailyStreak => _p.currentDailyStreak;
        public int BestDailyStreak => _p.bestDailyStreak;

        public float Accuracy =>
            _totalSubmissions > 0 ? (float)_correctTotal / _totalSubmissions : 0f;

        public float AverageAttemptsPerSolvedProblem =>
            _problemsSolved > 0 ? (float)_sumAttemptsWhenSolved / _problemsSolved : 0f;

        private void Awake()
        {
            _p = PracticeStatsFileStore.LoadOrCreate();
            _p.dailyGoalProblems = dailyGoalProblems;
            _p.dailyGoalMinutes = dailyGoalMinutes;
            _sessionStartUnscaled = Time.unscaledTime;
            _problemStartUnscaled = _sessionStartUnscaled;
            _lastSubmitUnscaled = _sessionStartUnscaled;
            RolloverTodayIfNeeded();
            RegisterSessionDayForRetention();
        }

        private void Start()
        {
            Changed?.Invoke();
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            float now = Time.unscaledTime;
            bool idle = now - _lastSubmitUnscaled > IdleThresholdSeconds;
            if (idle)
                _sessionIdleSeconds += dt;
            else
                _sessionActiveSeconds += dt;

            if (_activeGradeIndex >= 0 && _activeGradeIndex < 4)
                _sessionGradeSeconds[_activeGradeIndex] += dt;
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
                SaveToDisk();
        }

        private void OnApplicationQuit()
        {
            SaveToDisk();
        }

        private void OnDestroy()
        {
            SaveToDisk();
        }

        /// <summary>Call when the learner changes grade (dropdown).</summary>
        public void NotifyActiveGrade(GradeLevel grade)
        {
            _activeGradeIndex = (int)grade - 1;
            if (_activeGradeIndex < 0 || _activeGradeIndex > 3)
                _activeGradeIndex = 0;
        }

        public void NotifyNewProblem()
        {
            _attemptsCurrentProblem = 0;
            _problemStartUnscaled = Time.unscaledTime;
        }

        /// <param name="variantB">A/B preset for this problem (see practice controller).</param>
        public void NotifyAnswerAttempt(bool correct, GradeLevel grade, MathOperation operation, bool variantB)
        {
            _lastSubmitUnscaled = Time.unscaledTime;
            _attemptsCurrentProblem++;
            _totalSubmissions++;

            int gi = (int)grade - 1;
            if (gi >= 0 && gi < 4)
            {
                _attemptsByGrade[gi]++;
                if (correct)
                    _correctByGrade[gi]++;
            }

            int oi = (int)operation;
            if (oi >= 0 && oi < 4)
            {
                _attemptsByOperation[oi]++;
                if (correct)
                    _correctByOperation[oi]++;
            }

            RecordHeatmap();

            if (variantB)
            {
                _p.abSubmissionsB++;
                if (correct)
                    _p.abCorrectB++;
            }
            else
            {
                _p.abSubmissionsA++;
                if (correct)
                    _p.abCorrectA++;
            }

            if (correct)
            {
                _correctTotal++;
                _problemsSolved++;
                _sumAttemptsWhenSolved += _attemptsCurrentProblem;
                _currentStreak++;
                if (_currentStreak > _bestStreak)
                    _bestStreak = _currentStreak;

                float solveSec = Time.unscaledTime - _problemStartUnscaled;
                int ms = Mathf.RoundToInt(solveSec * 1000f);
                AppendSolveSample(gi, oi, ms);

                _p.lifetimeProblemsSolved++;
                _p.lifetimeCorrectSubmissions++;
                RolloverTodayIfNeeded();
                _p.todayProblemsSolved++;
                UpdateDailyStreakOnSolve();

                _p.hasLastWrong = 0;
            }
            else
            {
                _incorrectTotal++;
                _currentStreak = 0;
                _p.lifetimeIncorrectSubmissions++;
            }

            _p.lifetimeSubmissions++;
            Changed?.Invoke();
            SaveToDisk();
        }

        /// <summary>Call when the learner submits a wrong numeric answer (value they typed).</summary>
        public void NotifyWrongValueSubmitted(int wrongValue)
        {
            _p.lastWrongAnswer = wrongValue;
            _p.hasLastWrong = 1;
        }

        private void AppendSolveSample(int gradeIndex, int operation, int elapsedMs)
        {
            if (elapsedMs < 0 || elapsedMs > 600000)
                return;
            _p.solveTimeEntries.Add(new SolveTimeEntry
            {
                gradeIndex = gradeIndex,
                operation = operation,
                elapsedMs = elapsedMs
            });
            while (_p.solveTimeEntries.Count > MaxSolveSamples)
                _p.solveTimeEntries.RemoveAt(0);
        }

        private void RecordHeatmap()
        {
            var utc = DateTime.UtcNow;
            _p.weekdaySubmissionCount[(int)utc.DayOfWeek]++;
            _p.hourSubmissionCount[utc.Hour]++;
        }

        private void RolloverTodayIfNeeded()
        {
            string today = UtcTodayKey();
            if (_p.todayUtcKey != today)
            {
                _p.todayUtcKey = today;
                _p.todayProblemsSolved = 0;
                _p.todayActiveSeconds = 0f;
            }
        }

        private static string UtcTodayKey() => DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        private void UpdateDailyStreakOnSolve()
        {
            string today = UtcTodayKey();
            if (_p.lastProblemSolvedDateUtc == today)
                return;

            if (string.IsNullOrEmpty(_p.lastProblemSolvedDateUtc))
            {
                _p.currentDailyStreak = 1;
            }
            else if (DateTime.TryParse(_p.lastProblemSolvedDateUtc, CultureInfo.InvariantCulture,
                         DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime last))
            {
                DateTime lastD = last.Date;
                DateTime todayD = DateTime.UtcNow.Date;
                if (todayD == lastD.AddDays(1))
                    _p.currentDailyStreak++;
                else if (todayD > lastD.AddDays(1))
                    _p.currentDailyStreak = 1;
            }
            else
            {
                _p.currentDailyStreak = 1;
            }

            _p.lastProblemSolvedDateUtc = today;
            if (_p.currentDailyStreak > _p.bestDailyStreak)
                _p.bestDailyStreak = _p.currentDailyStreak;
        }

        private void RegisterSessionDayForRetention()
        {
            string d = UtcTodayKey();
            string merged = _p.sessionDaysMerged ?? string.Empty;
            if (merged.IndexOf(d, StringComparison.Ordinal) >= 0)
                return;
            if (string.IsNullOrEmpty(merged))
                _p.sessionDaysMerged = d;
            else
                _p.sessionDaysMerged = merged + ";" + d;

            string[] parts = _p.sessionDaysMerged.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > MaxSessionDayTokens)
            {
                int skip = parts.Length - MaxSessionDayTokens;
                _p.sessionDaysMerged = string.Join(";", parts.Skip(skip));
            }
        }

        private void CheckpointTimeIntoPersisted()
        {
            float dA = _sessionActiveSeconds - _checkpointActive;
            float dI = _sessionIdleSeconds - _checkpointIdle;
            _p.lifetimeActiveSeconds += dA;
            _p.lifetimeIdleSeconds += dI;
            _checkpointActive = _sessionActiveSeconds;
            _checkpointIdle = _sessionIdleSeconds;

            _p.todayActiveSeconds += dA;

            for (int i = 0; i < 4; i++)
            {
                float dg = _sessionGradeSeconds[i] - _checkpointGrade[i];
                _p.lifetimeGradeSeconds[i] += dg;
                _checkpointGrade[i] = _sessionGradeSeconds[i];
            }
        }

        public void SaveToDisk()
        {
            CheckpointTimeIntoPersisted();
            PracticeStatsFileStore.Save(_p);
        }

        /// <summary>Clears in-session counters only (does not delete the JSON file).</summary>
        public void ResetSession()
        {
            _sessionStartUnscaled = Time.unscaledTime;
            _problemStartUnscaled = _sessionStartUnscaled;
            _lastSubmitUnscaled = _sessionStartUnscaled;
            _attemptsCurrentProblem = 0;
            _totalSubmissions = 0;
            _correctTotal = 0;
            _incorrectTotal = 0;
            _problemsSolved = 0;
            _sumAttemptsWhenSolved = 0;
            _currentStreak = 0;
            _bestStreak = 0;
            _sessionActiveSeconds = 0;
            _sessionIdleSeconds = 0;
            Array.Clear(_sessionGradeSeconds, 0, _sessionGradeSeconds.Length);
            _checkpointActive = 0;
            _checkpointIdle = 0;
            Array.Clear(_checkpointGrade, 0, _checkpointGrade.Length);
            _lastSubmitUnscaled = Time.unscaledTime;
            Array.Clear(_correctByGrade, 0, _correctByGrade.Length);
            Array.Clear(_attemptsByGrade, 0, _attemptsByGrade.Length);
            Array.Clear(_correctByOperation, 0, _correctByOperation.Length);
            Array.Clear(_attemptsByOperation, 0, _attemptsByOperation.Length);
            Changed?.Invoke();
        }

        /// <summary>Wipes JSON and memory (use sparingly).</summary>
        public void ResetAllPersistentData()
        {
            _p = PracticeStatsPersistedData.CreateDefault();
            _p.dailyGoalProblems = dailyGoalProblems;
            _p.dailyGoalMinutes = dailyGoalMinutes;
            ResetSession();
            PracticeStatsFileStore.Save(_p);
        }

        public string GetFormattedSummary()
        {
            CheckpointTimeIntoPersisted();

            var sb = new StringBuilder(2048);
            TimeSpan t = TimeSpan.FromSeconds(SessionSecondsElapsed);
            string timeStr = t.TotalHours >= 1
                ? $"{t.Hours}:{t.Minutes:D2}:{t.Seconds:D2}"
                : $"{t.Minutes:D2}:{t.Seconds:D2}";

            sb.AppendLine("SESSION");
            sb.AppendLine($"Time: {timeStr}");
            sb.AppendLine($"Active / idle: {_sessionActiveSeconds:F0}s / {_sessionIdleSeconds:F0}s");
            sb.AppendLine($"Problems solved: {_problemsSolved}");
            sb.AppendLine($"Answer checks: {_totalSubmissions}");
            sb.AppendLine($"Correct / wrong: {_correctTotal} / {_incorrectTotal}");
            sb.AppendLine($"Accuracy: {Accuracy:P0}");
            sb.AppendLine($"Avg attempts (solved): {AverageAttemptsPerSolvedProblem:F1}");
            sb.AppendLine($"Tries this question: {_attemptsCurrentProblem}");
            sb.AppendLine($"Streak (session): {_currentStreak} (best {_bestStreak})");
            sb.AppendLine($"Streak (daily): {_p.currentDailyStreak} (best {_p.bestDailyStreak})");

            float gradeSum = 0f;
            for (int i = 0; i < 4; i++)
                gradeSum += _sessionGradeSeconds[i];
            sb.AppendLine();
            sb.AppendLine("GRADE MIX (this session)");
            if (gradeSum < 0.01f)
                sb.AppendLine("(no time yet)");
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    float pct = _sessionGradeSeconds[i] / gradeSum * 100f;
                    sb.AppendLine($"Grade {i + 1}: {pct:F0}%");
                }
            }

            sb.AppendLine();
            sb.AppendLine("TODAY (UTC)");
            sb.AppendLine($"Problems: {_p.todayProblemsSolved} / goal {_p.dailyGoalProblems}");
            float goalMinSec = _p.dailyGoalMinutes * 60f;
            sb.AppendLine($"Active (saved): {_p.todayActiveSeconds / 60f:F1} / goal {_p.dailyGoalMinutes} min");

            sb.AppendLine();
            sb.AppendLine("LIFETIME (saved file)");
            sb.AppendLine($"Problems solved: {_p.lifetimeProblemsSolved}");
            sb.AppendLine($"Checks: {_p.lifetimeSubmissions} (corr {_p.lifetimeCorrectSubmissions} / wrong {_p.lifetimeIncorrectSubmissions})");
            sb.AppendLine($"Active / idle: {_p.lifetimeActiveSeconds:F0}s / {_p.lifetimeIdleSeconds:F0}s");

            if (HasLastWrong)
                sb.AppendLine($"Last wrong answer typed: {LastWrongAnswer}");

            sb.AppendLine();
            sb.AppendLine("MEDIAN solve time (ms), saved samples");
            AppendMedianBlock(sb, -1, -1, "All");
            for (int g = 0; g < 4; g++)
                AppendMedianBlock(sb, g, -1, $"Grade {g + 1}");
            for (int o = 0; o < 4; o++)
            {
                string op = o switch { 0 => "+", 1 => "−", 2 => "×", _ => "÷" };
                AppendMedianBlock(sb, -1, o, op);
            }

            sb.AppendLine();
            sb.AppendLine("A/B (cumulative)");
            sb.AppendLine(FormatAbLine("A", _p.abCorrectA, _p.abSubmissionsA));
            sb.AppendLine(FormatAbLine("B", _p.abCorrectB, _p.abSubmissionsB));

            sb.AppendLine();
            sb.AppendLine("HEATMAP (submissions, saved)");
            sb.AppendLine("Weekday (Sun–Sat): " + string.Join(", ", _p.weekdaySubmissionCount));
            sb.AppendLine("(Hours 0–23 in file; too wide to print all — peak below)");
            int peakH = 0;
            for (int h = 1; h < 24; h++)
                if (_p.hourSubmissionCount[h] > _p.hourSubmissionCount[peakH])
                    peakH = h;
            sb.AppendLine($"Peak hour (UTC): {peakH}:00 ({_p.hourSubmissionCount[peakH]})");

            sb.AppendLine();
            sb.AppendLine("RETENTION");
            sb.AppendLine($"Distinct session days stored: {CountSessionDayTokens()}");
            sb.AppendLine($"Sessions in last 7 UTC days: {CountSessionDaysInLast(7)}");
            sb.AppendLine($"Days since last session day in file: {DaysSinceLastSessionDay()}");

            sb.AppendLine();
            sb.AppendLine("BY GRADE (session)");
            for (int i = 0; i < 4; i++)
            {
                sb.AppendLine(
                    $"Grade {i + 1}: {_correctByGrade[i]} correct / {_attemptsByGrade[i]} checks");
            }

            sb.AppendLine();
            sb.AppendLine("BY OPERATION (session)");
            sb.AppendLine($"+ {_correctByOperation[0]} / {_attemptsByOperation[0]}");
            sb.AppendLine($"− {_correctByOperation[1]} / {_attemptsByOperation[1]}");
            sb.AppendLine($"× {_correctByOperation[2]} / {_attemptsByOperation[2]}");
            sb.AppendLine($"÷ {_correctByOperation[3]} / {_attemptsByOperation[3]}");

            sb.AppendLine();
            sb.AppendLine($"Save: {PracticeStatsFileStore.FilePath}");
            return sb.ToString();
        }

        private static string FormatAbLine(string label, long correct, long total)
        {
            if (total <= 0)
                return $"{label}: —";
            float acc = correct / (float)total;
            return $"{label}: {correct}/{total} ({acc:P0})";
        }

        private void AppendMedianBlock(StringBuilder sb, int gradeFilter, int opFilter, string label)
        {
            IEnumerable<SolveTimeEntry> entries = _p.solveTimeEntries;
            if (gradeFilter >= 0)
                entries = entries.Where(e => e.gradeIndex == gradeFilter);
            if (opFilter >= 0)
                entries = entries.Where(e => e.operation == opFilter);
            List<int> ms = entries.Select(e => e.elapsedMs).OrderBy(x => x).ToList();
            if (ms.Count == 0)
            {
                sb.AppendLine($"{label}: —");
                return;
            }

            float med = MedianSorted(ms);
            sb.AppendLine($"{label}: {med:F0} ms (n={ms.Count})");
        }

        private static float MedianSorted(List<int> sortedMs)
        {
            int n = sortedMs.Count;
            if ((n & 1) == 1)
                return sortedMs[n / 2];
            return (sortedMs[n / 2 - 1] + sortedMs[n / 2]) * 0.5f;
        }

        private int CountSessionDayTokens()
        {
            if (string.IsNullOrEmpty(_p.sessionDaysMerged))
                return 0;
            return _p.sessionDaysMerged.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private int CountSessionDaysInLast(int days)
        {
            if (string.IsNullOrEmpty(_p.sessionDaysMerged))
                return 0;
            DateTime cutoff = DateTime.UtcNow.Date.AddDays(-(days - 1));
            string[] parts = _p.sessionDaysMerged.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var seen = new HashSet<string>();
            foreach (string p in parts)
            {
                if (!DateTime.TryParse(p, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime d))
                    continue;
                if (d.Date < cutoff)
                    continue;
                seen.Add(d.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }

            return seen.Count;
        }

        private int DaysSinceLastSessionDay()
        {
            if (string.IsNullOrEmpty(_p.sessionDaysMerged))
                return -1;
            string[] parts = _p.sessionDaysMerged.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            DateTime? max = null;
            foreach (string p in parts)
            {
                if (DateTime.TryParse(p, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime d))
                {
                    if (max == null || d.Date > max.Value)
                        max = d.Date;
                }
            }

            if (max == null)
                return -1;
            return (int)(DateTime.UtcNow.Date - max.Value).TotalDays;
        }
    }
}
