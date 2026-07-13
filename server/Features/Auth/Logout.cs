using PantryPlan.Api.Infrastructure.Auth;

namespace PantryPlan.Api.Features.Auth;

public static class Logout
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/auth/logout", Handle);
    }

    private static async Task<IResult> Handle(
        HttpContext httpContext,
        RefreshTokenService refreshTokenService)
    {
        var rawToken = httpContext.Request.Cookies["refreshToken"];

        if (!string.IsNullOrEmpty(rawToken))
        {
            var existingToken = await refreshTokenService.FindActiveAsync(rawToken);
            if (existingToken is not null)
            {
                await refreshTokenService.RevokeAsync(existingToken);
            }
        }

        httpContext.Response.Cookies.Delete("refreshToken", new CookieOptions { Path = "/api/auth" });

        return Results.Ok();
    }
}