using FluentValidation;

namespace AIPA.Application.Interviews.Commands.CreateInterviewSession;

/// <summary>
/// Validator for CreateInterviewSessionCommand
/// </summary>
public class CreateInterviewSessionCommandValidator : AbstractValidator<CreateInterviewSessionCommand>
{
    public CreateInterviewSessionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ScheduledAt)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Scheduled time must be in the future");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .WithMessage("Duration must be greater than 0")
            .LessThanOrEqualTo(480) // 8 hours max
            .WithMessage("Duration must not exceed 8 hours (480 minutes)");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid interview type");

        RuleFor(x => x.Difficulty)
            .IsInEnum()
            .WithMessage("Invalid difficulty level");
    }
}
