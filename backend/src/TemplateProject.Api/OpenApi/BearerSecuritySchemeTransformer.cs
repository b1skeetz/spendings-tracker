using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace TemplateProject.Api.OpenApi;

internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument doc, OpenApiDocumentTransformerContext ctx, CancellationToken ct)
    {
        doc.Components ??= new OpenApiComponents();
        doc.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header
        };

        var requirement = new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer", Type = ReferenceType.SecurityScheme
                    }
                },
                []
            }
        };
        foreach (var op in doc.Paths.Values.SelectMany(p => p.Operations.Values))
        {
            op.Security.Add(requirement);
        }

        return Task.CompletedTask;
    }
}