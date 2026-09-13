using Microsoft.AspNetCore.Identity;
using PantryPlan.Api.Domain;
using PantryPlan.Api.Infrastructure.Auth;

namespace PantryPlan.Api.Services.Auth;

public class AuthService(
    UserManager<User> userManager,
    JwtTokenService jwtTokenService,
    RefreshTokenService refreshTokenService,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<RegisterResult> RegisterAsync(string email, string password)
    {
        var user = new User
        {
            UserName = email,
            Email = email
        };

        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            logger.LogInformation("Registration failed for email {Email}: {Errors}", email, string.Join("; ", errors));
            return new RegisterResult(Succeeded: false, Errors: errors);
        }

        logger.LogInformation("New user registered: {UserId}", user.Id);
        return new RegisterResult(Succeeded: true, UserId: user.Id, Email: user.Email!);
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            logger.LogWarning("Login attempt for unknown email {Email}", email);
            return new LoginResult(Succeeded: false);
        }

        // Check lockout BEFORE checking the password. If the account is already locked out from prior failed
        // attempts, don't even bother validating the password - just fail the same way a wrong password would,
        // so we never reveal "this account is locked" as a distinct signal to whoever is attempting to log in.
        var isLockedOut = await userManager.IsLockedOutAsync(user);

        if (isLockedOut)
        {
            logger.LogWarning("Login attempt for locked-out user {UserId}", user.Id);
            return new LoginResult(Succeeded: false);
        }

        var passwordValid = await userManager.CheckPasswordAsync(user, password);

        if (!passwordValid)
        {
            // Record the failed attempt. Identity tracks this count itself and will automatically lock the
            // account once the configured MaxFailedAccessAttempts threshold (set in Program.cs) is reached.
            await userManager.AccessFailedAsync(user);
            logger.LogWarning("Failed login attempt for user {UserId}", user.Id);
            return new LoginResult(Succeeded: false);
        }

        // Correct password - clear any prior failed attempts so they don't linger and eventually cause an unexpected lockout later.
        await userManager.ResetAccessFailedCountAsync(user);

        var accessToken = jwtTokenService.GenerateAccessToken(user);
        var (rawRefreshToken, _) = await refreshTokenService.GenerateAsync(user.Id);

        logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return new LoginResult(
            Succeeded: true,
            AccessToken: accessToken,
            RawRefreshToken: rawRefreshToken,
            UserId: user.Id,
            Email: user.Email!);
    }

    public async Task<RefreshResult> RefreshAsync(string? rawRefreshToken)
    {
        if (string.IsNullOrEmpty(rawRefreshToken))
        {
            return new RefreshResult(Succeeded: false);
        }

        var existingToken = await refreshTokenService.FindActiveAsync(rawRefreshToken);

        if (existingToken is null)
        {
            var reusedUserId = await refreshTokenService.DetectReuseAsync(rawRefreshToken);
            if (reusedUserId is not null)
            {
                // Token was valid once but already rotated - someone is replaying
                // an old token. Nuke every active session for this user as a precaution.
                logger.LogError("Refresh token reuse detected for user {UserId}. Revoking all sessions.", reusedUserId);
                await refreshTokenService.RevokeAllForUserAsync(reusedUserId);
            }

            return new RefreshResult(Succeeded: false);
        }

        var (newRawToken, newTokenEntity) = await refreshTokenService.GenerateAsync(existingToken.UserId);
        await refreshTokenService.RevokeAsync(existingToken, newTokenEntity.Id);

        var accessToken = jwtTokenService.GenerateAccessToken(existingToken.User);

        return new RefreshResult(Succeeded: true, AccessToken: accessToken, RawRefreshToken: newRawToken);
    }

    public async Task LogoutAsync(string? rawRefreshToken)
    {
        if (string.IsNullOrEmpty(rawRefreshToken))
        {
            return;
        }

        var existingToken = await refreshTokenService.FindActiveAsync(rawRefreshToken);

        if (existingToken is not null)
        {
            await refreshTokenService.RevokeAsync(existingToken);
        }
    }
}
