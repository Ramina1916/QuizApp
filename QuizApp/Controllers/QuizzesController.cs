using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel;
using QuizApp.Data;
using QuizApp.Models;
using QuizApp.Models.ViewModels;


namespace QuizApp.Controllers
{

    public class QuizzesController : Controller
    {
     /*
     Handles all public-facing requests related to Quizzes.
     Responsibility: Translate URLs into database queries, shape the data into ViewModels, 
     and pass them to the View.Contains NO business logic (like scoring).
     */
        private readonly ApplicationDbContext _context;

        // DEPENDENCY INJECTION
        // We declare that we NEED an ApplicationDbContext. The framework provides it.
        public QuizzesController(ApplicationDbContext context)   // (Constructor injection)
        {
            _context = context;
        }


        // GET: /Quizzes
        // Displays a list of all active quizzes.
        public async Task<IActionResult> Index()
        {
            var quizzes = await _context.Quizzes
                .Where(q => q.Status == QuizStatus.Active)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new QuizListItemViewModel // select is a projection operator, so filters should excute before it
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    Category = q.Category,
                    Difficulty = q.Difficulty,
                    TimeLimitMinutes = q.TimeLimitMinutes,
                    QuestionCount = q.Questions.Count,
                    AttemptCount = q.Attempts.Count
                })
                .ToListAsync();

            // Pass the list of ViewModels to the View. The View only knows about ViewModels, never Entities.
            return View(quizzes);
        }


        // GET: /Quizzes/Details/5
        // Displays the details of a specific quiz, including its questions.
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null) return NotFound();

            // NESTED PROJECTION:
            // We build the parent ViewModel, and inside it, we build a List of child ViewModels to prevent sensitive fields.
            var quiz = await _context.Quizzes
                .AsNoTracking() // Skip change tracker setting
                .Where(q => q.Id == id && q.Status == QuizStatus.Active)   // Display only Active Quizzes
                .Select(q => new QuizDetailsViewModel
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description!,
                    Category = q.Category,
                    Difficulty = q.Difficulty,
                    TimeLimitMinutes = q.TimeLimitMinutes,
                    QuestionCount = q.Questions.Count,
                    Questions = q.Questions
                        .OrderBy(qq => qq.DisplayOrder) // Ensure questions appear in the correct order
                        .Select(qq => new QuestionListItemViewModel
                        {
                            Id = qq.Id,
                            Text = qq.Text,
                            QuestionType = qq.QuestionType,
                            Points = qq.Points
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
            // FirstOrDefaultAsync: We only expect ONE quiz to match this ID. 
            // (We use this instead of FindAsync because FindAsync ignores global query filters → Ingnoring Soft Deletion).

            if (quiz is null) return NotFound();
            return View(quiz);
        }

    }
}