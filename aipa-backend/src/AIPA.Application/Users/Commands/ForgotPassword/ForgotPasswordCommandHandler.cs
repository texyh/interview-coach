using AIPA.Application.Common.Interfaces;
using AIPA.Domain.Users.Repositories;
using AIPA.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIPA.Application.Users.Commands.ForgotPassword;

/// <summary>
/// Handler for forgot password command
/// </summary>
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing forgot password request for email: {Email}", request.Email);

        try
        {
            // Create email value object
            var email = Email.Create(request.Email);

            // Find user by email
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Forgot password attempted for non-existent user: {Email}", request.Email);
                // For security reasons, we don't reveal if the email exists or not
                return true;
            }

            // Generate password reset token
            user.GeneratePasswordResetToken();

            // Update user in repository
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send password reset email
            await _emailService.SendPasswordResetEmailAsync(
                user.Email.Value,
                user.FullName.DisplayName,
                user.PasswordResetToken!,
                cancellationToken);

            _logger.LogInformation("Password reset email sent successfully for user: {Email}", request.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password process for {Email}", request.Email);
            return false;
        }
    }
}
