using AIPA.Application.DTOs.Users;
using AIPA.Domain.Users.Entities;
using AutoMapper;

namespace AIPA.Application.Mappings;

/// <summary>
/// AutoMapper profile for User entity mappings
/// </summary>
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // User to UserDto mapping
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FullName.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.FullName.LastName))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.FullName.DisplayName))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

        // User to UserSummaryDto mapping
        CreateMap<User, UserSummaryDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.FullName.DisplayName))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

        // UserProfile to UserProfileDto mapping
        CreateMap<AIPA.Domain.ValueObjects.UserProfile, UserProfileDto>()
            .ForMember(dest => dest.Bio, opt => opt.Ignore()) // Not available in UserProfile value object
            .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore()) // Not available in UserProfile value object
            .ForMember(dest => dest.LinkedInUrl, opt => opt.Ignore()) // Not available in UserProfile value object
            .ForMember(dest => dest.GitHubUrl, opt => opt.Ignore()) // Not available in UserProfile value object
            .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.Skills))
            .ForMember(dest => dest.Interests, opt => opt.Ignore()); // Not available in UserProfile value object
    }
}
