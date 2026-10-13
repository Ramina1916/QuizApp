using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizApp.Models;

namespace QuizApp.Data
{
   /*
    The main database context for the application.
     Inherits from IdentityDbContext to include ASP.NET Core Identity tables
     (Users, Roles, Logins) alongside our custom Quiz tables.
   */
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        // DbSet properties represent tables in the database
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<AnswerOption> AnswersOptions { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<AttemptAnswer> AttemptAnswers { get; set; }
        // We don't write a DbSet<ApplicationUser> → Identity's base class already provides context.Users.

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Soft delete: deleted questions are invisible to normal reads
            builder.Entity<Question>()
                .HasQueryFilter(q => !q.IsDeleted);

            // Matching rule: an option is visible only while its question is visible.
            // Without this, EF must resolve "required parent" vs "filtered parent"
            // and silently drops rows instead.
            builder.Entity<AnswerOption>()
                .HasQueryFilter(o => !o.Question.IsDeleted);
        }

    }
}
