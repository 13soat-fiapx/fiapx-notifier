using FiapX.Infra.CrossCutting;
using FiapX.Infra.Messaging.Extensions;
using FiapX.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<BackgroundWorker>();

builder.Services.AddMessaging()
    .AddEmailSender(builder.Configuration)
    .AddAppServices();

var host = builder.Build();
host.Run();
