using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.Commands
{
    ///<inheritdoc cref="CQRS.ICommandDispatcher"/>
    internal class CommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceProvider _provider;

        public CommandDispatcher(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }
        public async Task DispatchAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(typeof(TCommand));
            var handler = _provider.GetService(handlerType);


            if (handler is null)
                throw new InvalidOperationException($"Handler for {typeof(TCommand).Name} not found");

            var method = handlerType.GetMethod("HandleAsync");
            if (method is null)
                throw new InvalidOperationException($"HandleAsync method not found for {handlerType.Name}");

            await(Task)method.Invoke(handler, [command])!;
        }
    }
}
