using FluentValidation;

namespace AIPA.Application.Users.Queries.GetUsers;

/// <summary>
/// Validator for GetUsersQuery
/// </summary>
public class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .WithMessage("Search term must not exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm));

        RuleFor(x => x.Status)
            .Must(BeValidStatus)
            .WithMessage("Status must be either 'Active' or 'Inactive'.")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));
    }

    private static bool BeValidStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return true;

        var validStatuses = new[] { "Active", "Inactive" };
        return validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
    }
}
