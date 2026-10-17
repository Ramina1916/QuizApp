namespace QuizApp.Models.ViewModels
{
    // The Slot: Represents one question the user needs to answer
    public class TakeAnswerSlotViewModel
    {
        public int QuestionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public QuestionType QuestionType { get; set; }

        // Only needed for MultipleChoice and TrueFalse
        public List<TakeOptionViewModel> Options { get; set; } = new();
    }
}

