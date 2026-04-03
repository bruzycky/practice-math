namespace PracticeMath.Core
{
    /// <summary>
    /// A single practice question and its expected integer answer.
    /// </summary>
    public readonly struct MathProblem
    {
        public string Prompt { get; }
        public int CorrectAnswer { get; }
        public MathOperation Operation { get; }
        public int LeftOperand { get; }
        public int RightOperand { get; }

        public MathProblem(
            string prompt,
            int correctAnswer,
            MathOperation operation,
            int leftOperand,
            int rightOperand)
        {
            Prompt = prompt;
            CorrectAnswer = correctAnswer;
            Operation = operation;
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
        }
    }
}
