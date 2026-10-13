using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApp.Models
{
    public class Quiz
    {
        [Key]public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        public Difficulty Difficulty { get; set; } = Difficulty.Medium;

        public int TimeLimitMinutes { get; set; } = 10;

        public QuizStatus Status { get; set; } = QuizStatus.Draft;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key to ApplicationUser
        public string CreatedByUserId { get; set; } = string.Empty;

        // Navigation property
        [ForeignKey(nameof(CreatedByUserId))]
        public virtual ApplicationUser? CreatedBy { get; set; }

        // Relationship: Quiz has many Questions
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

        // Relationship: Quiz has many Attempts
        public virtual ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
    }
}