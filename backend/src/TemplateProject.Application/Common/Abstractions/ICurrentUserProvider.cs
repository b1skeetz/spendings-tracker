namespace TemplateProject.Application.Common.Abstractions;

public interface ICurrentUserProvider
{
    Guid? GetCurrentUserId();
    void SetCurrentUserId(Guid userId);
}
