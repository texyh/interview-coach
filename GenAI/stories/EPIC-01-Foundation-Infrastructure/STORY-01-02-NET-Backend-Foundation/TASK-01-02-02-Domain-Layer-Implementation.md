# TASK-01-02-02: Domain Layer Implementation

**Story**: STORY-01-02 .NET Backend Foundation with Clean Architecture
**Epic**: EPIC-01 Foundation Infrastructure & Development Environment
**Estimated Hours**: 6 hours
**Assignee**: Senior Backend Developer
**Sprint**: 1

## Task Description

Implement the domain layer with core entities, value objects, domain services, and repository interfaces following Domain-Driven Design principles for the AI Interview Preparation Assistant.

## Acceptance Criteria

- [ ] Core domain entities implemented with proper encapsulation
- [ ] Value objects created for domain concepts
- [ ] Domain services for business logic that doesn't belong to entities
- [ ] Repository interfaces defined for data access abstraction
- [ ] Domain events implemented for cross-boundary communication
- [ ] Domain exceptions with meaningful error messages
- [ ] Business rules validated within domain objects

## Technical Implementation

### Core Domain Entities

```csharp
// User aggregate root
public class User : BaseEntity
{
    public UserId Id { get; private set; }
    public Email Email { get; private set; }
    public FullName FullName { get; private set; }
    public UserProfile Profile { get; private set; }
    public AccountStatus Status { get; private set; }

    private readonly List<InterviewSession> _interviewSessions = new();
    public IReadOnlyCollection<InterviewSession> InterviewSessions => _interviewSessions.AsReadOnly();

    private User() { } // For EF Core

    public static User Create(Email email, FullName fullName)
    {
        var user = new User
        {
            Id = UserId.New(),
            Email = email,
            FullName = fullName,
            Status = AccountStatus.Active,
            Profile = UserProfile.CreateDefault()
        };

        user.AddDomainEvent(new UserCreatedEvent(user.Id, user.Email));
        return user;
    }

    public void UpdateProfile(UserProfile profile)
    {
        Profile = profile ?? throw new ArgumentNullException(nameof(profile));
        AddDomainEvent(new UserProfileUpdatedEvent(Id, Profile));
    }
}

// InterviewSession aggregate root
public class InterviewSession : BaseEntity
{
    public InterviewSessionId Id { get; private set; }
    public UserId UserId { get; private set; }
    public InterviewType Type { get; private set; }
    public SessionStatus Status { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private readonly List<InterviewQuestion> _questions = new();
    public IReadOnlyCollection<InterviewQuestion> Questions => _questions.AsReadOnly();

    public static InterviewSession Schedule(UserId userId, InterviewType type, DateTime scheduledAt)
    {
        if (scheduledAt <= DateTime.UtcNow)
            throw new DomainException("Interview cannot be scheduled in the past");

        var session = new InterviewSession
        {
            Id = InterviewSessionId.New(),
            UserId = userId,
            Type = type,
            Status = SessionStatus.Scheduled,
            ScheduledAt = scheduledAt
        };

        session.AddDomainEvent(new InterviewSessionScheduledEvent(session.Id, userId, scheduledAt));
        return session;
    }
}
```

### Value Objects

```csharp
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
            throw new DomainException("Email cannot be empty");

        if (!IsValidEmail(email))
            throw new DomainException("Invalid email format");

        return new Email(email.ToLowerInvariant());
    }

    private static bool IsValidEmail(string email)
    {
        // Email validation logic
        return new EmailAddressAttribute().IsValid(email);
    }
}

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
            throw new DomainException("First name cannot be empty");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name cannot be empty");

        return new FullName(firstName.Trim(), lastName.Trim());
    }
}
```

### Repository Interfaces

```csharp
public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(UserId id, CancellationToken cancellationToken = default);
}

public interface IInterviewSessionRepository
{
    Task<InterviewSession?> GetByIdAsync(InterviewSessionId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<InterviewSession>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task AddAsync(InterviewSession session, CancellationToken cancellationToken = default);
    Task UpdateAsync(InterviewSession session, CancellationToken cancellationToken = default);
}
```

## Subtasks

1. **Core Entities Implementation (2.5 hours)**

   - Implement User and InterviewSession aggregate roots
   - Add proper encapsulation and business methods
   - Create entity base classes with common functionality

2. **Value Objects Creation (1.5 hours)**

   - Implement Email, FullName, and other value objects
   - Add validation logic and factory methods
   - Ensure immutability and proper equality

3. **Repository Interfaces (1 hour)**

   - Define repository contracts for each aggregate
   - Include async patterns and cancellation tokens
   - Add query methods for common access patterns

4. **Domain Events & Exceptions (1 hour)**
   - Implement domain event base classes
   - Create specific domain events for business operations
   - Define domain exceptions with meaningful messages

## Definition of Done

- [ ] All domain entities are properly encapsulated
- [ ] Value objects are immutable and validated
- [ ] Repository interfaces follow SOLID principles
- [ ] Domain events are implemented for all business operations
- [ ] Domain exceptions provide clear error messages
- [ ] Business rules are enforced within domain objects
- [ ] Unit tests cover all domain logic
- [ ] Code follows DDD patterns and conventions

## Dependencies

- Domain model requirements and business rules analysis
- Initial project structure and dependencies setup
- Understanding of business processes and workflows

## Resources

- [Domain-Driven Design Reference](https://domainlanguage.com/ddd/reference/)
- [.NET Domain Model Implementation](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/)
