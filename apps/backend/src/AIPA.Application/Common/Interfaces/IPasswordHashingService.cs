namespace AIPA.Application.Common.Interfaces;

/// <summary>
/// Service for password hashing and verification
/// </summary>
public interface IPasswordHashingService
{
    /// <summary>
    /// Hashes a password using a secure algorithm
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hash
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="hash">Hashed password</param>
    /// <returns>True if password matches hash</returns>
    bool VerifyPassword(string password, string hash);
}
