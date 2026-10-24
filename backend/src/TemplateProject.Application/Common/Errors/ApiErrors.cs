using JetBrains.Annotations;

namespace TemplateProject.Application.Common.Errors;

/// <summary>
/// Ошибки, которые use case'ы возвращают клиенту API.
/// Преобразование в HTTP-ответ выполняется на уровне Api (см. ApiErrorResultMapper).
/// </summary>
public static class ApiErrors
{
    public static class Codes
    {
        public const string UserNotExists = "user_not_exists";
        public const string TokenIsEmpty = "token_is_empty";
        public const string TooManyIncorrectCodeAttempts = "too_many_incorrect_code_attempts";
        public const string VerificationRequestTooFrequent = "verification_request_too_frequent";
    }

    [PublicAPI]
    public abstract class BaseApiError : BaseError
    {
        public abstract string Code { get; }
    }

    public class BadRequest : BaseApiError
    {
        public static BadRequest Instance { get; } = new();
        public override string Code => "bad_request";
    }

    public class Forbidden : BaseApiError
    {
        public static Forbidden Instance { get; } = new();
        public override string Code => "forbidden";
    }

    public class NotFound : BaseApiError
    {
        public static NotFound Instance { get; } = new();
        public override string Code => "not_found";
    }

    [Obsolete]
    public class UserAlreadyExists : BaseApiError
    {
        public static UserAlreadyExists Instance { get; } = new();
        public override string Code => "user_already_exists";
    }

    [PublicAPI]
    public class ValidationError : BaseApiError
    {
        public ValidationError(params string[] validations)
        {
            Validations = validations;
        }

        public IReadOnlyCollection<string> Validations { get; }
        public override string Code => "validation_error";
    }

    [PublicAPI]
    public class VerificationRequestTooFrequentError(TimeSpan nextAttempt) : BaseApiError
    {
        public TimeSpan NextAttempt { get; } = nextAttempt;
        public override string Code => Codes.VerificationRequestTooFrequent;
    }

    public class Unauthorized : BaseApiError
    {
        public static Unauthorized Instance { get; } = new();
        public override string Code => "unauthorized";
    }

    /// <summary>
    /// Refresh-токен отсутствует, просрочен, отозван или не найден (OAuth 2.0 invalid_grant).
    /// </summary>
    public class InvalidGrant : BaseApiError
    {
        public static InvalidGrant Instance { get; } = new();
        public override string Code => "invalid_grant";
    }
}
