using FiapX.Infra.CrossCutting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddEmailSender(builder.Configuration)
    .AddAppServices(builder.Configuration)
    .AddMessageConsumer(builder.Configuration)
    .AddLogging();

var host = builder.Build();
await host.RunAsync();
