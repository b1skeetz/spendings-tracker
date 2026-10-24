using System.Security.Claims;
using Duende.IdentityModel;
using TemplateProject.Application.Common.Abstractions;

namespace TemplateProject.Api.Services;

public class CurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid? _currentUserId;

    public CurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetCurrentUserId()
    {
        if (_currentUserId != null)
        {
            return _currentUserId;
        }

        var httpContext = _httpContextAccessor.HttpContext;

        var userId = httpContext?.User.FindFirstValue(JwtClaimTypes.Subject)
                     ?? httpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            _currentUserId = Guid.Parse(userId);
            return _currentUserId;
        }

        return null;
    }

    public void SetCurrentUserId(Guid userId)
    {
        _currentUserId = userId;
    }
}
