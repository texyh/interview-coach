using AIPA.Application.Common.Interfaces;
using AIPA.Domain.Users.Repositories;
using AIPA.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIPA.Application.Users.Commands.ResetPassword;

/// <summary>
/// Handler for reset password command
/// </summary>
public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHashingService passwordHashingService,
        IUnitOfWork unitOfWork,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHashingService = passwordHashingService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing password reset for email: {Email}", request.Email);

        try
        {
            // Create email value object
            var email = Email.Create(request.Email);

            // Find user by email
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Password reset attempted for non-existent user: {Email}", request.Email);
                return false;
            }

            // Validate the reset token
            if (!user.IsPasswordResetTokenValid(request.Token))
            {
                _logger.LogWarning("Invalid password reset token for user: {Email}", request.Email);
                return false;
            }

            // Hash the new password
            var passwordHash = _passwordHashingService.HashPassword(request.Password);

            // Reset the password
            user.ResetPassword(request.Token, passwordHash);

            // Update user in repository
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password reset successfully for user: {Email}", request.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset for {Email}", request.Email);
            return false;
        }
    }
}
