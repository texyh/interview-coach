using System.ComponentModel.DataAnnotations;

namespace AIPA.Application.DTOs.Auth;

/// <summary>
/// Request model for user authentication
/// </summary>
public class AuthenticationRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Whether to remember the user for longer token expiration
    /// </summary>
    public bool RememberMe { get; set; } = false;
}
