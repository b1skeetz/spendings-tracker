using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TemplateProject.Application.Common.Models;
using TemplateProject.Application.Features.Auth;

namespace TemplateProject.Api.Endpoints.Auth;

[PublicAPI]
public class LoginEndpoint : IFeatureEndpoint
{
    public void Map(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("/api/v1/auth/login", async (
                IMediator mediator,
                HttpResponse httpResponse,
                CancellationToken cancellationToken,
                [FromBody] LoginRequestModel request) =>
            {
                var result = await mediator.Send(new LoginCommand.Request(request.Login, request.Password), cancellationToken);

                return result.Match<BaseApiResponse<LoginCommand.Response>>(
                    error => error,
                    success =>
                    {
                        RefreshTokenCookie.Append(httpResponse, success.RefreshToken);
                        return success.Response;
                    });
            })
            .WithName("Login")
            .WithTags("Auth")
            .WithOpenApi();
    }

    public record LoginRequestModel
    {
        public required string Login { get; set; }
        public required string Password { get; set; }
    }
}
