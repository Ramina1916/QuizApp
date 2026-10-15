using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Identity.Client;
using QuizApp.Data;
using QuizApp.Models;
using QuizApp.Models.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;


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




        /* Displays a list of all active quizzes.*/
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




        /* Define Details action method to show Quiz Specifications */
        // Displays the details of a specific quiz, including its questions. → if no attribute is written above the action method, it is considered as Get method
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // NESTED PROJECTION:
            // We build the parent ViewModel, and inside it, we build a List of child ViewModels to prevent sensitive fields.
            var quiz = await _context.Quizzes
                .AsNoTracking() // Skip change tracker setting
                .Where(q => q.Id == id && ( q.CreatedByUserId == userId || q.Status == QuizStatus.Active))   // Display only Active Quizzes
                .Select(q => new QuizDetailsViewModel
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description!,
                    Category = q.Category,
                    Difficulty = q.Difficulty,
                    TimeLimitMinutes = q.TimeLimitMinutes,
                    QuestionCount = q.Questions.Count(qq => !qq.IsDeleted), // return the count of questions not deleted
                    Questions = q.Questions
                        .Where(qq  => !qq.IsDeleted) // Soft delete
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



        /* Define action Create Quiz */
        // Get : Show Empty form 
        [HttpGet]
        [Authorize] // → Ensure only logged-in users can use this page.
        // If not logged in, ASP.NET Core automatically redirects to /Identity/Account/Login
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken] // Prevent (CSRF) attacks
        // Cross-Site Request Forgery: is a web security vulnerability that tricks an authenticated user into performing unintended actions on a web application without their knowledge.
        public async Task<IActionResult> Create(CreateQuizViewModel model) // functionality → Create a Form → Send by HttpPost, check validation, add to the database
        {
            if (!ModelState.IsValid) // if validation failed, show form with error messages
            {
                return View(model);
            }
            var newQuize = new Quiz
            {
                Title = model.Title,
                Description = model.Description,
                Category = model.Category,
                Difficulty = model.Difficulty,
                TimeLimitMinutes = model.TimeLimitMinutes,
                Status = QuizStatus.Draft, // start being drafted until the creator publish them

                // Resource_Based Authorization
                CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                CreatedAt = DateTime.UtcNow
            };

            _context.Quizzes.Add(newQuize); // Add new Quize to the database
            await _context.SaveChangesAsync();

            // PRG Pattern(Post-Redirect-Get) : Prevents duplicate submissions if the user refreshes the page.
            return RedirectToAction(nameof(Details), new { id = newQuize.Id });
        }



        /* Define action AddQuestion method */
        [HttpGet]
        [Authorize]
        // Get : it find the Quiz by its Id, then validate author
        public async Task<IActionResult> AddQuestion(int quizId) //Get should be async because of database operation
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // getting the name identifier of logged in account from cookie

            // Read the quiz from the database with author validation (where clause)
            var quiz = await _context.Quizzes
                .AsNoTracking() // prevent tracking changes automatically
                .Where(q => q.Id == quizId && q.CreatedByUserId == userId)
                .FirstOrDefaultAsync();

            if (quiz is null) return NotFound();

            // return form with filled QuizId
            return View(new CreateQuestionViewModel
            {
                QuizId = quizId,
                Points = 1
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(CreateQuestionViewModel model)
        {

            if (!ModelState.IsValid) return View(model);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var isOwner = await _context.Quizzes
                .AnyAsync(q => q.Id == model.QuizId && q.CreatedByUserId == userId); // Validate author and Quiz Id

            if (!isOwner) return NotFound();

            var order = await _context.Questions.CountAsync(q => q.QuizId == model.QuizId) + 1; // calculate display order asynchronously

            // trace the optionstext then Convert to list
            var optionsList = model.OptionsText
                .Split('\n', StringSplitOptions.RemoveEmptyEntries) // ignore both upper and lower case
                .Select(line => line.Trim())
                .Where(txt => txt.Length > 0)
                .Select(txt => new AnswerOption
                {
                    Text = txt,
                    IsCorrect = model.CorrectAnswer.Trim().Equals(txt, StringComparison.OrdinalIgnoreCase)
                    // ⇡ check whether a correct answer is equal to inputted texts 
                })
                .ToList();

            // inputted options validation
            if (!optionsList.Any()) ModelState.AddModelError(nameof(model.OptionsText), "You must at least input one value.");
            if (!optionsList.Any(option => option.IsCorrect)) ModelState.AddModelError(nameof(model.CorrectAnswer), "The correct answer must match exactly one of the options above.");
            if (!ModelState.IsValid) return View(model); // validating the ModelState → if no, re-show page with error messages


            // create question entity
            var question = new Question
            {
                QuizId = model.QuizId,
                Text = model.Text,
                QuestionType = model.QuestionType,
                DisplayOrder = order,
                Options = optionsList
            };

            // save entity in the database
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            // (PRG)
            return RedirectToAction(nameof(Details), new { id = model.QuizId }); // redirect to the quize detail
        }
    }
}