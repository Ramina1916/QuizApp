namespace QuizApp.Models.ViewModels
{
    public class AttemptHistoryViewModel
    {
        public int AttemptId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public int? Score { get; set; }
        public int MaxScore { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}