using AIPA.Application.Common.Interfaces;
using AIPA.Domain.Enums;
using AIPA.Domain.Users.Entities;
using AIPA.Domain.ValueObjects;

namespace AIPA.Infrastructure.Persistence.Seeds;

/// <summary>
/// Database seeding for initial data
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds initial data to the database
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="passwordHashingService">Password hashing service</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task SeedAsync(ApplicationDbContext context, IPasswordHashingService passwordHashingService, CancellationToken cancellationToken = default)
    {
        // Seed users if none exist
        if (!context.Users.Any())
        {
            await SeedUsersAsync(context, passwordHashingService, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedUsersAsync(ApplicationDbContext context, IPasswordHashingService passwordHashingService, CancellationToken cancellationToken = default)
    {
        // Define plain text passwords (keep these for reference)
        const string johnPassword = "Password123!";
        const string janePassword = "DevPass456!";
        const string mikePassword = "JuniorDev789!";

        // User 1: John Doe - Software Developer
        var user1 = User.Create(
            Email.Create("john.doe@example.com"),
            FullName.Create("John", "Doe"));
        user1.UpdateProfile(UserProfile.Create(
            jobTitle: "Software Developer",
            company: "Tech Corp",
            yearsOfExperience: 3,
            skills: new List<string> { "C#", ".NET", "JavaScript", "React" },
            preferredInterviewTypes: new List<InterviewType> { InterviewType.Technical, InterviewType.Behavioral }
        ));
        user1.SetPasswordHash(passwordHashingService.HashPassword(johnPassword));
        user1.GenerateEmailVerificationToken();
        user1.VerifyEmail(user1.EmailVerificationToken ?? string.Empty);

        // User 2: Jane Smith - Senior Frontend Developer
        var user2 = User.Create(
            Email.Create("jane.smith@example.com"),
            FullName.Create("Jane", "Smith"));
        user2.UpdateProfile(UserProfile.Create(
            jobTitle: "Senior Frontend Developer",
            company: "Web Solutions Inc",
            yearsOfExperience: 5,
            skills: new List<string> { "TypeScript", "React", "Vue.js", "Node.js" },
            preferredInterviewTypes: new List<InterviewType> { InterviewType.Technical, InterviewType.SystemDesign }
        ));
        user2.SetPasswordHash(passwordHashingService.HashPassword(janePassword));
        user2.GenerateEmailVerificationToken();
        user2.VerifyEmail(user2.EmailVerificationToken ?? string.Empty);

        // User 3: Mike Wilson - Junior Developer
        var user3 = User.Create(
            Email.Create("mike.wilson@example.com"),
            FullName.Create("Mike", "Wilson"));
        user3.UpdateProfile(UserProfile.Create(
            jobTitle: "Junior Developer",
            company: "StartUp Ltd",
            yearsOfExperience: 1,
            skills: new List<string> { "Python", "Django", "PostgreSQL" },
            preferredInterviewTypes: new List<InterviewType> { InterviewType.Behavioral }
        ));
        user3.SetPasswordHash(passwordHashingService.HashPassword(mikePassword));
        user3.GenerateEmailVerificationToken();
        user3.VerifyEmail(user3.EmailVerificationToken ?? string.Empty);

        var users = new[] { user1, user2, user3 };

        await context.Users.AddRangeAsync(users, cancellationToken);
    }
}
