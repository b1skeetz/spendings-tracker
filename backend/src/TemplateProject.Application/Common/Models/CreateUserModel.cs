namespace TemplateProject.Application.Common.Models;

public record CreateUserModel
{
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Name { get; init; }
    public int? Age { get; init; }
    public DateTime RegistrationDate { get; init; }
    public bool EmailConfirmed { get; init; }
}
