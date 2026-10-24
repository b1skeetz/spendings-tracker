using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using TemplateProject.Application.Common.Models;

namespace TemplateProject.Api.OpenApi;

internal sealed class OneOfSimplifier : IOpenApiSchemaTransformer
{
    private const string InnerObjectPropertyName = "asT1";

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext ctx, CancellationToken _)
    {
        var modelType = ctx.JsonTypeInfo.Type;

        if (!IsBaseApiResponse(modelType) || !schema.Properties.ContainsKey(InnerObjectPropertyName))
        {
            return Task.CompletedTask;
        }

        var innerObject = schema.Properties
            .FirstOrDefault(p => p.Key == InnerObjectPropertyName);

        schema.Properties.Clear();
            
        foreach (var openApiSchema in innerObject.Value.Properties)
        {
            schema.Properties.Add(openApiSchema);
        }

        return Task.CompletedTask;
    }

    private static bool IsBaseApiResponse(Type type)
    {
        return type.IsGenericType &&
               type.GetGenericTypeDefinition() == typeof(BaseApiResponse<>) &&
               type.GetGenericArguments().Length == 1;
    }
}