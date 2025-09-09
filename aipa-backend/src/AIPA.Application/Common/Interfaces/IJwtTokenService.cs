using System.Security.Claims;
using AIPA.Domain.Users.Entities;

namespace AIPA.Application.Common.Interfaces;

/// <summary>
/// Service for JWT token operations
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token for a user
    /// </summary>
    /// <param name="user">User entity</param>
    /// <returns>JWT access token</returns>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a refresh token
    /// </summary>
    /// <returns>Refresh token string</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a JWT token and returns claims principal
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>Claims principal if valid, null otherwise</returns>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Gets the user ID from a JWT token
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>User ID if valid, null otherwise</returns>
    string? GetUserIdFromToken(string token);

    /// <summary>
    /// Checks if a token is expired
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>True if token is expired</returns>
    bool IsTokenExpired(string token);
}
