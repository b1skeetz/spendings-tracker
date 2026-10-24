using JetBrains.Annotations;

namespace TemplateProject.Api.Endpoints;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors | ImplicitUseTargetFlags.WithMembers)]
public interface IFeatureEndpoint
{
    void Map(IEndpointRouteBuilder endpointRouteBuilder);
}