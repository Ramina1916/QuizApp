using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models
{
     
   /* Represents a possible answer for a Question.
    For MultipleChoice/TrueFalse, these are the visible options.
    For ShortAnswer, the "Correct" option holds the accepted text answer.
   */

    public class AnswerOption
    {
        public int Id { get; set; }

        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!; // Specified Question

        [Required]
        [StringLength(500)] 
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; } // Indicates if this option is the correct answer.
    }
}