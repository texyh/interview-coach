using AIPA.Domain.Common.Exceptions;

namespace AIPA.Domain.ValueObjects;

public record FullName
{
    public string FirstName { get; }
    public string LastName { get; }
    public string DisplayName => $"{FirstName} {LastName}";

    private FullName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static FullName Create(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new BusinessRuleValidationException("First name cannot be empty");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new BusinessRuleValidationException("Last name cannot be empty");

        return new FullName(firstName.Trim(), lastName.Trim());
    }
}
