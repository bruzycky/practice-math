using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PracticeMath.Analytics
{
    /// <summary>Serializable root for JSON persistence (local file only).</summary>
    [Serializable]
    public sealed class PracticeStatsPersistedData
    {
        public int fileVersion = 2;

        public long lifetimeProblemsSolved;
        public long lifetimeSubmissions;
        public long lifetimeCorrectSubmissions;
        public long lifetimeIncorrectSubmissions;

        public float lifetimeActiveSeconds;
        public float lifetimeIdleSeconds;

        /// <summary>Seconds spent with each grade selected (1→index 0), lifetime checkpointed.</summary>
        public float[] lifetimeGradeSeconds = new float[4];

        public string lastProblemSolvedDateUtc = string.Empty;
        public int currentDailyStreak;
        public int bestDailyStreak;

        public string todayUtcKey = string.Empty;
        public int todayProblemsSolved;
        public float todayActiveSeconds;

        public int dailyGoalProblems = 10;
        public int dailyGoalMinutes = 15;

        public int[] weekdaySubmissionCount = new int[7];
        public int[] hourSubmissionCount = new int[24];

        public List<SolveTimeEntry> solveTimeEntries = new List<SolveTimeEntry>();

        public long abSubmissionsA;
        public long abCorrectA;
        public long abSubmissionsB;
        public long abCorrectB;

        /// <summary>Semicolon-separated UTC calendar dates (yyyy-MM-dd) when a session was opened.</summary>
        public string sessionDaysMerged = string.Empty;

        public int lastWrongAnswer;
        public int hasLastWrong;

        public static PracticeStatsPersistedData CreateDefault()
        {
            return new PracticeStatsPersistedData
            {
                lifetimeGradeSeconds = new float[4],
                weekdaySubmissionCount = new int[7],
                hourSubmissionCount = new int[24],
                solveTimeEntries = new List<SolveTimeEntry>()
            };
        }
    }

    [Serializable]
    public sealed class SolveTimeEntry
    {
        public int gradeIndex;
        public int operation;
        public int elapsedMs;
    }

    /// <summary>Loads/saves <see cref="PracticeStatsPersistedData"/> under <see cref="Application.persistentDataPath"/>.</summary>
    public static class PracticeStatsFileStore
    {
        private const string FileName = "practice_math_stats.json";

        public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static PracticeStatsPersistedData LoadOrCreate()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return PracticeStatsPersistedData.CreateDefault();

                string json = File.ReadAllText(FilePath);
                if (string.IsNullOrWhiteSpace(json))
                    return PracticeStatsPersistedData.CreateDefault();

                var data = JsonUtility.FromJson<PracticeStatsPersistedData>(json);
                if (data == null)
                    return PracticeStatsPersistedData.CreateDefault();

                if (data.lifetimeGradeSeconds == null || data.lifetimeGradeSeconds.Length != 4)
                    data.lifetimeGradeSeconds = new float[4];
                if (data.weekdaySubmissionCount == null || data.weekdaySubmissionCount.Length != 7)
                    data.weekdaySubmissionCount = new int[7];
                if (data.hourSubmissionCount == null || data.hourSubmissionCount.Length != 24)
                    data.hourSubmissionCount = new int[24];
                data.solveTimeEntries ??= new List<SolveTimeEntry>();
                if (data.sessionDaysMerged == null)
                    data.sessionDaysMerged = string.Empty;
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Practice stats load failed: {e.Message}");
                return PracticeStatsPersistedData.CreateDefault();
            }
        }

        public static void Save(PracticeStatsPersistedData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Practice stats save failed: {e.Message}");
            }
        }
    }
}
