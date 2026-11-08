using AIPA.Domain.Users.Repositories;
using AIPA.Domain.Interviews.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace AIPA.Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern interface for coordinating repositories and transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{

    /// <summary>
    /// Saves all changes to the database within a transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of affected entities</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a database transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction object</returns>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
