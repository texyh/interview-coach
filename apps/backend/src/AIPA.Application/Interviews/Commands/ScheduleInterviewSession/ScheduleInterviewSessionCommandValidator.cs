using FluentValidation;

namespace AIPA.Application.Interviews.Commands.ScheduleInterviewSession;

/// <summary>
/// Validator for ScheduleInterviewSessionCommand
/// </summary>
public class ScheduleInterviewSessionCommandValidator : AbstractValidator<ScheduleInterviewSessionCommand>
{
    public ScheduleInterviewSessionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.")
            .NotEqual(Guid.Empty)
            .WithMessage("User ID must be a valid GUID.");

        RuleFor(x => x.InterviewType)
            .NotEmpty()
            .WithMessage("Interview type is required.")
            .Must(BeValidInterviewType)
            .WithMessage("Interview type must be one of: Technical, Behavioral, System Design, Coding Challenge.");

        RuleFor(x => x.ScheduledAt)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Scheduled time must be in the future.")
            .LessThan(DateTime.UtcNow.AddMonths(6))
            .WithMessage("Scheduled time cannot be more than 6 months from now.");

        // Custom validation for business hours
        RuleFor(x => x.ScheduledAt)
            .Must(BeInBusinessHours)
            .WithMessage("Interview must be scheduled during business hours (9 AM - 6 PM UTC).");
    }

    private static bool BeValidInterviewType(string interviewType)
    {
        var validTypes = new[] { "Technical", "Behavioral", "System Design", "Coding Challenge" };
        return validTypes.Contains(interviewType, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeInBusinessHours(DateTime scheduledAt)
    {
        // Check if the time is between 9 AM and 6 PM UTC
        var hour = scheduledAt.Hour;
        return hour >= 9 && hour < 18;
    }
}
