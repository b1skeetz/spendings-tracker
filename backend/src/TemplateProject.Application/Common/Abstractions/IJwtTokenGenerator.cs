using TemplateProject.Application.Common.Models;

namespace TemplateProject.Application.Common.Abstractions;

public interface IJwtTokenGenerator
{
    AccessToken GenerateToken(Guid userId, string? email, IEnumerable<string> roles);
}
