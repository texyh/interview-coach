# Security Coding Guidelines - AIPA Platform

This document outlines security best practices and coding guidelines for the AIPA Interview Preparation Platform. All developers must follow these guidelines to ensure secure code development.

## Table of Contents

- [Input Validation](#input-validation)
- [Authentication & Authorization](#authentication--authorization)
- [Data Protection](#data-protection)
- [API Security](#api-security)
- [Error Handling](#error-handling)
- [Database Security](#database-security)
- [File Upload Security](#file-upload-security)
- [Frontend Security](#frontend-security)
- [Mobile App Security](#mobile-app-security)
- [Infrastructure Security](#infrastructure-security)

## Input Validation

### General Principles

- **Always validate and sanitize user input** on both client and server sides
- **Never trust client-side validation alone** - always validate on the server
- **Use whitelist validation** instead of blacklist when possible
- **Validate data type, length, format, and range**

### .NET Backend (C#)

```csharp
// Use Data Annotations for model validation
public class UserRegistrationModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8-100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]",
        ErrorMessage = "Password must contain uppercase, lowercase, number and special character")]
    public string Password { get; set; }
}

// Use parameterized queries to prevent SQL injection
public async Task<User> GetUserByIdAsync(int userId)
{
    const string query = "SELECT * FROM Users WHERE Id = @UserId";
    return await _context.Database.QueryFirstOrDefaultAsync<User>(query, new { UserId = userId });
}

// Validate and sanitize file paths
public bool IsValidFilePath(string filePath)
{
    if (string.IsNullOrWhiteSpace(filePath))
        return false;

    var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt" };
    var extension = Path.GetExtension(filePath).ToLowerInvariant();

    return allowedExtensions.Contains(extension) &&
           !filePath.Contains("..") &&
           !Path.IsPathRooted(filePath);
}
```

### NestJS/Node.js (TypeScript)

```typescript
// Use class-validator for input validation
import {
  IsEmail,
  IsString,
  MinLength,
  MaxLength,
  Matches,
} from "class-validator";

export class CreateUserDto {
  @IsEmail({}, { message: "Please provide a valid email address" })
  @MaxLength(255, { message: "Email must not exceed 255 characters" })
  email: string;

  @IsString()
  @MinLength(8, { message: "Password must be at least 8 characters long" })
  @MaxLength(100, { message: "Password must not exceed 100 characters" })
  @Matches(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]/, {
    message:
      "Password must contain uppercase, lowercase, number and special character",
  })
  password: string;
}

// Sanitize user input
import * as validator from "validator";

export function sanitizeInput(input: string): string {
  return validator.escape(validator.trim(input));
}

// Validate file uploads
export function validateFileUpload(file: Express.Multer.File): boolean {
  const allowedTypes = ["application/pdf", "audio/mpeg", "audio/wav"];
  const maxSize = 50 * 1024 * 1024; // 50MB

  return allowedTypes.includes(file.mimetype) && file.size <= maxSize;
}
```

### Next.js/React (TypeScript)

```typescript
// Use Zod for schema validation
import { z } from "zod";

const userRegistrationSchema = z.object({
  email: z.string().email("Invalid email format").max(255, "Email too long"),
  password: z
    .string()
    .min(8, "Password must be at least 8 characters")
    .max(100, "Password too long")
    .regex(
      /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]/,
      "Password must meet complexity requirements"
    ),
});

// Sanitize HTML content to prevent XSS
import DOMPurify from "dompurify";

export function sanitizeHtml(html: string): string {
  return DOMPurify.sanitize(html, {
    ALLOWED_TAGS: ["b", "i", "em", "strong", "p", "br"],
    ALLOWED_ATTR: [],
  });
}

// Validate API responses
export function validateApiResponse<T>(
  data: unknown,
  schema: z.ZodSchema<T>
): T {
  const result = schema.safeParse(data);
  if (!result.success) {
    throw new Error("Invalid API response format");
  }
  return result.data;
}
```

## Authentication & Authorization

### JWT Token Security

```csharp
// .NET - Secure JWT configuration
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Configuration["Jwt:Issuer"],
                ValidAudience = Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(Configuration["Jwt:SecretKey"])),
                ClockSkew = TimeSpan.Zero // Remove default 5-minute tolerance
            };
        });
}

// Generate secure JWT tokens
public string GenerateJwtToken(User user)
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Role, user.Role)
    };

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(60), // Short expiry
        signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### Password Security

```csharp
// .NET - Secure password hashing with bcrypt
public string HashPassword(string password)
{
    return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
}

public bool VerifyPassword(string password, string hashedPassword)
{
    return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
}
```

```typescript
// Node.js - Secure password hashing
import * as bcrypt from "bcrypt";

export async function hashPassword(password: string): Promise<string> {
  const saltRounds = 12;
  return bcrypt.hash(password, saltRounds);
}

export async function verifyPassword(
  password: string,
  hashedPassword: string
): Promise<boolean> {
  return bcrypt.compare(password, hashedPassword);
}
```

## Data Protection

### Encryption at Rest and in Transit

```csharp
// .NET - Data encryption
using System.Security.Cryptography;

public class EncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        _key = Convert.FromBase64String(configuration["Encryption:Key"]);
    }

    public string Encrypt(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plaintext);
        }

        var iv = aes.IV;
        var encrypted = msEncrypt.ToArray();
        var result = new byte[iv.Length + encrypted.Length];
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);

        return Convert.ToBase64String(result);
    }
}
```

### Secure Data Storage

```csharp
// .NET - Database configuration for sensitive data
public class ApplicationDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Encrypt sensitive fields
        modelBuilder.Entity<User>()
            .Property(u => u.PersonalData)
            .HasConversion(
                v => _encryptionService.Encrypt(v),
                v => _encryptionService.Decrypt(v));

        // Index non-sensitive searchable fields only
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}
```

## API Security

### Rate Limiting

```csharp
// .NET - Configure rate limiting
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
    services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
    services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
    services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
    services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
    services.AddInMemoryRateLimiting();
}
```

```typescript
// NestJS - Rate limiting with guards
import { Injectable, CanActivate, ExecutionContext } from "@nestjs/common";
import { Reflector } from "@nestjs/core";

@Injectable()
export class RateLimitGuard implements CanActivate {
  constructor(private reflector: Reflector) {}

  async canActivate(context: ExecutionContext): Promise<boolean> {
    const request = context.switchToHttp().getRequest();
    const key = `rate_limit_${request.ip}`;

    // Implement rate limiting logic using Redis or memory cache
    const currentRequests = await this.getCurrentRequestCount(key);
    const limit =
      this.reflector.get<number>("rateLimit", context.getHandler()) || 100;

    if (currentRequests >= limit) {
      throw new HttpException(
        "Too many requests",
        HttpStatus.TOO_MANY_REQUESTS
      );
    }

    await this.incrementRequestCount(key);
    return true;
  }
}
```

### CORS Configuration

```csharp
// .NET - Secure CORS setup
public void ConfigureServices(IServiceCollection services)
{
    services.AddCors(options =>
    {
        options.AddPolicy("AllowedOrigins", builder =>
        {
            builder
                .WithOrigins(Configuration.GetSection("AllowedOrigins").Get<string[]>())
                .AllowedHeaders("Content-Type", "Authorization", "X-Requested-With")
                .WithMethods("GET", "POST", "PUT", "DELETE")
                .AllowCredentials()
                .SetPreflightMaxAge(TimeSpan.FromMinutes(5));
        });
    });
}
```

### API Key Security

```typescript
// NestJS - API key authentication
import {
  Injectable,
  CanActivate,
  ExecutionContext,
  UnauthorizedException,
} from "@nestjs/common";

@Injectable()
export class ApiKeyGuard implements CanActivate {
  canActivate(context: ExecutionContext): boolean {
    const request = context.switchToHttp().getRequest();
    const apiKey = request.headers["x-api-key"];

    if (!apiKey) {
      throw new UnauthorizedException("API key is required");
    }

    // Validate API key against secure storage
    if (!this.isValidApiKey(apiKey)) {
      throw new UnauthorizedException("Invalid API key");
    }

    return true;
  }

  private isValidApiKey(apiKey: string): boolean {
    // Hash the provided key and compare with stored hashes
    const hashedKey = this.hashApiKey(apiKey);
    return this.storedApiKeyHashes.includes(hashedKey);
  }
}
```

## Error Handling

### Secure Error Messages

```csharp
// .NET - Generic error responses
[ApiController]
public class BaseController : ControllerBase
{
    protected IActionResult HandleException(Exception ex)
    {
        // Log the full exception details for debugging
        _logger.LogError(ex, "An error occurred: {Message}", ex.Message);

        // Return generic error message to client
        return StatusCode(500, new {
            message = "An internal error occurred. Please try again later.",
            errorId = Guid.NewGuid().ToString() // For tracking
        });
    }
}
```

### Structured Security Logging

```typescript
// NestJS - Security event logging
import { Injectable, Logger } from "@nestjs/common";

@Injectable()
export class SecurityLogger {
  private readonly logger = new Logger("Security");

  logAuthenticationFailure(email: string, ip: string, userAgent: string) {
    this.logger.warn("Authentication failure", {
      event: "auth_failure",
      email: this.hashEmail(email), // Hash PII
      ip,
      userAgent,
      timestamp: new Date().toISOString(),
    });
  }

  logSuspiciousActivity(userId: string, activity: string, metadata: any) {
    this.logger.error("Suspicious activity detected", {
      event: "suspicious_activity",
      userId,
      activity,
      metadata,
      timestamp: new Date().toISOString(),
    });
  }
}
```

## File Upload Security

```typescript
// File upload validation and security
export class FileSecurityService {
  private readonly allowedMimeTypes = [
    "application/pdf",
    "audio/mpeg",
    "audio/wav",
    "audio/mp4",
    "text/plain",
  ];

  private readonly maxFileSize = 50 * 1024 * 1024; // 50MB

  validateFile(file: Express.Multer.File): ValidationResult {
    // Check file size
    if (file.size > this.maxFileSize) {
      return { valid: false, error: "File size exceeds limit" };
    }

    // Check MIME type
    if (!this.allowedMimeTypes.includes(file.mimetype)) {
      return { valid: false, error: "File type not allowed" };
    }

    // Check for malicious content
    if (this.containsMaliciousContent(file)) {
      return {
        valid: false,
        error: "File contains potentially malicious content",
      };
    }

    return { valid: true };
  }

  private containsMaliciousContent(file: Express.Multer.File): boolean {
    // Implement virus scanning, malware detection
    // Check file headers match extensions
    // Scan for embedded scripts in documents
    return false;
  }

  sanitizeFileName(fileName: string): string {
    // Remove dangerous characters
    return fileName
      .replace(/[^a-zA-Z0-9.-]/g, "_")
      .replace(/_{2,}/g, "_")
      .substring(0, 100); // Limit length
  }
}
```

## Security Testing Requirements

### Unit Test Examples

```csharp
// .NET - Security unit tests
[TestClass]
public class AuthenticationSecurityTests
{
    [TestMethod]
    public async Task Login_WithInvalidCredentials_ShouldNotRevealUserExistence()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "nonexistent@test.com", Password = "wrong" };

        // Act
        var result = await _authController.Login(loginRequest);

        // Assert
        Assert.AreEqual("Invalid email or password", result.Message);
        // Should not reveal whether email exists or password is wrong
    }

    [TestMethod]
    public void GenerateJwtToken_ShouldHaveSecureExpiry()
    {
        // Act
        var token = _authService.GenerateJwtToken(new User());
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        // Assert
        Assert.IsTrue(jsonToken.ValidTo <= DateTime.UtcNow.AddHours(1));
        Assert.IsTrue(jsonToken.ValidFrom <= DateTime.UtcNow);
    }
}
```

```typescript
// NestJS - Security unit tests
describe("PasswordSecurityService", () => {
  it("should hash passwords with sufficient complexity", async () => {
    const password = "TestPassword123!";
    const hashedPassword = await passwordService.hashPassword(password);

    expect(hashedPassword).not.toBe(password);
    expect(hashedPassword.length).toBeGreaterThan(50);
    expect(hashedPassword).toMatch(/^\$2[aby]\$/); // bcrypt format
  });

  it("should reject weak passwords", () => {
    const weakPasswords = ["123456", "password", "admin", "qwerty"];

    weakPasswords.forEach((password) => {
      expect(() => passwordService.validatePasswordStrength(password)).toThrow(
        "Password does not meet security requirements"
      );
    });
  });
});
```

## Mobile App Security (Flutter)

```dart
// Flutter - Secure storage and communication
class SecurityService {
  static const _storage = FlutterSecureStorage();

  // Secure token storage
  static Future<void> storeToken(String token) async {
    await _storage.write(
      key: 'jwt_token',
      value: token,
      aOptions: AndroidOptions(
        encryptedSharedPreferences: true,
      ),
      iOptions: IOSOptions(
        accessibility: IOSAccessibility.first_unlock_this_device,
      ),
    );
  }

  // Certificate pinning for API calls
  static HttpClient createSecureHttpClient() {
    final client = HttpClient();
    client.badCertificateCallback = (cert, host, port) {
      // Implement certificate pinning
      return _validateCertificate(cert, host);
    };
    return client;
  }

  // Biometric authentication
  static Future<bool> authenticateWithBiometrics() async {
    final localAuth = LocalAuthentication();
    try {
      return await localAuth.authenticate(
        localizedReason: 'Please authenticate to access the app',
        options: AuthenticationOptions(
          biometricOnly: true,
          stickyAuth: true,
        ),
      );
    } catch (e) {
      return false;
    }
  }
}
```

## Security Checklist

### Code Review Security Checklist

- [ ] **Input Validation**: All user inputs are validated and sanitized
- [ ] **Authentication**: Proper authentication mechanisms implemented
- [ ] **Authorization**: Access controls are correctly enforced
- [ ] **Data Protection**: Sensitive data is encrypted at rest and in transit
- [ ] **Error Handling**: No sensitive information leaked in error messages
- [ ] **Logging**: Security events are properly logged without exposing PII
- [ ] **Dependencies**: No known vulnerable dependencies
- [ ] **Configuration**: Security headers and settings are configured
- [ ] **File Uploads**: File uploads are properly validated and secured
- [ ] **API Security**: Rate limiting, CORS, and API authentication implemented
- [ ] **Database**: SQL injection prevention measures in place
- [ ] **Secrets Management**: No hardcoded secrets or credentials
- [ ] **Session Management**: Secure session handling implemented
- [ ] **Cross-Site Protection**: CSRF and XSS protection measures implemented

### Deployment Security Checklist

- [ ] **HTTPS**: All communications use TLS 1.2 or higher
- [ ] **Security Headers**: All required security headers are configured
- [ ] **Environment Variables**: Secrets are stored in secure configuration
- [ ] **Database Security**: Database connections are encrypted and secured
- [ ] **Monitoring**: Security monitoring and alerting is configured
- [ ] **Backup Security**: Backups are encrypted and access-controlled
- [ ] **Infrastructure**: Security groups and network isolation configured
- [ ] **Updates**: Security patches and updates are applied
- [ ] **Penetration Testing**: Security testing has been performed
- [ ] **Incident Response**: Security incident response plan is in place

## Additional Resources

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [NIST Cybersecurity Framework](https://www.nist.gov/cyberframework)
- [Microsoft Security Development Lifecycle](https://www.microsoft.com/en-us/securityengineering/sdl)
- [ASP.NET Core Security Best Practices](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [Node.js Security Best Practices](https://nodejs.org/en/docs/guides/security/)
- [React Security Best Practices](https://snyk.io/blog/10-react-security-best-practices/)
- [Flutter Security Best Practices](https://flutter.dev/docs/deployment/security)

---

**Document Version**: 1.0
**Last Updated**: August 6, 2025
**Review Schedule**: Monthly security guideline reviews
**Contact**: security-team@aipa.local
