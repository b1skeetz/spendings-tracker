using JetBrains.Annotations;
using MediatR;
using TemplateProject.Application.Common.Abstractions;
using TemplateProject.Application.Common.Models;

namespace TemplateProject.Application.Features.Auth;

[PublicAPI]
public static class LogoutCommand
{
    public record Request(string? RefreshToken) : IRequest<BaseApiResponse<Response>>;

    public record Response
    {
        public required string Status { get; set; }
    }

    public class Handler : IRequestHandler<Request, BaseApiResponse<Response>>
    {
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IRefreshTokenService refreshTokenService, IUnitOfWork unitOfWork)
        {
            _refreshTokenService = refreshTokenService;
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseApiResponse<Response>> Handle(Request request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                await _refreshTokenService.RevokeAsync(request.RefreshToken, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return new Response { Status = "Ok" };
        }
    }
}
