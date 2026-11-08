using System.ComponentModel.DataAnnotations;
using AIPA.Domain.Common.Exceptions;

namespace AIPA.Domain.ValueObjects;

public record Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new BusinessRuleValidationException("Email cannot be empty");

        if (!IsValidEmail(email))
            throw new BusinessRuleValidationException("Invalid email format");

        return new Email(email.ToLowerInvariant());
    }

    private static bool IsValidEmail(string email)
    {
        return new EmailAddressAttribute().IsValid(email);
    }

    public static implicit operator string(Email email) => email.Value;
}
