using AIPA.Domain.Enums;
using AIPA.Domain.Users.Entities;
using AIPA.Domain.Users.Repositories;
using AIPA.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AIPA.Infrastructure.Persistence.Repositories;

/// <summary>
/// User repository implementation
/// </summary>
public class UserRepository : BaseRepository<User, UserId>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public new async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.InterviewSessions)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.InterviewSessions)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public new async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.InterviewSessions)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.InterviewSessions)
            .Where(u => u.Status == AccountStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public new async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await base.AddAsync(user, cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        Update(user);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(UserId id, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        if (user != null)
        {
            Remove(user);
        }
    }

    /// <summary>
    /// Get users with pagination and filtering
    /// </summary>
    public async Task<(IEnumerable<User> Users, int TotalCount)> GetUsersPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Include(u => u.InterviewSessions).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(u =>
                u.Email.Value.ToLower().Contains(lowerSearchTerm) ||
                u.FullName.FirstName.ToLower().Contains(lowerSearchTerm) ||
                u.FullName.LastName.ToLower().Contains(lowerSearchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderBy(u => u.FullName.FirstName)
            .ThenBy(u => u.FullName.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (users, totalCount);
    }
}
