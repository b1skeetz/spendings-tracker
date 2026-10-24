using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using TemplateProject.Api.Authorization;
using TemplateProject.Api.OpenApi;
using TemplateProject.Api.Services;
using TemplateProject.Application.Common.Abstractions;
using TemplateProject.Domain.Constants;
using TemplateProject.Infrastructure.Authentication;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace TemplateProject.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddOpenApi(opt =>
        {
            opt.AddDocumentTransformer((doc, _, _) =>
            {
                doc.Info.Title = "TemplateProject | v1";
                return Task.CompletedTask;
            });
            opt.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            opt.AddSchemaTransformer<OneOfSimplifier>();
        });

        services.Configure<ForwardedHeadersOptions>(opt =>
        {
            opt.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            opt.KnownProxies.Add(IPAddress.Parse("127.0.0.1"));// for localhost
            opt.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("172.16.0.0"), 12));// for docker env
        });

        services.AddControllers();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

        services.AddAuthorizationBuilder()
            .AddDefaultPolicy(AuthorizationPolicy.UserPolicy, policy => policy.RequireAuthenticatedUser())
            .AddPolicy(AuthorizationPolicy.OnlyAdminPolicy, policy => policy.RequireRole(Roles.Admin))
            .AddPolicy(AuthorizationPolicy.ManagementPolicy, policy => policy.RequireRole(Roles.Admin, Roles.Manager))
            ;

        services.AddAuthorization();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var authOptions = configuration.GetSection(AuthSettings.SectionName).Get<AuthSettings>()
                                  ?? throw new InvalidOperationException(
                                      $"Configuration section '{AuthSettings.SectionName}' is missing");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = authOptions.Audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = authOptions.GetSymmetricSecurityKey(),
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}
