using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TemplateProject.Infrastructure.Authentication;

public class AuthSettings
{
    public const string SectionName = "Auth";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public TimeSpan Lifetime { get; set; }
    public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(30);

    public SymmetricSecurityKey GetSymmetricSecurityKey() => new(Encoding.UTF8.GetBytes(Key));
}
