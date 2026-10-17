namespace QuizApp.Models.ViewModels
{
    // Represents the whole quiz page
    public class TakeQuizViewModel
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<TakeAnswerSlotViewModel> Answers { get; set; } = new();
    }
}

