using OneOf;
using TemplateProject.Application.Common.Errors;

namespace TemplateProject.Application.Common.Models;

[GenerateOneOf]
public partial class BaseApiResponse<T> : OneOfBase<ApiErrors.BaseApiError, T>, IBaseApiResponse
{
    public bool IsSuccess => IsT1;

    public object? GetContent()
    {
        return Match<object?>(
            _ => null,
            value => value
        );
    }
    
    public ApiErrors.BaseApiError? GetError()
    {
        return Match<ApiErrors.BaseApiError?>(
            error => error,
            _ => null
        );
    }
}

public interface IBaseApiResponse
{
    bool IsSuccess { get; }
    object? GetContent();
    ApiErrors.BaseApiError? GetError();
}

[GenerateOneOf]
public partial class ErrorOr<T> : OneOfBase<BaseError, T>;

[GenerateOneOf]
public partial class ApiErrorOr<T> : OneOfBase<ApiErrors.BaseApiError, T>;

public class ErrorOrNone : ErrorOr<Unit>
{
    public ErrorOrNone(Unit unit) : base(unit)
    {
    }

    public ErrorOrNone(BaseError error) : base(error)
    {
    }

    public static implicit operator ErrorOrNone(Unit _) => new(Unit.Value);

    public static implicit operator ErrorOrNone(BaseError error) => new(error);

    public bool IsError => IsT0;
    public BaseError Error => AsT0;
}

public class ApiErrorOrNone : ApiErrorOr<Unit>
{
    public ApiErrorOrNone(Unit unit) : base(unit)
    {
    }

    public ApiErrorOrNone(ApiErrors.BaseApiError error) : base(error)
    {
    }

    public static implicit operator ApiErrorOrNone(Unit _) => new(Unit.Value);

    public static implicit operator ApiErrorOrNone(ApiErrors.BaseApiError error) => new(error);

    public bool IsError => IsT0;
    public ApiErrors.BaseApiError Error => AsT0;
}
