namespace PantryPlan.Api.Models.Auth;

public record LoginRequest(string Email, string Password);

public record LoginResponse(string AccessToken, string UserId, string Email);
