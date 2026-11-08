using FluentValidation;

namespace AIPA.Application.Users.Queries.GetUserById;

/// <summary>
/// Validator for GetUserByIdQuery
/// </summary>
public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.")
            .NotEqual(Guid.Empty)
            .WithMessage("User ID must be a valid GUID.");
    }
}
