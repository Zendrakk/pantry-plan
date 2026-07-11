using Microsoft.AspNetCore.Identity;

namespace PantryPlan.Api.Domain
{
    // IdentityUser already gives: Id (string, GUID by default), UserName, Email, PasswordHash, PhoneNumber, EmailConfirmed,
    // security stamp, lockout fields, and more.
    public class User : IdentityUser
    {
    }
}
