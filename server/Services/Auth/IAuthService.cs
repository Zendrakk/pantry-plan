namespace PantryPlan.Api.Services.Auth;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(string email, string password);

    Task<LoginResult> LoginAsync(string email, string password);

    Task<RefreshResult> RefreshAsync(string? rawRefreshToken);

    Task LogoutAsync(string? rawRefreshToken);
}

public record RegisterResult(
    bool Succeeded,
    string? UserId = null,
    string? Email = null,
    IEnumerable<string>? Errors = null);

public record LoginResult(
    bool Succeeded,
    string? AccessToken = null,
    string? RawRefreshToken = null,
    string? UserId = null,
    string? Email = null);

public record RefreshResult(
    bool Succeeded,
    string? AccessToken = null,
    string? RawRefreshToken = null);
