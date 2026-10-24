using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TemplateProject.Domain.Constants;
using TemplateProject.Infrastructure.Identity;

namespace TemplateProject.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var context = serviceProvider.GetRequiredService<DatabaseContext>();

        await context.Database.MigrateAsync();

        await SetupInitialDataAsync(serviceProvider, context);
    }

    private static async Task SetupInitialDataAsync(IServiceProvider serviceProvider, DatabaseContext context)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var adminLogin = configuration.GetValue<string>("AdminCredentials:Email")!;
        var adminPassword = configuration.GetValue<string>("AdminCredentials:Password")!;

        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        await EnsureRoleExistsAsync(roleManager, Roles.Admin);
        await EnsureRoleExistsAsync(roleManager, Roles.User);
        await EnsureRoleExistsAsync(roleManager, Roles.Manager);

        var adminUser = await EnsureUserAsync(userManager, adminLogin, adminPassword, "Админ Системов");

        await EnsureOnlyRolesAsync(userManager, adminUser, [Roles.Admin]);

        context.ChangeTracker.Clear();
    }

    private static async Task EnsureRoleExistsAsync(RoleManager<ApplicationRole> roleManager, string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        await EnsureIdentitySucceededAsync(await roleManager.CreateAsync(new ApplicationRole
        {
            Name = roleName,
        }));
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string fullName)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user != null)
        {
            var shouldUpdate = false;

            if (!string.Equals(user.Name, fullName, StringComparison.Ordinal))
            {
                user.Name = fullName;
                shouldUpdate = true;
            }

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                shouldUpdate = true;
            }

            if (shouldUpdate)
            {
                await EnsureIdentitySucceededAsync(await userManager.UpdateAsync(user));
            }

            return user;
        }

        user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            Name = fullName,
            RegistrationDate = TimeProvider.System.GetUtcNow().UtcDateTime,
            Age = 25
        };

        await EnsureIdentitySucceededAsync(await userManager.CreateAsync(user, password));
        return user;
    }

    private static async Task EnsureOnlyRolesAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user,
        IEnumerable<string> expectedRoles)
    {
        var currentRoles = await userManager.GetRolesAsync(user);
        var expectedRoleSet = expectedRoles.ToHashSet(StringComparer.Ordinal);
        var rolesToRemove = currentRoles.Where(role => !expectedRoleSet.Contains(role)).ToArray();

        if (rolesToRemove.Length > 0)
        {
            await EnsureIdentitySucceededAsync(await userManager.RemoveFromRolesAsync(user, rolesToRemove));
        }

        foreach (var role in expectedRoleSet)
        {
            if (!currentRoles.Contains(role, StringComparer.Ordinal))
            {
                await EnsureIdentitySucceededAsync(await userManager.AddToRoleAsync(user, role));
            }
        }
    }

    private static Task EnsureIdentitySucceededAsync(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return Task.CompletedTask;
        }

        var errors = string.Join("; ", result.Errors.Select(x => $"{x.Code}: {x.Description}"));
        throw new InvalidOperationException($"Identity seeding failed: {errors}");
    }
}
