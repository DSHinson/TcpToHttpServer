using CQRS.Commands;
using CQRS.Events;
using CQRS.Queries;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CQRS
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCqrs(this IServiceCollection services, string assemblyName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic && a.FullName.StartsWith(assemblyName)).ToArray();

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    var interfaces = type.GetInterfaces();

                    foreach (var iface in interfaces)
                    {
                        if (!type.IsClass || type.IsAbstract) continue;

                        if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(ICommandHandler<>))
                        {
                            services.AddScoped(iface, type);
                        }

                        if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
                        {
                            services.AddScoped(iface, type);
                        }
                    }
                }
            }

            services.AddScoped<ICommandDispatcher, CommandDispatcher>();
            services.AddScoped<IQueryDispatcher, QueryDispatcher>();
            services.AddScoped<EventPlayerService>();

            return services;
        }

        public static IServiceCollection AddCqrs(this IServiceCollection services, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsClass || type.IsAbstract)
                        continue;

                    foreach (var iface in type.GetInterfaces())
                    {
                        if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(ICommandHandler<>))
                        {
                            services.AddScoped(iface, type);
                        }

                        if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
                        {
                            services.AddScoped(iface, type);
                        }
                    }
                }
            }

            services.AddScoped<ICommandDispatcher, CommandDispatcher>();
            services.AddScoped<IQueryDispatcher, QueryDispatcher>();
            services.AddScoped<EventPlayerService>();

            return services;
        }

    }
}
