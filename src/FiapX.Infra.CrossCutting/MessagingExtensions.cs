using FiapX.Infra.Messaging.Options;
using FiapX.Infra.Messaging.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiapX.Infra.CrossCutting;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AwsCredentialsOptions>(configuration.GetSection("AwsCredentials"));
        services.Configure<MessagingOptions>(configuration.GetSection(nameof(MessagingOptions)));

        services.AddMessaging(builder =>
        {
            if (configuration.GetSection(nameof(MessagingOptions)).Get<MessagingOptions>()!.DisableConsumers)
                return;

            // builder.AddConsumer<BudgetCreatedEventConsumer, BudgetCreatedEvent>();
        });

        return services;
    }
}
