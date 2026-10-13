using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApp.Models
{
    public class Quiz
    {
        /*Represents a quiz that users can take.
        Acts as the parent container for Questions and tracks overall metadata.*/


       [Key]public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Unspecified;

        public int TimeLimitMinutes { get; set; } = 10;

        public QuizStatus Status { get; set; } = QuizStatus.Draft;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // The admin who created this quiz, Foreign key to UserApplication
        public string CreatedByUserId { get; set; } = string.Empty;

        // Navigation property
        [ForeignKey(nameof(CreatedByUserId))]
        public virtual ApplicationUser? CreatedBy { get; set; } // marked as virtual, because too many kinds of users can create a Quiz

        // A quiz contains many questions
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

        // A quiz can be attempted many times by many users
        public virtual ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
    }
}