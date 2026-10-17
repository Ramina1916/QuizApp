namespace QuizApp.Models.ViewModels
{
    public class AnswerSubmission
    {
        public int QuestionId { get; set; }
        public int? SelectedOptionId { get; set; }   // Nullable -> If Users don't answer to the question
        public string? UserAnswer { get; set; }
    }
}

