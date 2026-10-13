using Microsoft.AspNetCore.Identity;

namespace QuizApp.Models
{
/*     Represents a user in the application.
     Inherits from IdentityUser to get built-in secure features 
     like password hashing, email confirmation, and role management.
*/

    public class ApplicationUser : IdentityUser
    {
        // You can add custom properties here later, for example:
        // public string? FirstName { get; set; }
        // public DateTime? DateOfBirth { get; set; }
    }
}