using Microsoft.AspNetCore.Identity;
using PantryPlan.Api.Domain;

namespace PantryPlan.Api.Features.Auth;

public static class Register
{
    public record Request(string Email, string Password);

    public record Response(string UserId, string Email);

    public static void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/auth/register", Handle);
    }

    private static async Task<IResult> Handle(Request request, UserManager<User> userManager)
    {
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return Results.BadRequest(new { errors });
        }

        return Results.Created($"/api/users/{user.Id}", new Response(user.Id, user.Email!));
    }
}