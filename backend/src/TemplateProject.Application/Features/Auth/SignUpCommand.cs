using JetBrains.Annotations;
using MediatR;
using TemplateProject.Application.Common.Abstractions;
using TemplateProject.Application.Common.Errors;
using TemplateProject.Application.Common.Models;
using TemplateProject.Domain.Constants;

namespace TemplateProject.Application.Features.Auth;

[PublicAPI]
public static class SignUpCommand
{
    public record Request : IRequest<BaseApiResponse<Response>>
    {
        public required string Email { get; init; }
        public required string Password { get; init; }
        public string? PhoneNumber { get; init; }
        public required string Name { get; init; }
        public int Age { get; init; }
    }

    public record Response
    {
        public required SignUpResponseData Data { get; set; }
        public required string Status { get; set; }
    }

    public class SignUpResponseData
    {
        public string Message { get; set; } = "Sign up successful";
    }

    public class Handler : IRequestHandler<Request, BaseApiResponse<Response>>
    {
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public Handler(IIdentityService identityService, TimeProvider timeProvider, IUnitOfWork unitOfWork)
        {
            _identityService = identityService;
            _timeProvider = timeProvider;
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseApiResponse<Response>> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = new CreateUserModel
            {
                UserName = request.Email,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Name = request.Name,
                Age = request.Age,
                RegistrationDate = _timeProvider.GetUtcNow().UtcDateTime,
                EmailConfirmed = true, // Временная затычка, чтобы не реализовывать подтверждение почты сейчас
            };

            var result = await _identityService.CreateUserAsync(user, request.Password);

            if (result.IsT0)
            {
                return ApiErrors.BadRequest.Instance;
            }

            await _identityService.AddToRoleAsync(result.AsT1, Roles.User);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new Response { Data = new SignUpResponseData(), Status = "Ok" };
        }
    }
}
