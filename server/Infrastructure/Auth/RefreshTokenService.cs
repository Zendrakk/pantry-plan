using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using PantryPlan.Api.Domain;
using PantryPlan.Api.Infrastructure.Persistence;

namespace PantryPlan.Api.Infrastructure.Auth;

public class RefreshTokenService(AppDbContext db)
{
    private readonly AppDbContext _db = db;
    private const int TokenLifetimeDays = 30;

    public async Task<(string rawToken, RefreshToken entity)> GenerateAsync(string userId)
    {
        var rawToken = GenerateRawToken();
        var tokenHash = Hash(rawToken);

        var entity = new RefreshToken
        {
            TokenHash = tokenHash,
            UserId = userId,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(TokenLifetimeDays)
        };

        _db.RefreshTokens.Add(entity);
        await _db.SaveChangesAsync();

        return (rawToken, entity);
    }

    public async Task<RefreshToken?> FindActiveAsync(string rawToken)
    {
        var tokenHash = Hash(rawToken);

        var token = await _db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        return token is not null && token.IsActive ? token : null;
    }

    public async Task RevokeAsync(RefreshToken token, Guid? replacedByTokenId = null)
    {
        token.RevokedAt = DateTimeOffset.UtcNow;
        token.ReplacedByTokenId = replacedByTokenId;
        await _db.SaveChangesAsync();
    }

    private static string GenerateRawToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}