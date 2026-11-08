using FluentValidation;
using AIPA.Domain.Interviews.Enums;

namespace AIPA.Application.Interviews.Commands.UpdateInterviewSession;

/// <summary>
/// Validator for UpdateInterviewSessionCommand
/// </summary>
public class UpdateInterviewSessionCommandValidator : AbstractValidator<UpdateInterviewSessionCommand>
{
    public UpdateInterviewSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithMessage("Session ID is required");

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
            .WithMessage("Scheduled time must be in the future")
            .When(x => x.Status != InterviewSessionStatus.Completed && x.Status != InterviewSessionStatus.Cancelled);

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

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid interview session status");
    }
}
