using Microsoft.AspNetCore.Identity;
using TemplateProject.Application.Common.Abstractions;
using TemplateProject.Application.Common.Errors;
using TemplateProject.Application.Common.Models;

namespace TemplateProject.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityService(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    private UserManager<ApplicationUser> UserManager => _signInManager.UserManager;

    public async Task<IdentityUserModel?> FindByUserNameAsync(string userName)
    {
        var user = await UserManager.FindByNameAsync(userName);

        return user is null ? null : new IdentityUserModel(user.Id, user.UserName, user.Email);
    }

    public async Task<IdentityUserModel?> FindByIdAsync(Guid userId)
    {
        var user = await UserManager.FindByIdAsync(userId.ToString());

        return user is null ? null : new IdentityUserModel(user.Id, user.UserName, user.Email);
    }

    public async Task<bool> CheckPasswordSignInAsync(Guid userId, string password)
    {
        var user = await GetUserAsync(userId);

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);

        return result.Succeeded;
    }

    public async Task<bool> IsEmailConfirmedAsync(Guid userId)
    {
        var user = await GetUserAsync(userId);

        return await UserManager.IsEmailConfirmedAsync(user);
    }

    public async Task<IReadOnlyCollection<string>> GetRolesAsync(Guid userId)
    {
        var user = await GetUserAsync(userId);

        var roles = await UserManager.GetRolesAsync(user);

        return roles.ToArray();
    }

    public async Task<ErrorOr<Guid>> CreateUserAsync(CreateUserModel model, string password)
    {
        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            Name = model.Name,
            Age = model.Age,
            RegistrationDate = model.RegistrationDate,
            EmailConfirmed = model.EmailConfirmed,
        };

        var result = await UserManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            return ToError(result);
        }

        return user.Id;
    }

    public async Task<ErrorOrNone> AddToRoleAsync(Guid userId, string role)
    {
        var user = await GetUserAsync(userId);

        var result = await UserManager.AddToRoleAsync(user, role);

        if (!result.Succeeded)
        {
            return ToError(result);
        }

        return Unit.Value;
    }

    private async Task<ApplicationUser> GetUserAsync(Guid userId)
    {
        // FindByIdAsync сначала ищет в ChangeTracker, поэтому повторного запроса к БД
        // для уже загруженного в рамках скоупа пользователя не будет
        return await UserManager.FindByIdAsync(userId.ToString())
               ?? throw new InvalidOperationException($"User {userId} not found");
    }

    private static Errors.IdentityOperationFailed ToError(IdentityResult result)
    {
        return new Errors.IdentityOperationFailed(
            result.Errors.Select(x => $"{x.Code}: {x.Description}").ToArray());
    }
}
