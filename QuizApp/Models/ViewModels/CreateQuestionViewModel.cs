using System.ComponentModel.DataAnnotations;
using QuizApp.Models;

namespace QuizApp.Models.ViewModels
{
    public class CreateQuestionViewModel  
    {
        public int QuizId { get; set; }

        [Required(ErrorMessage = "Question text is required.")]
        [StringLength(2000, ErrorMessage = "Text exceeds 2000 characters.")]
        public string Text { get; set; } = string.Empty;

        // value types nvere be null
        [Range(1, 3, ErrorMessage = "Please choose a question type.")]
        public QuestionType QuestionType { get; set; }

        [Range(1, 100, ErrorMessage = "Points must be between 1 and 100.")]
        public int Points { get; set; } = 1;

        [Required(ErrorMessage = "Please Enter the answer options. (each line, one option).")]
        public string OptionsText { get; set; } = string.Empty; 

        [Required(ErrorMessage = "Please define a correct answer.")]
        public string CorrectAnswer { get; set; } = string.Empty; 
    }
}