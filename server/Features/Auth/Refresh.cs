using PantryPlan.Api.Infrastructure.Auth;

namespace PantryPlan.Api.Features.Auth;

public static class Refresh
{
    public record Response(string AccessToken);

    public static void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/auth/refresh", Handle);
    }

    private static async Task<IResult> Handle(
    HttpContext httpContext,
    JwtTokenService tokenService,
    RefreshTokenService refreshTokenService)
    {
        var rawToken = httpContext.Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(rawToken))
        {
            return Results.Unauthorized();
        }

        var existingToken = await refreshTokenService.FindActiveAsync(rawToken);

        if (existingToken is null)
        {
            var reusedUserId = await refreshTokenService.DetectReuseAsync(rawToken);
            if (reusedUserId is not null)
            {
                // Token was valid once but already rotated - someone is replaying
                // an old token. Nuke every active session for this user as a precaution.
                await refreshTokenService.RevokeAllForUserAsync(reusedUserId);
            }

            httpContext.Response.Cookies.Delete("refreshToken", new CookieOptions { Path = "/api/auth" });
            return Results.Unauthorized();
        }

        var (newRawToken, newTokenEntity) = await refreshTokenService.GenerateAsync(existingToken.UserId);
        await refreshTokenService.RevokeAsync(existingToken, newTokenEntity.Id);

        var accessToken = tokenService.GenerateAccessToken(existingToken.User);

        httpContext.Response.Cookies.Append("refreshToken", newRawToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            Path = "/api/auth"
        });

        return Results.Ok(new Response(accessToken));
    }
}