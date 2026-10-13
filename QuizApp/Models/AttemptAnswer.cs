using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models
{
    public class AttemptAnswer
    {
        /* Represents the user's answer to a specific question during a QuizAttempt.
         Uses the Snapshot Pattern: stores the question / option text at the time of the attempt.
         This ensures that if an admin edits or deletes the original Question later, 
         the user's historical attempt record remains accurate and intact.*/
        public int Id { get; set; }

        public int AttemptId { get; set; }
        public QuizAttempt Attempt { get; set; } = null!;

        public int QuestionId { get; set; }
        // Note: We DO NOT put a 'Question Question' navigation here. 
        // We rely on the snapshots below to protect history if the Question is deleted.

        [StringLength(2000)]
        public string QuestionTextSnapshot { get; set; } = string.Empty;

        [StringLength(500)]
        public string? SelectedOptionTextSnapshot { get; set; } // For Multi choice questions and TrueFalse

        [StringLength(500)]
        public string? UserAnswer { get; set; } // For ShortAnswers

        public bool IsCorrect { get; set; }
        public int PointsEarned { get; set; }
    }
}