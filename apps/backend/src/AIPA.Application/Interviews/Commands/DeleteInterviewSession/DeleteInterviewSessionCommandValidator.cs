using FluentValidation;

namespace AIPA.Application.Interviews.Commands.DeleteInterviewSession;

/// <summary>
/// Validator for DeleteInterviewSessionCommand
/// </summary>
public class DeleteInterviewSessionCommandValidator : AbstractValidator<DeleteInterviewSessionCommand>
{
    public DeleteInterviewSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithMessage("Session ID is required");
    }
}
