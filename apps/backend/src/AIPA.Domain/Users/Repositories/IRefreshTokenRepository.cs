using AIPA.Domain.Users.Entities;
using AIPA.Domain.ValueObjects;

namespace AIPA.Domain.Users.Repositories;

/// <summary>
/// Repository for refresh token operations
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Gets a refresh token by its value
    /// </summary>
    /// <param name="token">Token value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Refresh token if found</returns>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active refresh tokens for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active refresh tokens</returns>
    Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new refresh token
    /// </summary>
    /// <param name="refreshToken">Refresh token to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a refresh token
    /// </summary>
    /// <param name="refreshToken">Refresh token to update</param>
    void Update(RefreshToken refreshToken);

    /// <summary>
    /// Removes a refresh token
    /// </summary>
    /// <param name="refreshToken">Refresh token to remove</param>
    void Remove(RefreshToken refreshToken);

    /// <summary>
    /// Revokes all refresh tokens for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RevokeAllUserTokensAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes expired refresh tokens
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveExpiredTokensAsync(CancellationToken cancellationToken = default);
}
