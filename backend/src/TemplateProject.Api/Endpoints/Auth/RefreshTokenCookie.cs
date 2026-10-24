using TemplateProject.Application.Common.Models;

namespace TemplateProject.Api.Endpoints.Auth;

/// <summary>
/// Refresh-токен передаётся только в HttpOnly-cookie, недоступной из JavaScript,
/// и отправляется браузером только на эндпоинты /api/connect/*.
/// </summary>
public static class RefreshTokenCookie
{
    public const string Name = "refresh_token";
    private const string Path = "/api/connect";

    public static string? Read(HttpRequest request) => request.Cookies[Name];

    public static void Append(HttpResponse response, IssuedRefreshToken refreshToken)
    {
        response.Cookies.Append(Name, refreshToken.Token, CreateOptions(refreshToken.ExpiresAt));
    }

    public static void Delete(HttpResponse response)
    {
        response.Cookies.Delete(Name, CreateOptions(null));
    }

    private static CookieOptions CreateOptions(DateTime? expiresAt) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = Path,
        Expires = expiresAt,
    };
}
