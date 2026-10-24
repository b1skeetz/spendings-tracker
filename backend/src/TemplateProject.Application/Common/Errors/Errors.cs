namespace TemplateProject.Application.Common.Errors;

public class BaseError;

public class Errors
{
    public class NotificationSendingFailed : BaseError
    {
        public static NotificationSendingFailed Instance { get; } = new();
    }

    public class RefreshTokenInvalid : BaseError
    {
        public static RefreshTokenInvalid Instance { get; } = new();
    }

    public class IdentityOperationFailed(IReadOnlyCollection<string> errors) : BaseError
    {
        public IReadOnlyCollection<string> Errors { get; } = errors;
    }
}
