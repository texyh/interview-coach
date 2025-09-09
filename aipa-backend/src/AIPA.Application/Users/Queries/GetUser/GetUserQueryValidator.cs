using FluentValidation;

namespace AIPA.Application.Users.Queries.GetUser;

/// <summary>
/// Validator for GetUserQuery
/// </summary>
public class GetUserQueryValidator : AbstractValidator<GetUserQuery>
{
    public GetUserQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}
