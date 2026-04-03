using System;

namespace PracticeMath.Core
{
    /// <summary>
    /// Tunable ranges for each operation. Integer bounds are inclusive.
    /// </summary>
    [Serializable]
    public struct GeneratorSettings
    {
        public bool IncludeAddition;
        public bool IncludeSubtraction;
        public bool IncludeMultiplication;
        public bool IncludeDivision;

        public int AdditionMin;
        public int AdditionMax;

        /// <summary>Operands for subtraction; answers are never negative.</summary>
        public int SubtractionMin;
        public int SubtractionMax;

        public int MultiplicationMin;
        public int MultiplicationMax;

        /// <summary>Divisor range (e.g. 2–10). Problems are whole-number division.</summary>
        public int DivisionDivisorMin;
        public int DivisionDivisorMax;
        public int DivisionQuotientMin;
        public int DivisionQuotientMax;

        public static GeneratorSettings DefaultEarlyElementary()
        {
            return new GeneratorSettings
            {
                IncludeAddition = true,
                IncludeSubtraction = true,
                IncludeMultiplication = true,
                IncludeDivision = true,
                AdditionMin = 0,
                AdditionMax = 10,
                SubtractionMin = 0,
                SubtractionMax = 10,
                MultiplicationMin = 0,
                MultiplicationMax = 10,
                DivisionDivisorMin = 2,
                DivisionDivisorMax = 10,
                DivisionQuotientMin = 1,
                DivisionQuotientMax = 10
            };
        }
    }

    /// <summary>
    /// Produces random math problems according to <see cref="GeneratorSettings"/>.
    /// </summary>
    public sealed class MathProblemGenerator
    {
        private readonly Random _rng;

        public MathProblemGenerator(int? seed = null)
        {
            _rng = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public MathProblem Next(GeneratorSettings settings)
        {
            var op = PickOperation(settings);
            switch (op)
            {
                case MathOperation.Addition:
                    return MakeAddition(settings);
                case MathOperation.Subtraction:
                    return MakeSubtraction(settings);
                case MathOperation.Multiplication:
                    return MakeMultiplication(settings);
                case MathOperation.Division:
                    return MakeDivision(settings);
                default:
                    return MakeAddition(settings);
            }
        }

        private MathOperation PickOperation(GeneratorSettings s)
        {
            int tries = 0;
            while (tries++ < 64)
            {
                var op = (MathOperation)_rng.Next(0, 4);
                if (IsEnabled(op, s))
                    return op;
            }

            if (s.IncludeAddition) return MathOperation.Addition;
            if (s.IncludeSubtraction) return MathOperation.Subtraction;
            if (s.IncludeMultiplication) return MathOperation.Multiplication;
            return MathOperation.Division;
        }

        private static bool IsEnabled(MathOperation op, GeneratorSettings s)
        {
            switch (op)
            {
                case MathOperation.Addition: return s.IncludeAddition;
                case MathOperation.Subtraction: return s.IncludeSubtraction;
                case MathOperation.Multiplication: return s.IncludeMultiplication;
                case MathOperation.Division: return s.IncludeDivision;
                default: return false;
            }
        }

        private MathProblem MakeAddition(GeneratorSettings s)
        {
            int a = NextInclusive(s.AdditionMin, s.AdditionMax);
            int b = NextInclusive(s.AdditionMin, s.AdditionMax);
            int sum = a + b;
            return new MathProblem($"{a} + {b} = ?", sum, MathOperation.Addition, a, b);
        }

        private MathProblem MakeSubtraction(GeneratorSettings s)
        {
            int hi = NextInclusive(s.SubtractionMin, s.SubtractionMax);
            int lo = NextInclusive(s.SubtractionMin, Math.Min(hi, s.SubtractionMax));
            int a = Math.Max(hi, lo);
            int b = Math.Min(hi, lo);
            return new MathProblem($"{a} − {b} = ?", a - b, MathOperation.Subtraction, a, b);
        }

        private MathProblem MakeMultiplication(GeneratorSettings s)
        {
            int a = NextInclusive(s.MultiplicationMin, s.MultiplicationMax);
            int b = NextInclusive(s.MultiplicationMin, s.MultiplicationMax);
            int p = a * b;
            return new MathProblem($"{a} × {b} = ?", p, MathOperation.Multiplication, a, b);
        }

        private MathProblem MakeDivision(GeneratorSettings s)
        {
            int divisor = NextInclusive(s.DivisionDivisorMin, s.DivisionDivisorMax);
            if (divisor == 0)
                divisor = 1;

            int quotient = NextInclusive(s.DivisionQuotientMin, s.DivisionQuotientMax);
            int dividend = divisor * quotient;

            return new MathProblem(
                $"{dividend} ÷ {divisor} = ?",
                quotient,
                MathOperation.Division,
                dividend,
                divisor);
        }

        private int NextInclusive(int min, int max)
        {
            if (max < min)
                (min, max) = (max, min);
            return _rng.Next(min, max + 1);
        }
    }
}
