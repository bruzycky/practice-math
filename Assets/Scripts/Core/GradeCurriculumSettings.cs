using System;

namespace PracticeMath.Core
{
    /// <summary>
    /// Maps a grade to <see cref="GeneratorSettings"/> for + − × ÷ (Ontario 2020–style ranges).
    /// Fractions / algebra / coding are out of scope here.
    /// </summary>
    public static class GradeCurriculumSettings
    {
        /// <summary>Addition, subtraction, multiplication, and division bounds for the given grade.</summary>
        public static GeneratorSettings ForGrade(GradeLevel grade) => BuildForGrade(grade);

        /// <summary>
        /// <paramref name="variantB"/> slightly raises caps for A/B experiments (same operations).
        /// </summary>
        public static GeneratorSettings ForGrade(GradeLevel grade, bool variantB)
        {
            GeneratorSettings s = BuildForGrade(grade);
            if (!variantB)
                return s;
            return ApplyVariantB(s);
        }

        private static GeneratorSettings ApplyVariantB(GeneratorSettings s)
        {
            s.AdditionMax = Math.Min(s.AdditionMax + 2, 5000);
            s.SubtractionMax = Math.Min(s.SubtractionMax + 5, 5000);
            s.MultiplicationMax = Math.Min(s.MultiplicationMax + 2, 12);
            s.DivisionDivisorMax = Math.Min(s.DivisionDivisorMax + 2, 12);
            s.DivisionQuotientMax = Math.Min(s.DivisionQuotientMax + 2, 20);
            return s;
        }

        private static GeneratorSettings BuildForGrade(GradeLevel grade)
        {
            switch (grade)
            {
                case GradeLevel.Grade1:
                    return new GeneratorSettings
                    {
                        IncludeAddition = true,
                        IncludeSubtraction = true,
                        IncludeMultiplication = true,
                        IncludeDivision = true,
                        AdditionMin = 0,
                        AdditionMax = 5,
                        SubtractionMin = 0,
                        SubtractionMax = 10,
                        MultiplicationMin = 0,
                        MultiplicationMax = 5,
                        MultiplicationAnchorFactors = null,
                        DivisionDivisorMin = 2,
                        DivisionDivisorMax = 5,
                        DivisionQuotientMin = 1,
                        DivisionQuotientMax = 5,
                        DivisionDivisorChoices = null
                    };

                case GradeLevel.Grade2:
                    return new GeneratorSettings
                    {
                        IncludeAddition = true,
                        IncludeSubtraction = true,
                        IncludeMultiplication = true,
                        IncludeDivision = true,
                        AdditionMin = 0,
                        AdditionMax = 10,
                        SubtractionMin = 0,
                        SubtractionMax = 20,
                        MultiplicationMin = 0,
                        MultiplicationMax = 10,
                        MultiplicationAnchorFactors = null,
                        DivisionDivisorMin = 2,
                        DivisionDivisorMax = 10,
                        DivisionQuotientMin = 1,
                        DivisionQuotientMax = 10,
                        DivisionDivisorChoices = null
                    };

                case GradeLevel.Grade3:
                    return new GeneratorSettings
                    {
                        IncludeAddition = true,
                        IncludeSubtraction = true,
                        IncludeMultiplication = true,
                        IncludeDivision = true,
                        AdditionMin = 0,
                        AdditionMax = 100,
                        SubtractionMin = 0,
                        SubtractionMax = 100,
                        MultiplicationMin = 1,
                        MultiplicationMax = 10,
                        MultiplicationAnchorFactors = new[] { 2, 5, 10 },
                        DivisionDivisorMin = 2,
                        DivisionDivisorMax = 10,
                        DivisionQuotientMin = 1,
                        DivisionQuotientMax = 10,
                        DivisionDivisorChoices = new[] { 2, 5, 10 }
                    };

                case GradeLevel.Grade4:
                    return new GeneratorSettings
                    {
                        IncludeAddition = true,
                        IncludeSubtraction = true,
                        IncludeMultiplication = true,
                        IncludeDivision = true,
                        AdditionMin = 0,
                        AdditionMax = 999,
                        SubtractionMin = 0,
                        SubtractionMax = 999,
                        MultiplicationMin = 0,
                        MultiplicationMax = 10,
                        MultiplicationAnchorFactors = null,
                        DivisionDivisorMin = 2,
                        DivisionDivisorMax = 10,
                        DivisionQuotientMin = 1,
                        DivisionQuotientMax = 10,
                        DivisionDivisorChoices = null
                    };

                default:
                    throw new ArgumentOutOfRangeException(nameof(grade), grade, null);
            }
        }
    }
}
