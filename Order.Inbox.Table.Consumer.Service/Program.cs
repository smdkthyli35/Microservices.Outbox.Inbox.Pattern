using MassTransit;
using Order.Inbox.Table.Consumer.Service;
using Order.Inbox.Table.Consumer.Service.Jobs;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
    });
});

builder.Services.AddQuartz(configurator =>
{
    JobKey jobKey = new("OrderInboxConsumerJob");
    configurator.AddJob<OrderInboxConsumerJob>(options => options.WithIdentity(jobKey));

    TriggerKey triggerKey = new("OrderInboxConsumerTrigger");
    configurator.AddTrigger(options => options
        .ForJob(jobKey)
        .WithIdentity(triggerKey)
        .StartAt(DateTime.UtcNow)
        .WithSimpleSchedule(builder => builder
            .WithIntervalInSeconds(5)
            .RepeatForever()));
});

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

var host = builder.Build();
host.Run();
