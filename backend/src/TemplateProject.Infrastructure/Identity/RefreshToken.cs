namespace TemplateProject.Infrastructure.Identity;

/// <summary>
/// Refresh-токен пользователя. Сам токен в БД не хранится, только его SHA-256 хэш.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string TokenHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
