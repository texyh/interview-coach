using FluentValidation;

namespace AIPA.Application.Users.Commands.CreateUser;

/// <summary>
/// Validator for CreateUserCommand following FluentValidation best practices
/// </summary>
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .MaximumLength(255)
            .WithMessage("Email must not exceed 255 characters.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.");

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

        // Custom validation rules
        RuleFor(x => x)
            .Must(HaveValidNameCombination)
            .WithMessage("First name and last name cannot be identical.")
            .WithName("User");
    }

    private static bool HaveValidNameCombination(CreateUserCommand command)
    {
        return !string.Equals(command.FirstName?.Trim(), command.LastName?.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }
}
