# TASK-01-02-05: API Layer & Controllers Foundation

**Story**: STORY-01-02 .NET Backend Foundation with Clean Architecture
**Epic**: EPIC-01 Foundation Infrastructure & Development Environment
**Estimated Hours**: 6 hours
**Assignee**: Backend Developer
**Sprint**: 2

## Task Description

Implement the API layer with controllers, routing, OpenAPI documentation, and proper HTTP response handling following RESTful principles and Clean Architecture patterns.

## Acceptance Criteria

- [ ] Base controller class with common functionality
- [ ] RESTful API controllers for core entities
- [ ] Proper HTTP status codes and response formats
- [ ] OpenAPI/Swagger documentation configured
- [ ] API versioning implemented
- [ ] Request/response DTOs properly mapped
- [ ] Error handling with consistent response format
- [ ] CORS configuration for development

## Technical Implementation

### Base Controller

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected readonly IMediator Mediator;
    protected readonly ILogger Logger;

    protected BaseController(IMediator mediator, ILogger logger)
    {
        Mediator = mediator;
        Logger = logger;
    }

    protected ActionResult<T> HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(new ApiResponse<T>
            {
                Success = true,
                Data = result.Value,
                Message = result.Message
            });
        }

        Logger.LogWarning("Request failed: {Error}", result.Error);

        return result.Error switch
        {
            "Not found" or "User not found" => NotFound(new ApiResponse<T>
            {
                Success = false,
                Error = result.Error,
                Message = result.Message
            }),
            var error when error.Contains("validation") => BadRequest(new ApiResponse<T>
            {
                Success = false,
                Error = result.Error,
                Message = result.Message
            }),
            _ => StatusCode(500, new ApiResponse<T>
            {
                Success = false,
                Error = "Internal server error",
                Message = "An unexpected error occurred"
            })
        };
    }

    protected ActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok(new ApiResponse
            {
                Success = true,
                Message = result.Message
            });
        }

        return HandleErrorResult(result);
    }

    private ActionResult HandleErrorResult(Result result)
    {
        Logger.LogWarning("Request failed: {Error}", result.Error);

        return result.Error switch
        {
            "Not found" => NotFound(new ApiResponse
            {
                Success = false,
                Error = result.Error,
                Message = result.Message
            }),
            var error when error.Contains("validation") => BadRequest(new ApiResponse
            {
                Success = false,
                Error = result.Error,
                Message = result.Message
            }),
            _ => StatusCode(500, new ApiResponse
            {
                Success = false,
                Error = "Internal server error",
                Message = "An unexpected error occurred"
            })
        };
    }
}
```

### Users Controller

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class UsersController : BaseController
{
    public UsersController(IMediator mediator, ILogger<UsersController> logger)
        : base(mediator, logger) { }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <returns>List of users</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetAllUsers()
    {
        var query = new GetAllUsersQuery();
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById([FromRoute] Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="request">User creation request</param>
    /// <returns>Created user</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] CreateUserRequest request)
    {
        var command = new CreateUserCommand(request.Email, request.FirstName, request.LastName);
        var result = await Mediator.Send(command);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetUserById),
                new { id = result.Value.Id },
                new ApiResponse<UserDto>
                {
                    Success = true,
                    Data = result.Value,
                    Message = "User created successfully"
                });
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Update user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">User update request</param>
    /// <returns>Updated user</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser([FromRoute] Guid id, [FromBody] UpdateUserRequest request)
    {
        var command = new UpdateUserCommand(id, request.FirstName, request.LastName);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse>> DeleteUser([FromRoute] Guid id)
    {
        var command = new DeleteUserCommand(id);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}
```

### Request/Response Models

```csharp
// Request models
public record CreateUserRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; init; } = string.Empty;
}

public record UpdateUserRequest
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; init; } = string.Empty;
}

// Response models
public record ApiResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? Error { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public record ApiResponse<T> : ApiResponse
{
    public T? Data { get; init; }
}

public record PaginatedResponse<T> : ApiResponse<IEnumerable<T>>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
```

### API Configuration

```csharp
public static class ApiConfiguration
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<GlobalExceptionFilter>();
            options.Filters.Add<ValidationFilter>();
        });

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Version"),
                new QueryStringApiVersionReader("version")
            );
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "AIPA API",
                Version = "v1",
                Description = "AI Interview Preparation Assistant API",
                Contact = new OpenApiContact
                {
                    Name = "AIPA Team",
                    Email = "dev@aipa.com"
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter JWT token"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
```

## Subtasks

1. **Base Controller and Common Response Handling (1.5 hours)**

   - Implement base controller with common functionality
   - Create standard response models and error handling
   - Set up consistent HTTP status code mapping

2. **Users Controller Implementation (2 hours)**

   - Implement full CRUD operations for users
   - Add proper routing and parameter validation
   - Configure OpenAPI documentation attributes

3. **API Configuration and Versioning (1.5 hours)**

   - Set up API versioning strategy
   - Configure OpenAPI/Swagger documentation
   - Add CORS configuration for development

4. **Request/Response Models and Validation (1 hour)**
   - Create request DTOs with validation attributes
   - Implement response wrapper models
   - Add pagination support for list endpoints

## Definition of Done

- [ ] All API endpoints follow RESTful conventions
- [ ] Proper HTTP status codes are returned
- [ ] OpenAPI documentation is complete and accurate
- [ ] Request validation works correctly
- [ ] Error responses have consistent format
- [ ] API versioning functions properly
- [ ] CORS is configured for development
- [ ] Integration tests verify API functionality

## Dependencies

- Application layer with MediatR handlers completed
- Domain and infrastructure layers implemented
- Understanding of API design standards

## Resources

- [ASP.NET Core Web API Documentation](https://docs.microsoft.com/en-us/aspnet/core/web-api/)
- [RESTful API Design Best Practices](https://docs.microsoft.com/en-us/azure/architecture/best-practices/api-design)
- [OpenAPI Specification](https://swagger.io/specification/)
