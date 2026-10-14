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
       
    }
}
