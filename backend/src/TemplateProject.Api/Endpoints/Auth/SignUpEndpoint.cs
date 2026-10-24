using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TemplateProject.Application.Features.Auth;

namespace TemplateProject.Api.Endpoints.Auth;

[PublicAPI]
public class SignUpEndpoint : IFeatureEndpoint
{
    public void Map(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("/api/v1/auth/sign-up", async (
                    IMediator mediator,
                    CancellationToken cancellationToken,
                    [FromBody] SignUpModel model) =>
                await mediator.Send(new SignUpCommand.Request
                {
                    Email = model.Email,
                    Password = model.Password,
                    PhoneNumber = model.PhoneNumber,
                    Name = model.Name,
                    Age = model.Age,
                }, cancellationToken))
            .WithName("SignUp")
            .WithTags("Auth")
            .WithOpenApi();
    }

    public class SignUpModel
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? PhoneNumber { get; set; }
        public required string Name { get; set; }
        public int Age { get; set; }
    }
}
