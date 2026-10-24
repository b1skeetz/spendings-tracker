using System.Text.Json.Serialization;
using JetBrains.Annotations;
using MediatR;
using TemplateProject.Api.Filters;
using TemplateProject.Application.Features.Auth;

namespace TemplateProject.Api.Endpoints.Auth;

[PublicAPI]
public class TokenEndpoints : IFeatureEndpoint
{
    private const string RefreshTokenGrantType = "refresh_token";

    public void Map(IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Обновление access-токена по refresh-токену из cookie (grant_type=refresh_token, OAuth 2.0)
        endpointRouteBuilder.MapPost("/api/connect/token", async (
                IMediator mediator,
                HttpRequest httpRequest,
                HttpResponse httpResponse,
                CancellationToken cancellationToken) =>
            {
                var grantType = httpRequest.HasFormContentType
                    ? (await httpRequest.ReadFormAsync(cancellationToken))["grant_type"].ToString()
                    : null;

                if (grantType != RefreshTokenGrantType)
                {
                    return Results.BadRequest(new Dictionary<string, string> { ["error"] = "unsupported_grant_type" });
                }

                var result = await mediator.Send(
                    new RefreshTokenCommand.Request(RefreshTokenCookie.Read(httpRequest)),
                    cancellationToken);

                return result.Match(
                    error =>
                    {
                        RefreshTokenCookie.Delete(httpResponse);
                        return error.ToResult();
                    },
                    success =>
                    {
                        RefreshTokenCookie.Append(httpResponse, success.RefreshToken);
                        return Results.Json(new TokenResponse
                        {
                            AccessToken = success.AccessToken.Value,
                            ExpiresIn = (int)success.AccessToken.Lifetime.TotalSeconds,
                        });
                    });
            })
            .WithName("Token")
            .WithTags("Auth")
            .Accepts<TokenRequestModel>("application/x-www-form-urlencoded")
            .Produces<TokenResponse>()
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi();

        endpointRouteBuilder.MapPost("/api/connect/logout", async (
                IMediator mediator,
                HttpRequest httpRequest,
                HttpResponse httpResponse,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(
                    new LogoutCommand.Request(RefreshTokenCookie.Read(httpRequest)),
                    cancellationToken);

                RefreshTokenCookie.Delete(httpResponse);

                return result;
            })
            .WithName("Logout")
            .WithTags("Auth")
            .WithOpenApi();
    }

    public record TokenRequestModel
    {
        [JsonPropertyName("grant_type")] public required string GrantType { get; init; }
        [JsonPropertyName("client_id")] public string? ClientId { get; init; }
    }

    public record TokenResponse
    {
        [JsonPropertyName("access_token")] public required string AccessToken { get; init; }
        [JsonPropertyName("token_type")] public string TokenType { get; init; } = "Bearer";
        [JsonPropertyName("expires_in")] public required int ExpiresIn { get; init; }
    }
}
