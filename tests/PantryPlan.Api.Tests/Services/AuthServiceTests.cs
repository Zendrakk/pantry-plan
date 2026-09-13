using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using PantryPlan.Api.Infrastructure.Auth;
using PantryPlan.Api.Services.Auth;

namespace PantryPlan.Api.Tests.Services
{
    public class AuthServiceTests
    {
        private static JwtTokenService CreateJwtTokenService()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:SigningKey"] = "test-signing-key-that-is-long-enough-for-hmacsha256",
                    ["Jwt:Issuer"] = "TestIssuer",
                    ["Jwt:Audience"] = "TestAudience"
                })
                .Build();

            return new JwtTokenService(config);
        }

        private static (AuthService service, RefreshTokenService refreshTokenService) CreateService()
        {
            var db = TestHelpers.CreateDbContext();
            var userManager = TestHelpers.CreateUserManager(db);
            var jwtTokenService = CreateJwtTokenService();
            var refreshTokenService = new RefreshTokenService(db);
            var logger = NullLogger<AuthService>.Instance;

            return (new AuthService(userManager, jwtTokenService, refreshTokenService, logger), refreshTokenService);
        }

        [Fact]
        public async Task RegisterAsync_WithValidCredentials_Succeeds()
        {
            var (service, _) = CreateService();

            var result = await service.RegisterAsync("test@example.com", "ValidPass123!");

            Assert.True(result.Succeeded);
            Assert.NotNull(result.UserId);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task RegisterAsync_WithWeakPassword_FailsWithErrors()
        {
            var (service, _) = CreateService();

            var result = await service.RegisterAsync("test@example.com", "weak");

            Assert.False(result.Succeeded);
            Assert.NotNull(result.Errors);
            Assert.NotEmpty(result.Errors!);
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateEmail_Fails()
        {
            var (service, _) = CreateService();
            await service.RegisterAsync("test@example.com", "ValidPass123!");

            var result = await service.RegisterAsync("test@example.com", "AnotherPass123!");

            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task LoginAsync_WithCorrectCredentials_Succeeds()
        {
            var (service, _) = CreateService();
            await service.RegisterAsync("test@example.com", "ValidPass123!");

            var result = await service.LoginAsync("test@example.com", "ValidPass123!");

            Assert.True(result.Succeeded);
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RawRefreshToken);
        }

        [Fact]
        public async Task LoginAsync_WithWrongPassword_Fails()
        {
            var (service, _) = CreateService();
            await service.RegisterAsync("test@example.com", "ValidPass123!");

            var result = await service.LoginAsync("test@example.com", "WrongPassword123!");

            Assert.False(result.Succeeded);
            Assert.Null(result.AccessToken);
        }

        [Fact]
        public async Task LoginAsync_WithNonexistentEmail_FailsSameAsWrongPassword()
        {
            var (service, _) = CreateService();

            var result = await service.LoginAsync("nobody@example.com", "SomePassword123!");

            // Deliberately identical failure shape to a wrong-password login,
            // preventing user enumeration - this test locks that behavior in.
            Assert.False(result.Succeeded);
            Assert.Null(result.AccessToken);
        }

        [Fact]
        public async Task RefreshAsync_WithValidToken_RotatesAndSucceeds()
        {
            var (service, _) = CreateService();
            await service.RegisterAsync("test@example.com", "ValidPass123!");
            var login = await service.LoginAsync("test@example.com", "ValidPass123!");

            var result = await service.RefreshAsync(login.RawRefreshToken);

            Assert.True(result.Succeeded);
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RawRefreshToken);
            Assert.NotEqual(login.RawRefreshToken, result.RawRefreshToken);
        }

        [Fact]
        public async Task RefreshAsync_WithNullToken_Fails()
        {
            var (service, _) = CreateService();

            var result = await service.RefreshAsync(null);

            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task RefreshAsync_WithGarbageToken_Fails()
        {
            var (service, _) = CreateService();

            var result = await service.RefreshAsync("not-a-real-token");

            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task RefreshAsync_WhenUsedTwiceWithSameToken_SecondCallFailsAndRevokesAllSessions()
        {
            // This is the reuse-detection / theft-response test.
            var (service, refreshTokenService) = CreateService();
            await service.RegisterAsync("test@example.com", "ValidPass123!");
            var login = await service.LoginAsync("test@example.com", "ValidPass123!");

            var firstRefresh = await service.RefreshAsync(login.RawRefreshToken);
            Assert.True(firstRefresh.Succeeded);

            // Replay the ORIGINAL (now-rotated-away) token, simulating a thief
            // using a stolen token after the legitimate user already refreshed.
            var replayResult = await service.RefreshAsync(login.RawRefreshToken);

            Assert.False(replayResult.Succeeded);

            // The token issued by the first (legitimate) refresh should now
            // ALSO be revoked, since reuse detection nukes every active session.
            var secondRefreshToken = await refreshTokenService.FindActiveAsync(firstRefresh.RawRefreshToken!);
            Assert.Null(secondRefreshToken);
        }

        [Fact]
        public async Task LogoutAsync_WithValidToken_RevokesIt()
        {
            var (service, refreshTokenService) = CreateService();
            await service.RegisterAsync("test@example.com", "ValidPass123!");
            var login = await service.LoginAsync("test@example.com", "ValidPass123!");

            await service.LogoutAsync(login.RawRefreshToken);

            var tokenAfterLogout = await refreshTokenService.FindActiveAsync(login.RawRefreshToken!);
            Assert.Null(tokenAfterLogout);
        }

        [Fact]
        public async Task LogoutAsync_WithNullToken_DoesNotThrow()
        {
            var (service, _) = CreateService();

            var exception = await Record.ExceptionAsync(() => service.LogoutAsync(null));

            Assert.Null(exception);
        }
    }
}
