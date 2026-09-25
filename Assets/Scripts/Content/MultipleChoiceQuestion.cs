using System;
using PracticeMath.Core;
using UnityEngine;

namespace PracticeMath.Content
{
    [Serializable]
    public struct MultipleChoiceQuestion
    {
        public string Prompt;
        public string[] Options;
        public int CorrectIndex;
        public GradeLevel MinGrade;
        public GradeLevel MaxGrade;

        public bool MatchesGrade(GradeLevel grade)
        {
            return grade >= MinGrade && grade <= MaxGrade;
        }

        /// <summary>Copies the question with answer choices in a random order (A–D).</summary>
        public MultipleChoiceQuestion WithShuffledOptions()
        {
            if (Options == null || Options.Length <= 1)
                return this;

            var shuffled = (string[])Options.Clone();
            int correct = CorrectIndex;

            for (int i = shuffled.Length - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
                if (correct == i)
                    correct = j;
                else if (correct == j)
                    correct = i;
            }

            return new MultipleChoiceQuestion
            {
                Prompt = Prompt,
                Options = shuffled,
                CorrectIndex = correct,
                MinGrade = MinGrade,
                MaxGrade = MaxGrade
            };
        }
    }
}
