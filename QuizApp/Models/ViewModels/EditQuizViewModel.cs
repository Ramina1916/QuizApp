using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models.ViewModels
{
    public class EditQuizViewModel // It looks like CreateQuizViewModel, but it needs an Id.
    {
        public int Id { get; set; } // We need the ID to know which quiz to update

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        public DifficultyLevel Difficulty { get; set; }

        [Required]
        [Range(1, 120)]
        public int TimeLimitMinutes { get; set; }
    }
}