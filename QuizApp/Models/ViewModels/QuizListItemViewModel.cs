using System.ComponentModel.DataAnnotations;


namespace QuizApp.Models.ViewModels // → ViewModels is for what the screen needs
{
    public class QuizListItemViewModel
    {
        // Using attributes on a read-only ViewModel are noise
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Category { get; set; } = string.Empty;

        public DifficultyLevel Difficulty { get; set; }

        public int TimeLimitMinutes { get; set; }

        public int QuestionCount { get; set; }

        public int AttemptCount {  get; set; }
    }
}
