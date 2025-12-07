namespace CQRS
{
    /// <summary>
    /// Base interface for all events.
    /// </summary>
    public interface IEvent;

    /// <summary>
    /// Represents a command event that can be processed or dispatched within the system.
    /// </summary>
    public interface ICommand : IEvent;


    /// <summary>
    /// Represents a query operation that returns a result of the specified type.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by the query.</typeparam>
    public interface IQuery<TResult> : IEvent;

    /// <summary>
    /// Defines a contract for asynchronously dispatching command objects to their appropriate handlers.
    /// </summary>
    public interface ICommandDispatcher
    {
        Task DispatchAsync<TCommand>(TCommand command) where TCommand : ICommand;
    }

    /// <summary>
    /// Defines a contract for handling a command of a specified type asynchronously.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to be handled. Must implement the <see cref="ICommand"/> interface.</typeparam>
    public interface ICommandHandler<TCommand> where TCommand : ICommand
    {
        Task HandleAsync(TCommand command);
    }

    /// <summary>
    /// Defines a contract for asynchronously dispatching queries to their corresponding handlers and returning the
    /// result.
    /// </summary>
    public interface IQueryDispatcher
    {
        Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>;
    }

    /// <summary>
    /// Defines a handler for processing queries of a specified type and returning a result asynchronously.
    /// </summary>
    /// <typeparam name="TQuery">The type of query to be handled. Must implement <see cref="IQuery{TResult}"/>.</typeparam>
    /// <typeparam name="TResult">The type of result returned by the query handler.</typeparam>
    public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<TResult> HandleAsync(TQuery query);
    }
}
