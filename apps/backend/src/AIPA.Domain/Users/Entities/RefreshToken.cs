using AIPA.Domain.Common;
using AIPA.Domain.ValueObjects;

namespace AIPA.Domain.Users.Entities;

/// <summary>
/// Represents a refresh token for JWT authentication
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>
    /// Unique identifier for the refresh token
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The refresh token value
    /// </summary>
    public string Token { get; private set; } = string.Empty;

    /// <summary>
    /// User ID that owns this refresh token
    /// </summary>
    public UserId UserId { get; private set; } = null!;

    /// <summary>
    /// User that owns this refresh token
    /// </summary>
    public User User { get; private set; } = null!;

    /// <summary>
    /// When the refresh token expires
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Whether the refresh token has been revoked
    /// </summary>
    public bool IsRevoked { get; private set; }

    /// <summary>
    /// When the refresh token was revoked (if applicable)
    /// </summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>
    /// Whether the refresh token is active (not expired and not revoked)
    /// </summary>
    public bool IsActive => !IsRevoked && ExpiresAt > DateTime.UtcNow;

    /// <summary>
    /// Creates a new refresh token
    /// </summary>
    /// <param name="token">Token value</param>
    /// <param name="userId">User ID</param>
    /// <param name="expiresAt">Expiration date</param>
    /// <returns>New refresh token</returns>
    public static RefreshToken Create(string token, UserId userId, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiration date must be in the future", nameof(expiresAt));

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = userId,
            ExpiresAt = expiresAt,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Revokes the refresh token
    /// </summary>
    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Private constructor for EF Core
    private RefreshToken() { }
}
