using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApp.Models
{
    public class Question
    {
        [Key] public int Id { get; set; }

        
        public int QuizId { get; set; } // Foreign Key

        [Required]
        [StringLength(2000)]
        public string Title { get; set; } = string.Empty;

        public QuestionType QuestionType { get; set; } = QuestionType.Unspecified;

        public int DisplayOrder { get; set; }

        public bool IsDeleted { get; set; } // Soft Delete decision if the quiz modified

        public DateTime? DeletedAt { get; set; }

        public ICollection<AnswerOption> Options { get; set; }

        public Quiz Quiz { get; set; } = null!; // '!' stop warning  

    }
}
