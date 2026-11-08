using AIPA.Domain.Enums;

namespace AIPA.Domain.ValueObjects;

public record UserProfile
{
    public string? JobTitle { get; }
    public string? Company { get; }
    public int? YearsOfExperience { get; }
    public List<string> Skills { get; }
    public List<InterviewType> PreferredInterviewTypes { get; }

    private UserProfile(
        string? jobTitle,
        string? company,
        int? yearsOfExperience,
        List<string> skills,
        List<InterviewType> preferredInterviewTypes)
    {
        JobTitle = jobTitle;
        Company = company;
        YearsOfExperience = yearsOfExperience;
        Skills = skills ?? [];
        PreferredInterviewTypes = preferredInterviewTypes ?? [];
    }

    public static UserProfile CreateDefault() => new(
        jobTitle: null,
        company: null,
        yearsOfExperience: null,
        skills: [],
        preferredInterviewTypes: []);

    public static UserProfile Create(
        string? jobTitle,
        string? company,
        int? yearsOfExperience,
        List<string>? skills,
        List<InterviewType>? preferredInterviewTypes)
    {
        return new UserProfile(
            jobTitle?.Trim(),
            company?.Trim(),
            yearsOfExperience,
            skills ?? [],
            preferredInterviewTypes ?? []);
    }
}
