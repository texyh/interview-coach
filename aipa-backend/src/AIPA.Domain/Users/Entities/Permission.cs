using AIPA.Domain.Common;
using AIPA.Domain.ValueObjects;

namespace AIPA.Domain.Users.Entities;

/// <summary>
/// Represents a permission in the system (e.g., CreateInterview, ViewAnalytics)
/// </summary>
public class Permission : BaseEntity
{
    public PermissionId Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<Role> _roles = [];
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    private Permission()
    {
        Id = null!;
        Name = null!;
        Description = null!;
        Category = null!;
    } // For EF Core

    private Permission(PermissionId id, string name, string description, string category, bool isActive)
    {
        Id = id;
        Name = name;
        Description = description;
        Category = category;
        IsActive = isActive;
    }

    public static Permission Create(string name, string description, string category)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Permission name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Permission category cannot be empty", nameof(category));

        return new Permission(
            PermissionId.New(),
            name.Trim(),
            description?.Trim() ?? string.Empty,
            category.Trim(),
            isActive: true);
    }

    public void UpdateDetails(string name, string description, string category)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Permission name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Permission category cannot be empty", nameof(category));

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Category = category.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
