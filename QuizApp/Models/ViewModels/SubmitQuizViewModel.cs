namespace QuizApp.Models.ViewModels
{
    public class SubmitQuizViewModel
    {
        public int QuizId { get; set; }
        public List<AnswerSubmission> Answers { get; set; } = new();
    }
}
