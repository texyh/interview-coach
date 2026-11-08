using FluentValidation;

namespace AIPA.Application.Users.Commands.UpdateUser;

/// <summary>
/// Validator for UpdateUserCommand
/// </summary>
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.")
            .NotEqual(Guid.Empty)
            .WithMessage("User ID must be a valid GUID.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required.")
            .MaximumLength(100)
            .WithMessage("First name must not exceed 100 characters.")
            .Matches(@"^[a-zA-ZÀ-ÿ\u0100-\u017F\u1E00-\u1EFF\s'-]+$")
            .WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required.")
            .MaximumLength(100)
            .WithMessage("Last name must not exceed 100 characters.")
            .Matches(@"^[a-zA-ZÀ-ÿ\u0100-\u017F\u1E00-\u1EFF\s'-]+$")
            .WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes.");

        // Profile validation when provided
        When(x => x.Profile != null, () =>
        {
            RuleFor(x => x.Profile!.Bio)
                .MaximumLength(500)
                .WithMessage("Bio must not exceed 500 characters.");

            RuleFor(x => x.Profile!.PhoneNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .WithMessage("Phone number must be in a valid international format.")
                .When(x => !string.IsNullOrWhiteSpace(x.Profile?.PhoneNumber));

            RuleFor(x => x.Profile!.LinkedInUrl)
                .Must(BeValidLinkedInUrl)
                .WithMessage("LinkedIn URL must be a valid LinkedIn profile URL.")
                .When(x => !string.IsNullOrWhiteSpace(x.Profile?.LinkedInUrl));

            RuleFor(x => x.Profile!.GitHubUrl)
                .Must(BeValidGitHubUrl)
                .WithMessage("GitHub URL must be a valid GitHub profile URL.")
                .When(x => !string.IsNullOrWhiteSpace(x.Profile?.GitHubUrl));
        });

        // Custom validation rules
        RuleFor(x => x)
            .Must(HaveValidNameCombination)
            .WithMessage("First name and last name cannot be identical.")
            .WithName("User");
    }

    private static bool HaveValidNameCombination(UpdateUserCommand command)
    {
        return !string.Equals(command.FirstName?.Trim(), command.LastName?.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool BeValidLinkedInUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               uri.Host.EndsWith("linkedin.com", StringComparison.OrdinalIgnoreCase);
    }

    private static bool BeValidGitHubUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               uri.Host.EndsWith("github.com", StringComparison.OrdinalIgnoreCase);
    }
}
