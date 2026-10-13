namespace QuizApp.Models
{
    // Enums are ideal for values that do not change
    public enum DifficultyLevel { Unspecified = 0, Easy, Medium, Hard }
    public enum QuizStatus { Draft, Active, Archived }

    public enum QuestionType { Unspecified = 0, MultipleChoice, TrueFalse, ShortAnswer }
}
