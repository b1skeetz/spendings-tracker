using TemplateProject.Api.Filters;

namespace TemplateProject.Api.Endpoints;

public static class EndpointRegistrationExtensions
{
    public static void RegisterFeatureEndpoints(this WebApplication webApplication)
    {
        var types = typeof(EndpointRegistrationExtensions).Assembly.GetTypes();
        var endpointRegistrations = types
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t => typeof(IFeatureEndpoint).IsAssignableFrom(t))
            .ToList();

        foreach (var registration in endpointRegistrations)
        {
            var featureEndpoint = (IFeatureEndpoint)Activator.CreateInstance(registration)!;

            var groupMap = webApplication
                .MapGroup("")
                .AddEndpointFilter<BaseResponseFilter>();

            featureEndpoint.Map(groupMap);
        }
    }
}
