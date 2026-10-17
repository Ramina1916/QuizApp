namespace QuizApp.Models.ViewModels
{
    public class QuizDetailsViewModel
    {
        public int Id { get; set;  }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DifficultyLevel Difficulty { set; get; }
        public int TimeLimitMinutes { get; set; }
        public int QuestionCount {  get; set; }

        // A list of Questions
        public List<QuestionListItemViewModel> Questions { get; set; } = new();

        // Tells the View if the current user is allowed to manage this quiz
        public bool IsOwner { get; set; }

    }
}
