using Microsoft.AspNetCore.Identity;
using PantryPlan.Api.Domain;
using PantryPlan.Api.Infrastructure.Auth;

namespace PantryPlan.Api.Features.Auth;

public static class Login
{
    public record Request(string Email, string Password);

    public record Response(string AccessToken, string UserId, string Email);

    public static void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/auth/login", Handle);
    }

    private static async Task<IResult> Handle(
        Request request,
        UserManager<User> userManager,
        JwtTokenService tokenService,
        RefreshTokenService refreshTokenService,
        HttpContext httpContext)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return Results.Unauthorized();
        }

        var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);

        if (!passwordValid)
        {
            return Results.Unauthorized();
        }

        var accessToken = tokenService.GenerateAccessToken(user);
        var (rawRefreshToken, _) = await refreshTokenService.GenerateAsync(user.Id);

        httpContext.Response.Cookies.Append("refreshToken", rawRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            Path = "/api/auth"
        });

        return Results.Ok(new Response(accessToken, user.Id, user.Email!));
    }
}