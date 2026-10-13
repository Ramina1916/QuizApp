
namespace QuizApp.Models
{
    public class QuizAttempt
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        // We will link this to ApplicationUser later

        public int QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; } // Null means they abandoned it

        public int? Score { get; set; }      // Null until submitted
        public int MaxScore { get; set; }    // Total possible points

        // Navigation: All the specific answers they gave
        public ICollection<AttemptAnswer> Answers { get; set; } = new List<AttemptAnswer>();
    }
}