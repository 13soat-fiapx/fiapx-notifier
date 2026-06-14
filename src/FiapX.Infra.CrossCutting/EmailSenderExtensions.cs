using FiapX.Infra.EmailSender;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiapX.Infra.CrossCutting;

public static class EmailSenderExtensions
{
    public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration configuration)
    {
        var emailSenderOptions = configuration.GetSection(nameof(EmailSenderOptions)).Get<EmailSenderOptions>()!;
        services.AddScoped<IEmailSenderService>(_ => new EmailSenderService(emailSenderOptions));

        return services;
    }
}
