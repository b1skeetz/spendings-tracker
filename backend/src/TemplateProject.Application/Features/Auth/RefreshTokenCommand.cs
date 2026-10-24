using JetBrains.Annotations;
using MediatR;
using TemplateProject.Application.Common.Abstractions;
using TemplateProject.Application.Common.Errors;
using TemplateProject.Application.Common.Models;

namespace TemplateProject.Application.Features.Auth;

[PublicAPI]
public static class RefreshTokenCommand
{
    public record Request(string? RefreshToken) : IRequest<BaseApiResponse<Response>>;

    public record Response(AccessToken AccessToken, IssuedRefreshToken RefreshToken);

    public class Handler : IRequestHandler<Request, BaseApiResponse<Response>>
    {
        private readonly IIdentityService _identityService;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(
            IIdentityService identityService,
            IJwtTokenGenerator jwtTokenGenerator,
            IRefreshTokenService refreshTokenService,
            IUnitOfWork unitOfWork)
        {
            _identityService = identityService;
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenService = refreshTokenService;
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseApiResponse<Response>> Handle(Request request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return ApiErrors.InvalidGrant.Instance;
            }

            var redeemResult = await _refreshTokenService.RedeemAsync(request.RefreshToken, cancellationToken);

            // Сохраняем даже при ошибке: при повторном использовании токена сервис отзывает все токены пользователя
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (redeemResult.IsT0)
            {
                return ApiErrors.InvalidGrant.Instance;
            }

            var user = await _identityService.FindByIdAsync(redeemResult.AsT1);

            if (user is null || !await _identityService.IsEmailConfirmedAsync(user.Id))
            {
                return ApiErrors.InvalidGrant.Instance;
            }

            var roles = await _identityService.GetRolesAsync(user.Id);

            var accessToken = _jwtTokenGenerator.GenerateToken(user.Id, user.Email, roles);
            var refreshToken = _refreshTokenService.Issue(user.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new Response(accessToken, refreshToken);
        }
    }
}
