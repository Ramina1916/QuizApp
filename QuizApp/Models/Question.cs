using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApp.Models
{
    public class Question
    {
        /*
         Represents an individual question within a specific Quiz.
         Includes Soft Delete (IsDeleted) so that deleting a question 
        does not destroy historical QuizAttempt records.
        */
        public int Id { get; set; }

        // Foreign key
        public int QuizId { get; set; }   
        
        public Quiz Quiz { get; set; } = null!;

        [Required]
        [StringLength(2000)]
        public string Text { get; set; } = string.Empty;    

        public QuestionType QuestionType { get; set; } = QuestionType.Unspecified;

        public int DisplayOrder { get; set; } = 1;

        [Range(1, 100)]
        public int Points { get; set; } = 1;    

        // Soft Delete properties
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        // The possible answers for this question
        public ICollection<AnswerOption> Options { get; set; } = new List<AnswerOption>();
    }
}
