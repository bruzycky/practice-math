namespace PracticeMath.Core
{
    public static class LearningModuleDisplay
    {
        public static int ModuleCount => 8;

        public static string GetTitle(LearningModule module)
        {
            switch (module)
            {
                case LearningModule.Practice:
                    return "Practice (+−×÷)";
                case LearningModule.Quiz:
                    return "10-Question Quiz";
                case LearningModule.TimesTables:
                    return "Times Tables Quiz (0–12)";
                case LearningModule.TimesTableGrid:
                    return "Times Table Chart";
                case LearningModule.Geometry:
                    return "Shapes & Space";
                case LearningModule.Patterns:
                    return "Patterns";
                case LearningModule.Money:
                    return "Money (Canada)";
                case LearningModule.Data:
                    return "Charts & Data";
                default:
                    return module.ToString();
            }
        }

        public static bool TracksAnswerChecks(LearningModule module)
        {
            return module is LearningModule.Practice
                or LearningModule.Quiz
                or LearningModule.TimesTables
                or LearningModule.Geometry
                or LearningModule.Patterns
                or LearningModule.Money
                or LearningModule.Data;
        }

        public static bool TracksExplorations(LearningModule module) =>
            module == LearningModule.TimesTableGrid;
    }
}
