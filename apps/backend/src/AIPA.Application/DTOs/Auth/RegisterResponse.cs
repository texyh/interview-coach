namespace AIPA.Application.DTOs.Auth;

/// <summary>
/// Response model for user registration
/// </summary>
public class RegisterResponse
{
    /// <summary>
    /// Unique identifier of the registered user
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's full name
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Whether email verification is required
    /// </summary>
    public bool EmailVerificationRequired { get; set; } = true;

    /// <summary>
    /// Message to display to the user
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the user was registered
    /// </summary>
    public DateTime RegisteredAt { get; set; }
}
