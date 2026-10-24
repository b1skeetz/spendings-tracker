using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TemplateProject.Application.Common.Abstractions;
using TemplateProject.Application.Common.Errors;
using TemplateProject.Application.Common.Models;
using TemplateProject.Infrastructure.Authentication;
using TemplateProject.Infrastructure.Persistence;

namespace TemplateProject.Infrastructure.Identity;

public class RefreshTokenService : IRefreshTokenService
{
    private const int TokenSizeInBytes = 64;

    private readonly DatabaseContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly AuthSettings _authOptions;

    public RefreshTokenService(DatabaseContext context, TimeProvider timeProvider, IOptions<AuthSettings> authSettings)
    {
        _context = context;
        _timeProvider = timeProvider;
        _authOptions = authSettings.Value;
    }

    public IssuedRefreshToken Issue(Guid userId)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(TokenSizeInBytes));

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = Hash(token),
            CreatedAt = now,
            ExpiresAt = now.Add(_authOptions.RefreshTokenLifetime),
        };

        _context.RefreshTokens.Add(refreshToken);

        return new IssuedRefreshToken(token, refreshToken.ExpiresAt);
    }

    public async Task<ErrorOr<Guid>> RedeemAsync(string token, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var refreshToken = await FindAsync(token, cancellationToken);

        if (refreshToken is null)
        {
            return Errors.RefreshTokenInvalid.Instance;
        }

        if (refreshToken.RevokedAt != null)
        {
            await RevokeAllActiveAsync(refreshToken.UserId, now, cancellationToken);
            return Errors.RefreshTokenInvalid.Instance;
        }

        if (refreshToken.ExpiresAt <= now)
        {
            return Errors.RefreshTokenInvalid.Instance;
        }

        refreshToken.RevokedAt = now;

        return refreshToken.UserId;
    }

    public async Task RevokeAsync(string token, CancellationToken cancellationToken)
    {
        var refreshToken = await FindAsync(token, cancellationToken);

        if (refreshToken is { RevokedAt: null })
        {
            refreshToken.RevokedAt = _timeProvider.GetUtcNow().UtcDateTime;
        }
    }

    private Task<RefreshToken?> FindAsync(string token, CancellationToken cancellationToken)
    {
        var hash = Hash(token);

        return _context.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
    }

    private async Task RevokeAllActiveAsync(Guid userId, DateTime now, CancellationToken cancellationToken)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var activeToken in activeTokens)
        {
            activeToken.RevokedAt = now;
        }
    }

    private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
