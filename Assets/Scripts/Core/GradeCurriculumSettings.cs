namespace PracticeMath.Core
{
    /// <summary>
    /// Maps a grade to <see cref="GeneratorSettings"/> for + − × ÷ (Ontario 2020–style ranges).
    /// Fractions / algebra / coding are out of scope here.
    /// </summary>
    public static class GradeCurriculumSettings
    {
        /// <summary>Addition, subtraction, multiplication, and division bounds for the given grade.</summary>
        public static GeneratorSettings ForGrade(GradeLevel grade)
        {
            switch (grade)
            {
                case GradeLevel.Grade1:
                    // Sums to 10-style: operands 0–5; subtraction related facts to 10; small × and ÷
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
                    // Facts to 20; building multiplication/division through 10×10 representations
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
                    // Recall ×2, ×5, ×10 and related division; mental +/- within 1000 — use 0–100 operands for prompts
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
                    // Larger whole numbers; full multiplication facts to 10×10 and related division
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
                    throw new System.ArgumentOutOfRangeException(nameof(grade), grade, null);
            }
        }
    }
}
