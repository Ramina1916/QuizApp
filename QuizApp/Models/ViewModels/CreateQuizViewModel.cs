using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models.ViewModels
{
    public class CreateQuizViewModel // → An Input View model with Validation
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a difficulty level.")]
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Unspecified;

        [Required(ErrorMessage = "Time limit is required.")]
        [Range(1, 120, ErrorMessage = "Time limit must be between 1 and 120 minutes.")]
        public int TimeLimitMinutes { get; set; } = 15;
    }
}
