using Microsoft.AspNetCore.Mvc;
using TemplateProject.Application.Common.Errors;

namespace TemplateProject.Api.Filters;

/// <summary>
/// Преобразует ошибки уровня Application в HTTP-ответы.
/// </summary>
public static class ApiErrorResultMapper
{
#pragma warning disable CS0612 // UserAlreadyExists помечен [Obsolete], но маппинг для него сохраняем
    public static int GetStatusCode(this ApiErrors.BaseApiError error) => error switch
    {
        ApiErrors.BadRequest => StatusCodes.Status400BadRequest,
        ApiErrors.ValidationError => StatusCodes.Status400BadRequest,
        ApiErrors.InvalidGrant => StatusCodes.Status400BadRequest,
        ApiErrors.Unauthorized => StatusCodes.Status401Unauthorized,
        ApiErrors.Forbidden => StatusCodes.Status403Forbidden,
        ApiErrors.NotFound => StatusCodes.Status404NotFound,
        ApiErrors.UserAlreadyExists => StatusCodes.Status409Conflict,
        ApiErrors.VerificationRequestTooFrequentError => StatusCodes.Status429TooManyRequests,
        _ => throw new ArgumentOutOfRangeException(nameof(error), error.GetType().Name, "Unknown api error")
    };

    public static IResult ToResult(this ApiErrors.BaseApiError error) => error switch
    {
        ApiErrors.BadRequest => Results.BadRequest(),
        ApiErrors.ValidationError validationError => Results.BadRequest(new { Errors = validationError.Validations }),
        // Формат ошибки токен-эндпоинта по OAuth 2.0 (RFC 6749, 5.2)
        ApiErrors.InvalidGrant => Results.BadRequest(new Dictionary<string, string> { ["error"] = error.Code }),
        ApiErrors.Unauthorized => Results.Unauthorized(),
        ApiErrors.Forbidden => Results.Forbid(),
        ApiErrors.NotFound => Results.NotFound(),
        ApiErrors.UserAlreadyExists => ToProblemDetails(error),
        ApiErrors.VerificationRequestTooFrequentError tooFrequentError => ToProblemDetails(
            error,
            new Dictionary<string, object?>
            {
                { "code", error.Code },
                {
                    "nextAttemptInSeconds",
                    (int)Math.Round(tooFrequentError.NextAttempt.TotalSeconds, MidpointRounding.ToPositiveInfinity)
                }
            }
        ),
        _ => throw new ArgumentOutOfRangeException(nameof(error), error.GetType().Name, "Unknown api error")
    };
#pragma warning restore CS0612

    private static IResult ToProblemDetails(ApiErrors.BaseApiError error, Dictionary<string, object?>? extensions = null)
    {
        var problem = new ProblemDetails
        {
            Type = $"urn:errors:{error.Code}",
            Status = error.GetStatusCode(),
            Extensions =
            {
                ["code"] = error.Code
            }
        };

        if (extensions is not null)
        {
            foreach (var (key, value) in extensions)
            {
                problem.Extensions[key] = value;
            }
        }

        return Results.Problem(problem);
    }
}
