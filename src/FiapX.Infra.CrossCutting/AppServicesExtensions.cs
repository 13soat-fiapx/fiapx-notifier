using FiapX.Application.Base;
using FiapX.Application.Options;
using Microsoft.Extensions.DependencyInjection;

namespace FiapX.Infra.CrossCutting;

public static class AppServicesExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        var appServices = typeof(IAppService).Assembly.GetTypes()
            .Where(type => type.GetInterfaces().Contains(typeof(IAppService)));
        foreach (var appService in appServices)
            services.AddScoped(appService);

        services.Configure<EmailContent>(configuration.GetSection(nameof(EmailContent)));

        return services;
    }
}
