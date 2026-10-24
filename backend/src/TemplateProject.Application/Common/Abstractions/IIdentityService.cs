using TemplateProject.Application.Common.Models;

namespace TemplateProject.Application.Common.Abstractions;

public interface IIdentityService
{
    Task<IdentityUserModel?> FindByUserNameAsync(string userName);

    Task<IdentityUserModel?> FindByIdAsync(Guid userId);

    Task<bool> CheckPasswordSignInAsync(Guid userId, string password);

    Task<bool> IsEmailConfirmedAsync(Guid userId);

    Task<IReadOnlyCollection<string>> GetRolesAsync(Guid userId);

    Task<ErrorOr<Guid>> CreateUserAsync(CreateUserModel model, string password);

    Task<ErrorOrNone> AddToRoleAsync(Guid userId, string role);
}
