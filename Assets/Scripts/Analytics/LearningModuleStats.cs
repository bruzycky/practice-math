using PracticeMath.Core;

namespace PracticeMath.Analytics
{
    internal static class LearningModuleStats
    {
        public const int ModuleSlotCount = 8;

        public static int ToIndex(LearningModule module)
        {
            int i = (int)module;
            return i >= 0 && i < ModuleSlotCount ? i : -1;
        }

        public static void EnsureModuleArrays(PracticeStatsPersistedData data)
        {
            if (data == null)
                return;

            if (data.lifetimeModuleVisits == null || data.lifetimeModuleVisits.Length != ModuleSlotCount)
                data.lifetimeModuleVisits = new int[ModuleSlotCount];
            if (data.lifetimeModuleSubmissions == null || data.lifetimeModuleSubmissions.Length != ModuleSlotCount)
                data.lifetimeModuleSubmissions = new int[ModuleSlotCount];
            if (data.lifetimeModuleCorrect == null || data.lifetimeModuleCorrect.Length != ModuleSlotCount)
                data.lifetimeModuleCorrect = new int[ModuleSlotCount];
        }
    }
}
