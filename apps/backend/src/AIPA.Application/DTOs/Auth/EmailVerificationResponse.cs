namespace AIPA.Application.DTOs.Auth;

/// <summary>
/// Response model for email verification
/// </summary>
public class EmailVerificationResponse
{
    /// <summary>
    /// Whether the verification was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message to display to the user
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the email was verified
    /// </summary>
    public DateTime? VerifiedAt { get; set; }
}
