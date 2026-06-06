using FiapX.Infra.CrossCutting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddEmailSender(builder.Configuration)
    .AddAppServices()
    .AddMessageConsumer(builder.Configuration)
    .AddLogging();

var host = builder.Build();
host.Run();
