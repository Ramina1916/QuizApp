namespace QuizApp.Models.ViewModels
{
    public class MyQuizViewModel // implement the Creator Dashboard : a private place to see their Drafts and their Active quizzes so they can manage them.
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public QuizApp.Models.QuizStatus Status { get; set; }
        public int QuestionCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}