using Microsoft.AspNetCore.Identity;

namespace PantryPlan.Api.Domain
{
    // IdentityUser already gives: Id (string, GUID by default), UserName, Email, PasswordHash, PhoneNumber, EmailConfirmed,
    // security stamp, lockout fields, and more.
    public class User : IdentityUser
    {
        //  DateTimeOffset stores an explicit UTC offset alongside the timestamp, which avoids a whole category of timezone bugs.
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
