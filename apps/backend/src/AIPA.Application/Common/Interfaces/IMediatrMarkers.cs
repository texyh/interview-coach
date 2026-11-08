using AIPA.Application.Common.Models;
using MediatR;

namespace AIPA.Application.Common.Interfaces;

/// <summary>
/// Marker interface for queries that return a result
/// </summary>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

/// <summary>
/// Marker interface for commands that return a result
/// </summary>
/// <typeparam name="TResponse">The response type</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}

/// <summary>
/// Marker interface for commands that don't return a value
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Base query handler interface
/// </summary>
/// <typeparam name="TQuery">The query type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}

/// <summary>
/// Base command handler interface
/// </summary>
/// <typeparam name="TCommand">The command type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}

/// <summary>
/// Base command handler interface for commands without response
/// </summary>
/// <typeparam name="TCommand">The command type</typeparam>
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}
