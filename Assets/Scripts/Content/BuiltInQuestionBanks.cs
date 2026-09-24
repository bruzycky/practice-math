using System.Collections.Generic;
using PracticeMath.Core;

namespace PracticeMath.Content
{
    /// <summary>Default multiple-choice pools for bootstrap scenes (no ScriptableObject required).</summary>
    public static class BuiltInQuestionBanks
    {
        public static IReadOnlyList<MultipleChoiceQuestion> ForModule(LearningModule module)
        {
            switch (module)
            {
                case LearningModule.Geometry:
                    return Geometry;
                case LearningModule.Patterns:
                    return Patterns;
                case LearningModule.Money:
                    return Money;
                case LearningModule.Data:
                    return Data;
                default:
                    return Geometry;
            }
        }

        private static readonly MultipleChoiceQuestion[] Geometry =
        {
            Q("How many sides does a triangle have?", new[] { "3", "4", "5", "6" }, 0, GradeLevel.Grade1, GradeLevel.Grade4),
            Q("How many sides does a rectangle have?", new[] { "4", "3", "5", "6" }, 0, GradeLevel.Grade1, GradeLevel.Grade4),
            Q("Which shape has 6 equal square faces?", new[] { "Cube", "Sphere", "Cone", "Cylinder" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
            Q("A square has how many corners (vertices)?", new[] { "4", "3", "5", "8" }, 0, GradeLevel.Grade1, GradeLevel.Grade4),
            Q("Which is a 3D object?", new[] { "Sphere", "Circle", "Square", "Triangle" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
            Q("Perimeter means…", new[] { "Distance around a shape", "Space inside a shape", "Height of a shape", "Weight of a shape" }, 0, GradeLevel.Grade3, GradeLevel.Grade4),
            Q("A rectangle 5 cm long and 2 cm wide has perimeter…", new[] { "14 cm", "10 cm", "7 cm", "12 cm" }, 0, GradeLevel.Grade3, GradeLevel.Grade4),
            Q("Which tool is best to measure length in centimetres?", new[] { "Ruler", "Scale for mass", "Clock", "Thermometer" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
            Q("How many edges does a cube have?", new[] { "12", "6", "8", "4" }, 0, GradeLevel.Grade3, GradeLevel.Grade4),
            Q("Two lines that meet at a right angle are…", new[] { "Perpendicular", "Parallel", "Curved", "Equal" }, 0, GradeLevel.Grade3, GradeLevel.Grade4),
        };

        private static readonly MultipleChoiceQuestion[] Patterns =
        {
            Q("What comes next? 2, 4, 6, 8, __", new[] { "10", "9", "12", "7" }, 0, GradeLevel.Grade1, GradeLevel.Grade4),
            Q("What comes next? 5, 10, 15, 20, __", new[] { "25", "22", "30", "24" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
            Q("What comes next? 1, 4, 7, 10, __", new[] { "13", "12", "11", "14" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
            Q("Skip count by 10: 30, 40, 50, __", new[] { "60", "55", "70", "52" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
            Q("Pattern A, B, A, B, A, __", new[] { "B", "A", "C", "D" }, 0, GradeLevel.Grade1, GradeLevel.Grade4),
            Q("Growing pattern: 1, 3, 5, 7, __", new[] { "9", "8", "10", "6" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
            Q("What comes next? 100, 200, 300, __", new[] { "400", "350", "500", "320" }, 0, GradeLevel.Grade3, GradeLevel.Grade4),
            Q("Rule: add 4 each time. 3, 7, 11, __", new[] { "15", "14", "13", "16" }, 0, GradeLevel.Grade3, GradeLevel.Grade4),
            Q("What comes next? 25, 50, 75, __", new[] { "100", "90", "80", "95" }, 0, GradeLevel.Grade3, GradeLevel.Grade4),
            Q("Pattern ■ ● ■ ● ■ __", new[] { "●", "■", "▲", "◆" }, 0, GradeLevel.Grade1, GradeLevel.Grade3),
        };

        private static readonly MultipleChoiceQuestion[] Money =
        {
            Q("Which coin is worth 25 cents in Canada?", new[] { "Quarter", "Dime", "Nickel", "Penny" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
            Q("How much is 2 quarters?", new[] { "50¢", "25¢", "75¢", "40¢" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
            Q("Which is worth the most?", new[] { "$5 bill", "Loonie ($1)", "Quarter", "Dime" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
            Q("$1.25 + $0.75 equals…", new[] { "$2.00", "$1.50", "$2.25", "$1.95" }, 0, GradeLevel.Grade3, GradeLevel.Grade4),
            Q("You have $3.00 and spend $1.25. Change is…", new[] { "$1.75", "$1.25", "$2.25", "$1.50" }, 0, GradeLevel.Grade3, GradeLevel.Grade4),
            Q("Four dimes equal…", new[] { "40¢", "4¢", "50¢", "25¢" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
            Q("One loonie equals…", new[] { "$1.00", "25¢", "50¢", "$2.00" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
            Q("$10 − $3.45 equals…", new[] { "$6.55", "$7.45", "$6.45", "$7.55" }, 0, GradeLevel.Grade3, GradeLevel.Grade4),
            Q("A toonie is worth…", new[] { "$2", "$1", "25¢", "50¢" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
            Q("Which amount is greater?", new[] { "$4.50", "$3.99", "$4.05", "$4.25" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
        };

        private static readonly MultipleChoiceQuestion[] Data =
        {
            Q("On a pictograph, each ★ = 2 books. Three stars mean…", new[] { "6 books", "3 books", "5 books", "2 books" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
            Q("Which graph uses bars to compare amounts?", new[] { "Bar graph", "Line map", "Clock", "Ruler" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
            Q("Cats: 4, Dogs: 7. How many more dogs than cats?", new[] { "3", "4", "11", "2" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
            Q("If 5 students chose apples and 2 chose oranges, total students?", new[] { "7", "5", "3", "10" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
            Q("Which is certain?", new[] { "A dropped ball will fall down", "It will rain tomorrow", "You will roll a 6", "Ice cream will melt in the sun today" }, 0, GradeLevel.Grade3, GradeLevel.Grade4),
            Q("Which is unlikely?", new[] { "Snow in July in Ontario", "Sun rises in the morning", "2 + 2 = 4", "A week has 7 days" }, 0, GradeLevel.Grade3, GradeLevel.Grade4),
            Q("Tally |||| means how many?", new[] { "5", "4", "6", "3" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
            Q("Red 8, Blue 5. Together they are…", new[] { "13", "12", "3", "10" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
            Q("The middle value when data is in order is called…", new[] { "Median", "Perimeter", "Product", "Quotient" }, 0, GradeLevel.Grade4, GradeLevel.Grade4),
            Q("Which question can a graph answer?", new[] { "Which category has the most?", "What is 6 × 7?", "How many sides in a hexagon?", "What is 1/2 + 1/4?" }, 0, GradeLevel.Grade2, GradeLevel.Grade4),
        };

        private static MultipleChoiceQuestion Q(string prompt, string[] options, int correct, GradeLevel min, GradeLevel max)
        {
            return new MultipleChoiceQuestion
            {
                Prompt = prompt,
                Options = options,
                CorrectIndex = correct,
                MinGrade = min,
                MaxGrade = max
            };
        }
    }
}
