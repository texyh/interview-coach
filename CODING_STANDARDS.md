# Coding Standards and Development Guidelines

This document outlines the coding standards, development practices, and quality guidelines for the AIPA project across all platforms and technologies.

## Table of Contents

- [General Principles](#general-principles)
- [Code Formatting](#code-formatting)
- [Language-Specific Standards](#language-specific-standards)
- [Git Workflow](#git-workflow)
- [Code Review Guidelines](#code-review-guidelines)
- [Testing Standards](#testing-standards)
- [Security Guidelines](#security-guidelines)
- [Documentation Standards](#documentation-standards)

## General Principles

### Core Values

- **Consistency**: Code should be consistent across the entire codebase
- **Readability**: Code should be self-documenting and easy to understand
- **Maintainability**: Code should be easy to modify and extend
- **Security**: Security considerations should be built into all code
- **Performance**: Code should be efficient and scalable
- **Testability**: Code should be designed to be easily testable

### Naming Conventions

- Use descriptive and meaningful names
- Avoid abbreviations unless they are well-known (e.g., `url`, `id`, `api`)
- Use consistent naming patterns across the codebase
- Follow language-specific naming conventions

## Code Formatting

All code formatting is enforced through automated tools and IDE configuration:

- **EditorConfig**: Root-level formatting consistency (`.editorconfig`)
- **UTF-8 encoding** and **LF line endings** for all files
- **Trailing whitespace** is automatically removed
- **Final newlines** are enforced in all files

### Indentation Standards

- **Spaces only** (no tabs)
- **2 spaces**: JSON, YAML, TypeScript, JavaScript, HTML, CSS
- **4 spaces**: C#, Dart, Python, XML
- **Continuation lines**: Use double indentation

## Language-Specific Standards

### .NET Backend (C#)

#### Configuration

- **StyleCop Analyzers**: Comprehensive style rule enforcement
- **Roslyn Analyzers**: Advanced code analysis and security checks
- **Nullable Reference Types**: Enabled project-wide
- **Implicit Usings**: Enabled for cleaner file headers
- **Treat Warnings as Errors**: Enforced in release builds

#### Naming Conventions

```csharp
// Classes, Methods, Properties - PascalCase
public class UserService
{
    public string FirstName { get; set; }
    public void ProcessRequest() { }
}

// Fields, Parameters, Variables - camelCase
private readonly ILogger logger;
public void Method(string userName)
{
    var localVariable = "";
}

// Constants - PascalCase
public const int MaxRetryCount = 3;

// Interfaces - Prefixed with 'I'
public interface IUserRepository { }
```

#### Code Organization

- **One class per file**
- **Namespace matches folder structure**
- **Using statements** at the top of file
- **File-scoped namespaces** preferred
- **Regional organization** within classes

#### Error Handling

```csharp
// Use specific exception types
throw new ArgumentNullException(nameof(parameter));

// Implement proper logging
try
{
    // operation
}
catch (SpecificException ex)
{
    logger.LogError(ex, "Specific error occurred");
    throw; // Re-throw to preserve stack trace
}
```

### TypeScript/JavaScript (AI Service & Web App)

#### Configuration

- **ESLint**: Comprehensive linting with TypeScript support
- **Prettier**: Automated code formatting
- **Strict TypeScript**: Full type safety enforcement
- **Import organization**: Automatic import sorting

#### Type Safety

```typescript
// Always use explicit types for function signatures
function processUser(user: User): Promise<UserResponse> {
  return Promise.resolve({ id: user.id, status: "processed" });
}

// Use interfaces for object structures
interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

// Prefer union types over any
type Status = "pending" | "completed" | "failed";
```

#### Async/Await

```typescript
// Prefer async/await over promises
async function fetchUserData(id: string): Promise<User> {
  try {
    const response = await api.get(`/users/${id}`);
    return response.data;
  } catch (error) {
    logger.error("Failed to fetch user", { id, error });
    throw new UserFetchError(`Failed to fetch user ${id}`);
  }
}
```

#### Import Organization

```typescript
// 1. Node modules
import express from "express";
import { Injectable } from "@nestjs/common";

// 2. Internal modules (absolute paths)
import { UserService } from "@/services/UserService";
import { ApiResponse } from "@/types/api";

// 3. Relative imports
import "./component.styles.css";
```

### Flutter/Dart (Mobile App)

#### Configuration

- **Flutter Lints**: Comprehensive linting rules
- **Effective Dart**: Style guide compliance
- **Strong Mode**: Full type safety
- **Performance optimizations**: Enabled

#### Naming Conventions

```dart
// Classes, Enums - PascalCase
class UserRepository extends Repository {
  // Public members - camelCase
  String userName;

  // Private members - prefixed with underscore
  String _apiKey;

  // Methods - camelCase
  Future<User> fetchUser(String id) async {
    // Local variables - camelCase
    final response = await http.get(Uri.parse('$baseUrl/users/$id'));
    return User.fromJson(json.decode(response.body));
  }
}

// Constants - lowerCamelCase
const String baseUrl = 'https://api.example.com';

// Files - snake_case
// user_repository.dart
// api_service.dart
```

#### Widget Guidelines

```dart
// Prefer StatelessWidget when possible
class UserCard extends StatelessWidget {
  const UserCard({
    Key? key,
    required this.user,
    this.onTap,
  }) : super(key: key);

  final User user;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: ListTile(
        title: Text(user.name),
        onTap: onTap,
      ),
    );
  }
}
```

## Git Workflow

### Branch Naming

- **Feature branches**: `feature/TASK-XX-XX-XX-description`
- **Bug fixes**: `bugfix/issue-description`
- **Hotfixes**: `hotfix/critical-issue-description`
- **Release branches**: `release/v1.2.3`

### Commit Messages

Follow conventional commit format:

```
type(scope): brief description

Detailed explanation if needed

Fixes #123
```

**Types**: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`
**Scopes**: `backend`, `ai-service`, `web-app`, `mobile-app`, `infra`

### Pre-commit Checks

Automated checks run before each commit:

- **Code formatting** and linting
- **Build verification** for all projects
- **Test execution** (unit tests)
- **Security scanning** for sensitive data
- **File size validation**

## Code Review Guidelines

### Review Checklist

- [ ] **Functionality**: Code works as intended
- [ ] **Logic**: Algorithm and business logic are correct
- [ ] **Performance**: No obvious performance issues
- [ ] **Security**: No security vulnerabilities
- [ ] **Testing**: Adequate test coverage
- [ ] **Documentation**: Code is well-documented
- [ ] **Standards**: Follows coding standards
- [ ] **Dependencies**: No unnecessary dependencies

### Review Process

1. **Self-review** before requesting review
2. **Automated checks** must pass
3. **At least one approving review** required
4. **Address all feedback** before merging
5. **Squash commits** when merging

## Testing Standards

### Coverage Requirements

- **Unit tests**: Minimum 80% code coverage
- **Integration tests**: Critical user flows
- **E2E tests**: Main application scenarios
- **Performance tests**: Load and stress testing

### Test Organization

```
tests/
├── unit/           # Unit tests
├── integration/    # Integration tests
├── e2e/           # End-to-end tests
├── fixtures/      # Test data
└── helpers/       # Test utilities
```

### Test Naming

```csharp
// .NET
[Test]
public void ProcessUser_WithValidData_ReturnsSuccess()

// TypeScript
describe('UserService', () => {
  it('should process user with valid data successfully', () => {
    // test implementation
  });
});

// Dart
void main() {
  group('UserRepository', () => {
    test('should fetch user with valid ID', () async {
      // test implementation
    });
  });
}
```

## Security Guidelines

### Sensitive Data

- **Never commit** secrets, passwords, or API keys
- **Use environment variables** for configuration
- **Implement proper** input validation
- **Sanitize user inputs** to prevent injection attacks

### Authentication & Authorization

- **Implement proper** authentication flows
- **Use secure session** management
- **Follow principle** of least privilege
- **Validate permissions** at every endpoint

### Data Protection

- **Encrypt sensitive data** at rest and in transit
- **Implement proper** logging without exposing secrets
- **Use HTTPS** for all communications
- **Follow GDPR/privacy** guidelines

## Documentation Standards

### Code Documentation

- **XML documentation** for .NET public APIs
- **JSDoc comments** for TypeScript public functions
- **Dart documentation** for public classes and methods
- **README files** for each project component

### Documentation Format

```csharp
/// <summary>
/// Processes user registration request with validation and persistence.
/// </summary>
/// <param name="request">The user registration request containing user details.</param>
/// <param name="cancellationToken">Cancellation token for async operation.</param>
/// <returns>A task representing the registration result with user ID.</returns>
/// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
/// <exception cref="ValidationException">Thrown when user data is invalid.</exception>
public async Task<RegistrationResult> ProcessRegistrationAsync(
    UserRegistrationRequest request,
    CancellationToken cancellationToken = default)
```

````typescript
/**
 * Fetches user profile data from the API with caching support.
 *
 * @param userId - Unique identifier for the user
 * @param useCache - Whether to use cached data if available
 * @returns Promise resolving to user profile data
 * @throws {UserNotFoundError} When user doesn't exist
 * @throws {ApiError} When API request fails
 *
 * @example
 * ```typescript
 * const user = await fetchUserProfile('123', true);
 * console.log(user.name);
 * ```
 */
async function fetchUserProfile(
  userId: string,
  useCache = true
): Promise<UserProfile>;
````

### Project Documentation

- **Architecture decisions** in ADR format
- **API documentation** with OpenAPI/Swagger
- **Deployment guides** with step-by-step instructions
- **Troubleshooting guides** for common issues

## IDE and Tool Configuration

### VS Code Extensions

Essential extensions for development:

- **Language Support**: C#, TypeScript, Dart/Flutter
- **Linting**: ESLint, StyleCop, Dart analyzer
- **Formatting**: Prettier, C# formatter, Dart formatter
- **Testing**: Jest Runner, .NET Test Explorer
- **Azure**: Azure tools for deployment and monitoring
- **Git**: Git Graph, GitLens for version control

### EditorConfig Support

All IDEs should respect the `.editorconfig` settings:

- Consistent indentation and formatting
- Proper line ending handling
- Character encoding standardization
- Trailing whitespace management

## Enforcement

These standards are enforced through:

- **Automated linting** and formatting tools
- **Pre-commit hooks** preventing non-compliant code
- **CI/CD pipelines** with quality gates
- **Code review** process with mandatory checks
- **IDE configuration** providing real-time feedback

## Updates and Maintenance

This document should be:

- **Reviewed regularly** and updated as needed
- **Discussed in team meetings** for any changes
- **Versioned** with the codebase
- **Accessible** to all team members

---

_Last updated: [Current Date]_
_Version: 1.0_
