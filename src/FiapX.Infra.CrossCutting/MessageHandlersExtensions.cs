using FiapX.Application.Handlers;
using FiapX.Domain.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace FiapX.Infra.CrossCutting;

public static class MessageHandlersExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        var handlers = typeof(NotificationHandler).Assembly.GetTypes()
            .Where(type => type.GetInterfaces().Contains(typeof(IMessageHandler)));
        foreach (var handler in handlers)
            services.AddSingleton(handler);

        return services;
    }
}
