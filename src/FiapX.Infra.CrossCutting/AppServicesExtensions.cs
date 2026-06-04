using FiapX.Domain.Base;
using Microsoft.Extensions.DependencyInjection;

namespace FiapX.Infra.CrossCutting;

public static class AppServicesExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        var appServices = typeof(IAppService).Assembly.GetTypes()
            .Where(type => type.GetInterfaces().Contains(typeof(IAppService)));
        foreach (var appService in appServices)
            services.AddScoped(appService);

        return services;
    }
}
