namespace AIPA.Domain.Exceptions;

/// <summary>
/// Base exception for domain-specific errors
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleValidationException : DomainException
{
    public BusinessRuleValidationException(string rule, string message)
        : base($"Business rule '{rule}' violated: {message}")
    {
        Rule = rule;
    }

    public string Rule { get; }
}

/// <summary>
/// Exception thrown when an entity is not found
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object id)
        : base($"{entityName} with id '{id}' was not found")
    {
        EntityName = entityName;
        Id = id;
    }

    public string EntityName { get; }
    public object Id { get; }
}

/// <summary>
/// Exception thrown when attempting to create a duplicate entity
/// </summary>
public class DuplicateEntityException : DomainException
{
    public DuplicateEntityException(string entityName, string field, object value)
        : base($"{entityName} with {field} '{value}' already exists")
    {
        EntityName = entityName;
        Field = field;
        Value = value;
    }

    public string EntityName { get; }
    public string Field { get; }
    public object Value { get; }
}
