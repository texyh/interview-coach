namespace AIPA.Application.Common.Interfaces;

/// <summary>
/// Interface for email service operations
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email verification email to the user
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="fullName">User's full name</param>
    /// <param name="verificationToken">Email verification token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if email was sent successfully</returns>
    Task<bool> SendEmailVerificationAsync(string email, string fullName, string verificationToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password reset email to the user
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="fullName">User's full name</param>
    /// <param name="resetToken">Password reset token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if email was sent successfully</returns>
    Task<bool> SendPasswordResetEmailAsync(string email, string fullName, string resetToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a welcome email to newly registered users
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="fullName">User's full name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if email was sent successfully</returns>
    Task<bool> SendWelcomeEmailAsync(string email, string fullName, CancellationToken cancellationToken = default);
}
