
using CQRS.Events;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CQRS.Tests
{
    public class CqrsInfrastructureTests
    {
        public class TestState
        {
            public int Value { get; set; }
        }

        // Command + handler
        public class IncrementCommand : ICommand { }

        public class IncrementCommandHandler : ICommandHandler<IncrementCommand>
        {
            private readonly TestState _state;
            public IncrementCommandHandler(TestState state) => _state = state;

            public Task HandleAsync(IncrementCommand command)
            {
                _state.Value++;
                return Task.CompletedTask;
            }
        }

        // Replayable command + handler
        [EventReplayBehavior(EventReplayOptions.Replayable | EventReplayOptions.MutatesData)]
        public class ReplayableIncrementCommand : ICommand { }

        public class ReplayableIncrementCommandHandler : ICommandHandler<ReplayableIncrementCommand>
        {
            private readonly TestState _state;
            public ReplayableIncrementCommandHandler(TestState state) => _state = state;

            public Task HandleAsync(ReplayableIncrementCommand command)
            {
                _state.Value++;
                return Task.CompletedTask;
            }
        }

        // Query + handler
        [EventReplayBehavior(EventReplayOptions.Replayable)]
        public class GetValueQuery : IQuery<int> { }

        public class GetValueQueryHandler : IQueryHandler<GetValueQuery, int>
        {
            private readonly TestState _state;
            public GetValueQueryHandler(TestState state) => _state = state;

            public Task<int> HandleAsync(GetValueQuery query) => Task.FromResult(_state.Value);
        }

        [Test]
        public async Task CommandAndQuery_Dispatchers_Invoke_Handlers()
        {
            var services = new ServiceCollection();

            // shared state injected into handlers
            services.AddSingleton<TestState>();

            // Register handlers/dispatchers by scanning the test assembly
            services.AddCqrs(Assembly.GetExecutingAssembly());

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            var cmdDispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();
            var qryDispatcher = scope.ServiceProvider.GetRequiredService<IQueryDispatcher>();

            // Dispatch command
            await cmdDispatcher.DispatchAsync(new IncrementCommand());

            // Query should reflect the increment
            var value = await qryDispatcher.DispatchAsync<GetValueQuery, int>(new GetValueQuery());
            Assert.That(value, Is.EqualTo(1));
        }

        [Test]
        public async Task EventPlayerService_Emits_And_Logs_Events()
        {
            var services = new ServiceCollection();
            services.AddSingleton<TestState>();
            services.AddCqrs(Assembly.GetExecutingAssembly());

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            var eventPlayer = scope.ServiceProvider.GetRequiredService<EventPlayerService>();
            var state = scope.ServiceProvider.GetRequiredService<TestState>();
            var qryResult = await eventPlayer.EmitAsync<int>(new GetValueQuery());
            Assert.That(qryResult, Is.EqualTo(0));

            await eventPlayer.EmitAsync(new IncrementCommand());

            // After emitting command, query should show incremented state
            var newVal = await eventPlayer.EmitAsync(new GetValueQuery());
            
            Assert.That(newVal, Is.EqualTo(1));

            var log = eventPlayer.GetEventLog().ToArray();
            
            Assert.That(log.Length, Is.EqualTo(3));
            Assert.That(log[0], Is.TypeOf<GetValueQuery>());
            Assert.That(log[1], Is.TypeOf<IncrementCommand>());
            Assert.That(log[2], Is.TypeOf<GetValueQuery>());
        }

        [Test]
        public void ServiceCollectionRegistersRequiredServices()
        {
            var services = new ServiceCollection();
            services.AddCqrs(Assembly.GetExecutingAssembly());
            var provider = services.BuildServiceProvider();

            Assert.That(provider.GetService<ICommandDispatcher>(),Is.Not.Null);
            Assert.That(provider.GetService<IQueryDispatcher>(), Is.Not.Null);
            Assert.That(provider.GetService<EventPlayerService>(), Is.Not.Null);
        }

        [Test]
        public async Task EventPlayerService_Replay_Replays_Only_Replayable_Commands()
        {
            var services = new ServiceCollection();
            services.AddSingleton<TestState>();
            services.AddCqrs(Assembly.GetExecutingAssembly());

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            var eventPlayer = scope.ServiceProvider.GetRequiredService<EventPlayerService>();
            var state = scope.ServiceProvider.GetRequiredService<TestState>();

            // Emit a non-replayable command (IncrementCommand) and a replayable one
            await eventPlayer.EmitAsync(new IncrementCommand());               // state => 1
            await eventPlayer.EmitAsync(new ReplayableIncrementCommand());    // state => 2

            Assert.That(state.Value , Is.EqualTo(2));

            // Replay should only re-execute the replayable command, bringing state to 3
            await eventPlayer.ReplayAsync();

            Assert.That(state.Value, Is.EqualTo(3));
        }
    }
}
