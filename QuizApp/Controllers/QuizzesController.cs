using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using QuizApp.Models.ViewModels;

namespace QuizApp.Controllers
{

    public class QuizzesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuizzesController(ApplicationDbContext context)   // (Constructor injection)
        {
            _context = context;
        }
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
            return View(quizzes);
        }
    }
}