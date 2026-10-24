using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using MediatR;
using TemplateProject.Application.Common.Abstractions;
using TemplateProject.Application.Common.Errors;
using TemplateProject.Application.Common.Models;

namespace TemplateProject.Application.Features.Auth;

[PublicAPI]
public static class LoginCommand
{
    public record Response
    {
        public required string UserName { get; set; }
        public required string Token { get; set; }
        public required string Status { get; set; }
    }

    /// <summary>
    /// Response — тело HTTP-ответа. Refresh-токен в тело не попадает: Api кладёт его в HttpOnly-cookie.
    /// </summary>
    public record Result(Response Response, IssuedRefreshToken RefreshToken);

    public record Request : IRequest<BaseApiResponse<Result>>
    {
        [Required(AllowEmptyStrings = false)] public string Login { get; set; }
        [Required(AllowEmptyStrings = false)] public string Password { get; set; }

        public Request(string login, string password)
        {
            Login = login;
            Password = password;
        }
    }

    public class Handler : IRequestHandler<Request, BaseApiResponse<Result>>
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

        public async Task<BaseApiResponse<Result>> Handle(Request request, CancellationToken cancellationToken)
        {
            if (request.Login == "" || request.Password == "")
            {
                return ApiErrors.BadRequest.Instance;
            }

            var user = await _identityService.FindByUserNameAsync(request.Login);

            if (user is null)
            {
                return ApiErrors.Unauthorized.Instance;
            }

            var checkPasswordResult = await _identityService.CheckPasswordSignInAsync(user.Id, request.Password);

            if (!checkPasswordResult)
            {
                return ApiErrors.Unauthorized.Instance;
            }

            var checkEmailConfirmation = await _identityService.IsEmailConfirmedAsync(user.Id);

            if (!checkEmailConfirmation)
            {
                return ApiErrors.Forbidden.Instance;
            }

            var roles = await _identityService.GetRolesAsync(user.Id);

            var response = new Response
            {
                UserName = user.Id.ToString(),
                Token = _jwtTokenGenerator.GenerateToken(user.Id, user.Email, roles).Value,
                Status = "Ok"
            };

            var refreshToken = _refreshTokenService.Issue(user.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new Result(response, refreshToken);
        }
    }
}
