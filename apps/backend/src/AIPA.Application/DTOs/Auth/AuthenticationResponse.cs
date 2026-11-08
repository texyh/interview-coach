using AIPA.Domain.Enums;

namespace AIPA.Application.DTOs.Auth;

/// <summary>
/// Response model for successful authentication
/// </summary>
public class AuthenticationResponse
{
    /// <summary>
    /// JWT access token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token for obtaining new access tokens
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in UTC
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// User information
    /// </summary>
    public UserInfo User { get; set; } = null!;
}

/// <summary>
/// User information included in authentication response
/// </summary>
public class UserInfo
{
    /// <summary>
    /// User's unique identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's full name
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// User's account status
    /// </summary>
    public AccountStatus Status { get; set; }

    /// <summary>
    /// User's roles
    /// </summary>
    public List<string> Roles { get; set; } = [];

    /// <summary>
    /// Whether the user's email is verified
    /// </summary>
    public bool IsEmailVerified { get; set; }

    /// <summary>
    /// User's profile information
    /// </summary>
    public UserProfileInfo? Profile { get; set; }
}

/// <summary>
/// User profile information
/// </summary>
public class UserProfileInfo
{
    /// <summary>
    /// User's job title
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// User's company
    /// </summary>
    public string? Company { get; set; }

    /// <summary>
    /// Years of experience
    /// </summary>
    public int? YearsOfExperience { get; set; }
}
