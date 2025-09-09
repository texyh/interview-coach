using System.ComponentModel.DataAnnotations;

namespace AIPA.Application.DTOs.Auth;

/// <summary>
/// Request model for refreshing access tokens
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// The refresh token
    /// </summary>
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Request model for user logout
/// </summary>
public class LogoutRequest
{
    /// <summary>
    /// The refresh token to invalidate
    /// </summary>
    public string? RefreshToken { get; set; }
}
