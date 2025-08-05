# TASK-01-02-04: Infrastructure Layer & Data Access

**Story**: STORY-01-02 .NET Backend Foundation with Clean Architecture
**Epic**: EPIC-01 Foundation Infrastructure & Development Environment
**Estimated Hours**: 10 hours
**Assignee**: Backend Developer
**Sprint**: 2

## Task Description

Implement the infrastructure layer with Entity Framework Core, repository implementations, database context configuration, and external service integrations for the AI Interview Preparation Assistant.

## Acceptance Criteria

- [ ] DbContext configured with PostgreSQL provider
- [ ] Entity configurations and mappings implemented
- [ ] Repository pattern implementations for all aggregates
- [ ] Database migrations created and tested
- [ ] Connection string management and health checks
- [ ] Unit of Work pattern implemented
- [ ] Database seeding for development environment
- [ ] External service integrations abstracted

## Technical Implementation

### DbContext Configuration

```csharp
public class AipaDbContext : DbContext
{
    public AipaDbContext(DbContextOptions<AipaDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<InterviewSession> InterviewSessions => Set<InterviewSession>();
    public DbSet<InterviewQuestion> InterviewQuestions => Set<InterviewQuestion>();
    public DbSet<UserResponse> UserResponses => Set<UserResponse>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AipaDbContext).Assembly);

        // Configure schema
        modelBuilder.HasDefaultSchema("aipa");

        // Configure naming conventions
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.GetTableName()?.ToSnakeCase());

            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.GetColumnName().ToSnakeCase());
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Handle domain events
        var domainEvents = ChangeTracker.Entries<BaseEntity>()
            .Select(x => x.Entity)
            .Where(x => x.DomainEvents.Any())
            .SelectMany(x => x.DomainEvents)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch domain events after successful save
        foreach (var domainEvent in domainEvents)
        {
            // Domain event dispatcher will be implemented later
        }

        return result;
    }
}
```

### Entity Configurations

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(
                id => id.Value,
                value => new UserId(value))
            .ValueGeneratedNever();

        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

            email.HasIndex(e => e.Value)
                .IsUnique()
                .HasDatabaseName("ix_users_email");
        });

        builder.OwnsOne(u => u.FullName, fullName =>
        {
            fullName.Property(fn => fn.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(100)
                .IsRequired();

            fullName.Property(fn => fn.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Configure navigation properties
        builder.HasMany<InterviewSession>()
            .WithOne()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events for EF
        builder.Ignore(u => u.DomainEvents);
    }
}

public class InterviewSessionConfiguration : IEntityTypeConfiguration<InterviewSession>
{
    public void Configure(EntityTypeBuilder<InterviewSession> builder)
    {
        builder.ToTable("interview_sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(
                id => id.Value,
                value => new InterviewSessionId(value))
            .ValueGeneratedNever();

        builder.Property(s => s.UserId)
            .HasConversion(
                id => id.Value,
                value => new UserId(value));

        builder.Property(s => s.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasMany<InterviewQuestion>()
            .WithOne()
            .HasForeignKey(q => q.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(s => s.DomainEvents);
    }
}
```

### Repository Implementations

```csharp
public class UserRepository : IUserRepository
{
    private readonly AipaDbContext _context;

    public UserRepository(AipaDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .OrderBy(u => u.FullName.LastName)
            .ThenBy(u => u.FullName.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(UserId id, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        if (user != null)
        {
            _context.Users.Remove(user);
        }
    }
}
```

### Unit of Work Pattern

```csharp
public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IInterviewSessionRepository InterviewSessions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync();
}

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly AipaDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AipaDbContext context, IUserRepository users, IInterviewSessionRepository interviewSessions)
    {
        _context = context;
        Users = users;
        InterviewSessions = interviewSessions;
    }

    public IUserRepository Users { get; }
    public IInterviewSessionRepository InterviewSessions { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
    }
}
```

## Subtasks

1. **DbContext and Entity Configuration (3 hours)**

   - Configure DbContext with PostgreSQL
   - Implement entity type configurations
   - Set up naming conventions and schema

2. **Repository Implementations (3 hours)**

   - Implement repository pattern for all aggregates
   - Add query optimization and includes
   - Implement pagination and filtering

3. **Migrations and Database Setup (2 hours)**

   - Create initial database migration
   - Set up database seeding for development
   - Configure connection string management

4. **Unit of Work and Transaction Management (2 hours)**
   - Implement Unit of Work pattern
   - Add transaction management
   - Configure domain event handling

## Definition of Done

- [ ] DbContext is properly configured and working
- [ ] All entity configurations map domain objects correctly
- [ ] Repository implementations follow defined interfaces
- [ ] Database migrations create proper schema
- [ ] Unit of Work pattern manages transactions properly
- [ ] Database seeding works for development environment
- [ ] Connection health checks are implemented
- [ ] Integration tests verify data access functionality

## Dependencies

- Domain layer implementation completed
- Repository interfaces defined
- PostgreSQL database design completed

## Resources

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Repository Pattern with EF Core](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-implementation-entity-framework-core)
