using Microsoft.AspNetCore.Mvc;
using PantryPlan.Api.Models.Auth;
using PantryPlan.Api.Services.Auth;

namespace PantryPlan.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private const string RefreshTokenCookieName = "refreshToken";
    private const string RefreshTokenCookiePath = "/api/auth";

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request.Email, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Created($"/api/users/{result.UserId}", new RegisterResponse(result.UserId!, result.Email!));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await authService.LoginAsync(request.Email, request.Password);

        if (!result.Succeeded)
        {
            return Unauthorized();
        }

        AppendRefreshTokenCookie(result.RawRefreshToken!);

        return Ok(new LoginResponse(result.AccessToken!, result.UserId!, result.Email!));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var rawToken = Request.Cookies[RefreshTokenCookieName];

        var result = await authService.RefreshAsync(rawToken);

        if (!result.Succeeded)
        {
            DeleteRefreshTokenCookie();
            return Unauthorized();
        }

        AppendRefreshTokenCookie(result.RawRefreshToken!);

        return Ok(new RefreshResponse(result.AccessToken!));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var rawToken = Request.Cookies[RefreshTokenCookieName];

        await authService.LogoutAsync(rawToken);

        DeleteRefreshTokenCookie();

        return Ok();
    }

    private void AppendRefreshTokenCookie(string rawToken)
    {
        Response.Cookies.Append(RefreshTokenCookieName, rawToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            Path = RefreshTokenCookiePath
        });
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions { Path = RefreshTokenCookiePath });
    }
}
