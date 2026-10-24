using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Duende.IdentityModel;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TemplateProject.Application.Common.Abstractions;
using TemplateProject.Application.Common.Models;

namespace TemplateProject.Infrastructure.Authentication;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly AuthSettings _authOptions;
    private readonly TimeProvider _timeProvider;

    public JwtTokenGenerator(IOptions<AuthSettings> authSettings, TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _authOptions = authSettings.Value;
    }

    public AccessToken GenerateToken(Guid userId, string? email, IEnumerable<string> roles)
    {
        var currentDate = _timeProvider.GetUtcNow();

        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Subject, userId.ToString()),
            new(JwtClaimTypes.IssuedAt, currentDate.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        };

        if (!string.IsNullOrEmpty(email))
        {
            claims.Add(new Claim(JwtClaimTypes.Email, email));
        }

        claims.AddRange(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));

        var jwt = new JwtSecurityToken(
            issuer: _authOptions.Issuer,
            audience: _authOptions.Audience,
            claims: claims,
            expires: currentDate.Add(_authOptions.Lifetime).UtcDateTime,
            signingCredentials: new SigningCredentials(_authOptions.GetSymmetricSecurityKey(),
                SecurityAlgorithms.HmacSha256));

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(jwt), _authOptions.Lifetime);
    }
}
