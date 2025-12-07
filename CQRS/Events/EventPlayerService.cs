using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.Events
{
    public class EventPlayerService
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryDispatcher _queryDispatcher;

        private readonly List<IEvent> _eventLog = new();

        public EventPlayerService(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
        {
            _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
            _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
        }

        public async Task<TResult> EmitAsync<TResult>(IQuery<TResult> query)
        {
            _eventLog.Add(query);

            // get the concrete type, e.g. GetValueQuery
            var queryType = query.GetType();

            // create IQueryHandler<GetValueQuery, TResult>
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResult));

            // resolve handler directly
            var handler = _queryDispatcher
                .GetType()                                  // QueryDispatcher
                .GetMethod("DispatchAsync")!                // generic method
                .MakeGenericMethod(queryType, typeof(TResult));

            // call QueryDispatcher.DispatchAsync<GetValueQuery, TResult>(query)
            var task = (Task<TResult>)handler.Invoke(_queryDispatcher, new object[] { query })!;

            return await task;
        }

        public async Task EmitAsync(ICommand command)
        {
            _eventLog.Add(command);
            var method = typeof(ICommandDispatcher)
         .GetMethod(nameof(ICommandDispatcher.DispatchAsync))!
         .MakeGenericMethod(command.GetType());

            await (Task)method.Invoke(_commandDispatcher, [command])!;
        }

        public IEnumerable<IEvent> GetEventLog() => _eventLog.AsReadOnly();

        public async Task ReplayAsync()
        {
            foreach (var evt in _eventLog)
            {
                var attr = evt.GetType()
                    .GetCustomAttributes(typeof(EventReplayBehaviorAttribute), false)
                    .FirstOrDefault() as EventReplayBehaviorAttribute;

                if (attr is null || !attr.Options.HasFlag(EventReplayOptions.Replayable))
                    continue;

                if (evt is ICommand command)
                {
                    var method = typeof(ICommandDispatcher)
                        .GetMethod(nameof(ICommandDispatcher.DispatchAsync))!
                        .MakeGenericMethod(command.GetType());

                    await (Task)method.Invoke(_commandDispatcher, [command])!;
                }
            }
        }

    }
}
