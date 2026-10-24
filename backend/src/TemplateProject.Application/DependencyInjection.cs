using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TemplateProject.Application.Common.Behaviors;

namespace TemplateProject.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddAutoMapper(opt =>
        {
            opt.AddMaps(assembly);
        });
        services.AddMediatR(opt =>
        {
            opt.RegisterServicesFromAssembly(assembly);
        });

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }
}
