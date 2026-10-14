namespace QuizApp.Models.ViewModels
{
    public class QuestionListItemViewModel
    {
        public int Id { set; get; }
        public string Text { get; set; } = string.Empty;

        public QuestionType QuestionType { get; set; }

        public int Points { get; set; }

    }
}
