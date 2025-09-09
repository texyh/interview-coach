using System.ComponentModel.DataAnnotations;

namespace AIPA.Application.DTOs.Auth;

/// <summary>
/// Request model for forgot password (initiating password reset)
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
}
