using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TemplateProject.Application.Common.Abstractions;
using TemplateProject.Infrastructure.Authentication;
using TemplateProject.Infrastructure.Identity;
using TemplateProject.Infrastructure.Persistence;

namespace TemplateProject.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddDbContext<DatabaseContext>((_, opt) =>
        {
            opt.UseNpgsql(configuration.GetConnectionString("Database"));

            if (environment.IsDevelopment())
            {
                opt.EnableSensitiveDataLogging();
            }
        });
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DatabaseContext>());

        services.AddTransient<TimeProvider>(_ => TimeProvider.System);

        services.Configure<AuthSettings>(configuration.GetSection(AuthSettings.SectionName));
        services.AddTransient<IJwtTokenGenerator, JwtTokenGenerator>();

        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddSignInManager()
            .AddEntityFrameworkStores<DatabaseContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        return services;
    }
}
