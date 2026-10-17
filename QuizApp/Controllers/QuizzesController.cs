using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Identity.Client;
using QuizApp.Data;
using QuizApp.Models;
using QuizApp.Models.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
                .Where(q => q.Status == QuizStatus.Active && !q.IsDeleted)
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
                .Where(q => q.Id == id && (q.CreatedByUserId == userId || q.Status == QuizStatus.Active))   // Display only Active Quizzes
                .Select(q => new QuizDetailsViewModel
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description!,
                    Category = q.Category,
                    Difficulty = q.Difficulty,
                    TimeLimitMinutes = q.TimeLimitMinutes,
                    QuestionCount = q.Questions.Count(qq => !qq.IsDeleted), // return the count of questions not deleted
                    IsOwner =(q.CreatedByUserId == userId), // Compare the quiz creator to the logged-in user
                    Questions = q.Questions
                        .Where(qq => !qq.IsDeleted) // Soft delete
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
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(txt => txt.Length > 0)
                .Select(txt => new AnswerOption
                {
                    Text = txt,
                    IsCorrect = model.CorrectAnswer.Trim().Equals(txt, StringComparison.OrdinalIgnoreCase) // ignore both upper and lower case letters
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

        /* Define Take method, that load the Quiz page to take */
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Take(int id)
        {

            var quiz = await _context.Quizzes
                .AsNoTracking()
                .Where(q => q.Status == QuizStatus.Active && q.Id == id)
                .Select(q => new TakeQuizViewModel
                {
                    QuizId = q.Id,
                    Title = q.Title,
                    TimeLimitMinutes = q.TimeLimitMinutes,
                    Answers = q.Questions
                     .Where(qq => !qq.IsDeleted)
                     .OrderBy(qq => qq.DisplayOrder)
                     .Select(qq => new TakeAnswerSlotViewModel
                     {
                         QuestionId = qq.Id,
                         Text = qq.Text,
                         QuestionType = qq.QuestionType,
                         Options = qq.Options
                         .Select(opt => new TakeOptionViewModel
                         {
                             Id = opt.Id,
                             Text = opt.Text
                         }).ToList()
                     }).ToList()
                })
                .FirstOrDefaultAsync();

            if (quiz is null) return NotFound();

            return View(quiz); // This should be your ViewModel, not the entity
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(SubmitQuizViewModel model)
        {
            if (!ModelState.IsValid)
                // If model is invalid, redirect back to Take with an error
                return RedirectToAction(nameof(Take), new { id = model.QuizId });

            // Check Is the quiz actually Active?
            var isActive = await _context.Quizzes.AnyAsync(q => q.Id == model.QuizId && q.Status == QuizStatus.Active);
            if (!isActive) return NotFound();

            // Load the TRUTH from the database
            var dbQuestions = await _context.Questions
                .Where(q => q.QuizId == model.QuizId && !q.IsDeleted)
                .Include(q => q.Options)
                .AsNoTracking()
                .ToListAsync();

            var truth = dbQuestions.ToDictionary(q => q.Id);

            int earnedScore = 0;
            int maxScore = dbQuestions.Sum(q => q.Points); // Max score comes from DB, not browser!
            var attemptAnswers = new List<AttemptAnswer>();

            // Judge each submitted answer
            foreach (var posted in model.Answers)
            {
                if (truth.TryGetValue(posted.QuestionId, out var q))
                {
                    bool isCorrect = false;
                    int pointsEarned = 0;
                    string snapshotText = null!; // This will hold either the option text OR the typed text

                    if (q.QuestionType == QuestionType.ShortAnswer)
                    {
                        // SHORT ANSWER LOGIC:
                        // Get all correct options for this question, cleaned up (lowercase, trimmed)
                        var correctAnswers = q.Options
                            .Where(o => o.IsCorrect)
                            .Select(o => o.Text.Trim().ToLower())
                            .ToList();

                        // Clean up the user's typed answer
                        var userAnswerClean = posted.UserAnswer?.Trim().ToLower() ?? "";

                        // Check if the user's answer matches ANY of the correct answers
                        isCorrect = correctAnswers.Contains(userAnswerClean);
                        snapshotText = posted.UserAnswer!; // Snapshot is what they typed
                    }
                    else
                    {
                        // MULTIPLE CHOICE / TRUE-FALSE LOGIC:
                        var chosen = q.Options.FirstOrDefault(o => o.Id == posted.SelectedOptionId);

                        if (chosen != null)
                        {
                            isCorrect = chosen.IsCorrect;
                            snapshotText = chosen.Text; // Snapshot is the option they clicked
                        }
                    }

                    // Award points if correct
                    if (isCorrect)
                    {
                        pointsEarned = q.Points;
                        earnedScore += pointsEarned;
                    }

                    // Build the snapshot record
                    attemptAnswers.Add(new AttemptAnswer
                    {
                        AttemptId = 0,
                        QuestionId = posted.QuestionId,
                        QuestionTextSnapshot = q.Text,
                        SelectedOptionTextSnapshot = snapshotText, // Works for BOTH question types now!
                        UserAnswer = posted.UserAnswer,
                        IsCorrect = isCorrect,
                        PointsEarned = pointsEarned
                    });
                }
            }

            // Save the attempt and all its answers in ONE transaction
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var attempt = new QuizAttempt
            {
                UserId = userId!,
                QuizId = model.QuizId,
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                Score = earnedScore,
                MaxScore = maxScore,
                Answers = attemptAnswers // EF Core cascades this save!
            };

            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            // Redirect to the results page
            return RedirectToAction("Results", new { id = attempt.Id });
        }

        /* Get the Result */
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Results(int id)
        {
            // SECURITY: Get the current user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Load the attempt AND its snapshot answers.
            // SECURITY CHECK: We add "&& a.UserId == userId" so users can ONLY see their own results.
            var attempt = await _context.QuizAttempts
                .AsNoTracking()
                .Include(a => a.Answers)
                .Where(a => a.Id == id && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (attempt is null) return NotFound();

            return View(attempt);
        }



        /* Display a history of Quiz taken */
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // We join Attempts with Quizzes to get the Title.
            // SECURITY: We filter by UserId so users ONLY see their own history.
            var history = await _context.QuizAttempts
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .Select(a => new AttemptHistoryViewModel
                {
                    AttemptId = a.Id,
                    QuizTitle = a.Quiz!.Title, // EF Core translates this to a SQL JOIN automatically!
                    Score = a.Score,
                    MaxScore = a.MaxScore,
                    CompletedAt = a.CompletedAt!.Value
                })
                .OrderByDescending(a => a.CompletedAt)
                .ToListAsync();

            return View(history);
        }



        /* Display creator dashboard */
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyQuizzes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // SECURITY: We ONLY return quizzes created by the currently logged-in user.
            var myQuizzes = await _context.Quizzes
                .AsNoTracking()
                .Where(q => q.CreatedByUserId == userId)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new MyQuizViewModel
                {
                    Id = q.Id,
                    Title = q.Title,
                    Status = q.Status,
                    QuestionCount = q.Questions.Count(qq => !qq.IsDeleted),
                    CreatedAt = q.CreatedAt
                })
                .ToListAsync();

            return View(myQuizzes);
        }




        /* PUBLISH A DRAFT QUIZ*/
        [HttpPost] // JUNIOR TIP: Never use [HttpGet] to change data! If a web crawler visits a GET delete/publish link, it accidentally changes your database. Always use POST for state changes.
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // SECURITY: Find the quiz AND verify ownership in one query.
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == id && q.CreatedByUserId == userId);

            if (quiz is null) return NotFound();

            // LOGIC: Only publish if it's actually a draft.
            if (quiz.Status == QuizStatus.Draft)
            {
                quiz.Status = QuizStatus.Active;
                await _context.SaveChangesAsync();
            }

            // PRG Pattern: Redirect back to the management page to prevent double-submissions.
            return RedirectToAction(nameof(MyQuizzes));
        }




        /*SOFT DELETE A QUIZ*/
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == id && q.CreatedByUserId == userId);

            if (quiz is null) return NotFound();

            // WHY SOFT DELETE? 
            // If we hard delete (remove the row), users who already took this quiz will have 
            // broken history records. Soft delete just hides it from the public list.
            quiz.IsDeleted = true;
            quiz.DeletedAt = DateTime.UtcNow;

            // JUNIOR TIP: We don't need to change the Status to Draft. 
            // The global query filter (!q.IsDeleted) will automatically hide it from the public Index page.
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyQuizzes));
        }



        // GET: Show the Edit Form
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // SECURITY: Load the quiz AND verify ownership in one go.
            var quiz = await _context.Quizzes
                .AsNoTracking() // Read-only for the form display
                .FirstOrDefaultAsync(q => q.Id == id && q.CreatedByUserId == userId);

            if (quiz is null) return NotFound(); // Blocks non-owners safely

            // Map Entity to ViewModel
            var model = new EditQuizViewModel
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                Category = quiz.Category,
                Difficulty = quiz.Difficulty,
                TimeLimitMinutes = quiz.TimeLimitMinutes
            };

            return View(model);
        }

        

        // POST: Save the Edits
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditQuizViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // SECURITY: Load the actual entity from DB to update it. 
            // We DO NOT use AsNoTracking here because we intend to SaveChanges.
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == model.Id && q.CreatedByUserId == userId);

            if (quiz is null) return NotFound();

            // UPDATE: Only update the properties the user is allowed to change.
            // Notice we DO NOT update Status, CreatedAt, or CreatedByUserId here.
            quiz.Title = model.Title;
            quiz.Description = model.Description;
            quiz.Category = model.Category;
            quiz.Difficulty = model.Difficulty;
            quiz.TimeLimitMinutes = model.TimeLimitMinutes;

            await _context.SaveChangesAsync();

            // PRG Pattern: Redirect to Details to see the changes
            return RedirectToAction(nameof(Details), new { id = quiz.Id });
        }
    }
}