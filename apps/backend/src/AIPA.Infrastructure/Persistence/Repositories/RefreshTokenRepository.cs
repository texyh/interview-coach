using AIPA.Domain.Users.Entities;
using AIPA.Domain.Users.Repositories;
using AIPA.Domain.ValueObjects;
using AIPA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIPA.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of refresh token repository
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a refresh token by its value
    /// </summary>
    /// <param name="token">Token value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Refresh token if found</returns>
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    /// <summary>
    /// Gets all active refresh tokens for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active refresh tokens</returns>
    public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Adds a new refresh token
    /// </summary>
    /// <param name="refreshToken">Refresh token to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    /// <summary>
    /// Updates a refresh token
    /// </summary>
    /// <param name="refreshToken">Refresh token to update</param>
    public void Update(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
    }

    /// <summary>
    /// Removes a refresh token
    /// </summary>
    /// <param name="refreshToken">Refresh token to remove</param>
    public void Remove(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Remove(refreshToken);
    }

    /// <summary>
    /// Revokes all refresh tokens for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task RevokeAllUserTokensAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke();
        }
    }

    /// <summary>
    /// Removes expired refresh tokens
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task RemoveExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        _context.RefreshTokens.RemoveRange(expiredTokens);
    }
}
