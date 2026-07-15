namespace PantryPlan.Api.Models.Auth;

public record RegisterRequest(string Email, string Password);

public record RegisterResponse(string UserId, string Email);
