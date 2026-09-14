using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
    [EnableRateLimiting("AuthPolicy")]
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
    [EnableRateLimiting("AuthPolicy")]
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
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            Path = RefreshTokenCookiePath
        };

        cookieOptions.Extensions.Add("Partitioned");

        Response.Cookies.Append(RefreshTokenCookieName, rawToken, cookieOptions);
    }

    private void DeleteRefreshTokenCookie()
    {
        var cookieOptions = new CookieOptions
        {
            Path = RefreshTokenCookiePath,
            Secure = true,
            SameSite = SameSiteMode.None
        };

        cookieOptions.Extensions.Add("Partitioned");

        Response.Cookies.Delete(RefreshTokenCookieName, cookieOptions);
    }
}
