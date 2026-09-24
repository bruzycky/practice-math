using System;
using PracticeMath.Core;

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
    }
}
