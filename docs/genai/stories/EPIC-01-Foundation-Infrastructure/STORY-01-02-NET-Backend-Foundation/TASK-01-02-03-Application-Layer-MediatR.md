# TASK-01-02-03: Application Layer with MediatR

**Story**: STORY-01-02 .NET Backend Foundation with Clean Architecture
**Epic**: EPIC-01 Foundation Infrastructure & Development Environment
**Estimated Hours**: 8 hours
**Assignee**: Backend Developer
**Sprint**: 2

## Task Description

Implement the application layer using MediatR for CQRS pattern implementation, including use cases, DTOs, validators, and mapping configurations for the AI Interview Preparation Assistant.

## Acceptance Criteria

- [ ] MediatR configured with dependency injection
- [ ] Command and Query handlers implemented
- [ ] DTOs created for request/response objects
- [ ] FluentValidation validators for all commands
- [ ] AutoMapper profiles for domain-to-DTO mapping
- [ ] Application services for complex business logic
- [ ] Pipeline behaviors for cross-cutting concerns
- [ ] Error handling and result patterns implemented

## Technical Implementation

### Command and Query Structure

```csharp
// Commands
public record CreateUserCommand(string Email, string FirstName, string LastName) : IRequest<Result<UserDto>>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<CreateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var email = Email.Create(request.Email);
            var fullName = FullName.Create(request.FirstName, request.LastName);

            // Check if user already exists
            var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (existingUser != null)
            {
                return Result<UserDto>.Failure("User with this email already exists");
            }

            var user = User.Create(email, fullName);
            await _userRepository.AddAsync(user, cancellationToken);

            var userDto = _mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain validation failed for user creation");
            return Result<UserDto>.Failure(ex.Message);
        }
    }
}

// Queries
public record GetUserByIdQuery(Guid UserId) : IRequest<Result<UserDto>>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure("User not found");
        }

        var userDto = _mapper.Map<UserDto>(user);
        return Result<UserDto>.Success(userDto);
    }
}
```

### DTOs and Mapping

```csharp
// DTOs
public record UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string DisplayName { get; init; }
    public string Status { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record InterviewSessionDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Type { get; init; }
    public string Status { get; init; }
    public DateTime ScheduledAt { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public List<InterviewQuestionDto> Questions { get; init; } = new();
}

// AutoMapper Profiles
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FullName.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.FullName.LastName))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.FullName.DisplayName))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
```

### Validation

```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-zA-Z\\s]+$").WithMessage("First name can only contain letters and spaces");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-zA-Z\\s]+$").WithMessage("Last name can only contain letters and spaces");
    }
}
```

### Pipeline Behaviors

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Any())
            {
                throw new ValidationException(failures);
            }
        }

        return await next();
    }
}
```

## Subtasks

1. **MediatR Configuration & Setup (1.5 hours)**

   - Configure MediatR with dependency injection
   - Set up pipeline behaviors for validation and logging
   - Register all handlers automatically

2. **Command Handlers Implementation (3 hours)**

   - Implement user management commands (Create, Update, Delete)
   - Implement interview session commands
   - Add proper error handling and logging

3. **Query Handlers Implementation (2 hours)**

   - Implement user queries (GetById, GetByEmail, GetAll)
   - Implement interview session queries
   - Add filtering and pagination support

4. **DTOs and Mapping Configuration (1.5 hours)**
   - Create DTOs for all domain entities
   - Configure AutoMapper profiles
   - Add custom mapping configurations for complex objects

## Definition of Done

- [ ] All command and query handlers are implemented
- [ ] FluentValidation validates all commands properly
- [ ] AutoMapper profiles map domain objects to DTOs correctly
- [ ] Pipeline behaviors handle cross-cutting concerns
- [ ] Error handling provides meaningful responses
- [ ] Unit tests cover all handlers and validators
- [ ] Integration tests verify CQRS flow
- [ ] Performance is acceptable for expected load

## Dependencies

- Domain layer implementation completed
- Repository interfaces defined
- Understanding of application use cases

## Resources

- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [CQRS Pattern Implementation](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [FluentValidation Documentation](https://fluentvalidation.net/)
