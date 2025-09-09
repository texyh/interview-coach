using AIPA.Application.Common.Interfaces;
using AIPA.Application.DTOs.Auth;
using AIPA.Domain.Enums;
using AIPA.Domain.Users.Entities;
using AIPA.Domain.Users.Repositories;
using AIPA.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
namespace AIPA.Infrastructure.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly int _refreshTokenExpirationDays;

    public AuthenticationService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHashingService passwordHashingService,
        IUnitOfWork unitOfWork,
        ILogger<AuthenticationService> logger,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHashingService = passwordHashingService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _refreshTokenExpirationDays = int.Parse(configuration["Jwt:RefreshTokenExpiryDays"] ?? "30");
    }

    /// <summary>
    /// Authenticates a user with email and password
    /// </summary>
    /// <param name="request">Authentication request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with tokens</returns>
    public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Authentication attempt for email: {Email}", request.Email);

        // Find user by email
        var emailValueObject = Email.Create(request.Email);
        var user = await _userRepository.GetByEmailAsync(emailValueObject, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Authentication failed: User not found for email: {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Check if account is active
        if (user.Status != AccountStatus.Active)
        {
            _logger.LogWarning("Authentication failed: Account not active for user: {UserId}, Status: {Status}",
                user.Id.Value, user.Status);
            throw new UnauthorizedAccessException("Account is not active");
        }

        // Check if account is locked
        if (user.IsAccountLocked())
        {
            _logger.LogWarning("Authentication failed: Account is locked for user: {UserId}", user.Id.Value);
            throw new UnauthorizedAccessException("Account is temporarily locked");
        }

        // Verify password
        if (!ValidatePassword(user, request.Password))
        {
            user.RecordFailedLogin();
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("Authentication failed: Invalid password for user: {UserId}", user.Id.Value);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Record successful login
        user.RecordSuccessfulLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        // Create refresh token entity
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(request.RememberMe ? _refreshTokenExpirationDays * 2 : _refreshTokenExpirationDays);
        var refreshToken = RefreshToken.Create(refreshTokenValue, user.Id, refreshTokenExpiration);

        // Save refresh token
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Authentication successful for user: {UserId}", user.Id.Value);

        return new AuthenticationResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            User = new UserInfo
            {
                Id = user.Id.Value.ToString(),
                Email = user.Email.Value,
                FullName = $"{user.FullName.FirstName} {user.FullName.LastName}",
                Status = user.Status,
                Roles = user.UserRoles?.Select(ur => ur.Role?.Name ?? string.Empty).ToList() ?? new List<string>(),
                IsEmailVerified = true, // Assuming verified for now
                Profile = new UserProfileInfo
                {
                    JobTitle = user.Profile?.JobTitle,
                    Company = user.Profile?.Company,
                    YearsOfExperience = user.Profile?.YearsOfExperience
                }
            }
        };
    }

    /// <summary>
    /// Refreshes an access token using a refresh token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New authentication response with tokens</returns>
    public async Task<AuthenticationResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Refresh token attempt");

        // Find refresh token
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (refreshToken == null || !refreshToken.IsActive)
        {
            _logger.LogWarning("Refresh token failed: Invalid or expired token");
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        var user = refreshToken.User;
        if (user.Status != AccountStatus.Active)
        {
            _logger.LogWarning("Refresh token failed: Account not active for user: {UserId}, Status: {Status}",
                user.Id.Value, user.Status);
            throw new UnauthorizedAccessException("Account is not active");
        }

        // Generate new tokens
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        // Revoke old refresh token
        refreshToken.Revoke();
        _refreshTokenRepository.Update(refreshToken);

        // Create new refresh token
        var newRefreshToken = RefreshToken.Create(newRefreshTokenValue, user.Id, DateTime.UtcNow.AddDays(_refreshTokenExpirationDays));
        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Token refresh successful for user: {UserId}", user.Id.Value);

        return new AuthenticationResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenValue,
            User = new UserInfo
            {
                Id = user.Id.Value.ToString(),
                Email = user.Email.Value,
                FullName = $"{user.FullName.FirstName} {user.FullName.LastName}",
                Status = user.Status,
                Roles = user.UserRoles?.Select(ur => ur.Role?.Name ?? string.Empty).ToList() ?? new List<string>(),
                IsEmailVerified = true,
                Profile = new UserProfileInfo
                {
                    JobTitle = user.Profile?.JobTitle,
                    Company = user.Profile?.Company,
                    YearsOfExperience = user.Profile?.YearsOfExperience
                }
            }
        };
    }

    /// <summary>
    /// Logs out a user by invalidating their refresh token
    /// </summary>
    /// <param name="request">Logout request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if logout was successful</returns>
    public async Task<bool> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Logout attempt");

        try
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
            if (refreshToken != null)
            {
                refreshToken.Revoke();
                _refreshTokenRepository.Update(refreshToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Logout successful for user: {UserId}", refreshToken.UserId.Value);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return false;
        }
    }

    /// <summary>
    /// Validates a user's password
    /// </summary>
    /// <param name="user">User entity</param>
    /// <param name="password">Password to validate</param>
    /// <returns>True if password is valid</returns>
    public bool ValidatePassword(User user, string password)
    {
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
            return false;

        return _passwordHashingService.VerifyPassword(password, user.PasswordHash);
    }
}
