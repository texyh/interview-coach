using FluentValidation;

namespace AIPA.Application.Users.Queries.GetUserByEmail;

/// <summary>
/// Validator for GetUserByEmailQuery
/// </summary>
public class GetUserByEmailQueryValidator : AbstractValidator<GetUserByEmailQuery>
{
    public GetUserByEmailQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address");
    }
}
