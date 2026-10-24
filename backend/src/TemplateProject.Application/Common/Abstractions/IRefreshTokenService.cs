using TemplateProject.Application.Common.Models;

namespace TemplateProject.Application.Common.Abstractions;

/// <summary>
/// Выпуск и погашение refresh-токенов. Методы только изменяют состояние,
/// сохранение выполняется через <see cref="IUnitOfWork"/>.
/// </summary>
public interface IRefreshTokenService
{
    IssuedRefreshToken Issue(Guid userId);

    /// <summary>
    /// Проверяет токен и отзывает его (токен одноразовый). Возвращает Id владельца.
    /// Повторное предъявление уже отозванного токена считается утечкой:
    /// отзываются все активные токены пользователя.
    /// </summary>
    Task<ErrorOr<Guid>> RedeemAsync(string token, CancellationToken cancellationToken);

    Task RevokeAsync(string token, CancellationToken cancellationToken);
}
