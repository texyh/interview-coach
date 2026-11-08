namespace AIPA.Application.Common.Models;

/// <summary>
/// Represents pagination parameters
/// </summary>
public record PaginationQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public int Skip => (Page - 1) * PageSize;
}

/// <summary>
/// Represents a paginated result
/// </summary>
/// <typeparam name="T">The type of items in the result</typeparam>
public record PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
