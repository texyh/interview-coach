using System.Diagnostics.CodeAnalysis;

namespace AIPA.Application.Common.Models;

/// <summary>
/// Represents the result of an operation that can succeed or fail
/// </summary>
/// <typeparam name="T">The type of the result value</typeparam>
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public IEnumerable<string> Errors { get; }

    private Result(bool isSuccess, T? value, IEnumerable<string> errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors ?? Array.Empty<string>();
    }

    /// <summary>
    /// Creates a successful result with a value
    /// </summary>
    public static Result<T> Success(T value) => new(true, value, Array.Empty<string>());

    /// <summary>
    /// Creates a failed result with a single error
    /// </summary>
    public static Result<T> Failure(string error) => new(false, default, new[] { error });

    /// <summary>
    /// Creates a failed result with multiple errors
    /// </summary>
    public static Result<T> Failure(IEnumerable<string> errors) => new(false, default, errors);

    /// <summary>
    /// Creates a failed result with a single error (alias for Failure)
    /// </summary>
    public static Result<T> Error(string error) => Failure(error);

    /// <summary>
    /// Creates a failed result with multiple errors (alias for Failure)
    /// </summary>
    public static Result<T> Error(IEnumerable<string> errors) => Failure(errors);

    /// <summary>
    /// Implicit conversion from T to Result&lt;T&gt;
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Gets the first error message
    /// </summary>
    public string FirstError => Errors.FirstOrDefault() ?? string.Empty;
}

/// <summary>
/// Represents the result of an operation without a return value
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IEnumerable<string> Errors { get; }

    private Result(bool isSuccess, IEnumerable<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? Array.Empty<string>();
    }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result Success() => new(true, Array.Empty<string>());

    /// <summary>
    /// Creates a failed result with a single error
    /// </summary>
    public static Result Failure(string error) => new(false, new[] { error });

    /// <summary>
    /// Creates a failed result with multiple errors
    /// </summary>
    public static Result Failure(IEnumerable<string> errors) => new(false, errors);

    /// <summary>
    /// Creates a failed result with a single error (alias for Failure)
    /// </summary>
    public static Result Error(string error) => Failure(error);

    /// <summary>
    /// Creates a failed result with multiple errors (alias for Failure)
    /// </summary>
    public static Result Error(IEnumerable<string> errors) => Failure(errors);

    /// <summary>
    /// Gets the first error message
    /// </summary>
    public string FirstError => Errors.FirstOrDefault() ?? string.Empty;
}
