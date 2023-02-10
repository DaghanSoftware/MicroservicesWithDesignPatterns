using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Order.API.Models;
using Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMassTransit(x => {
    x.AddConsumer<PaymentCompletedEventConsumer>();
    x.AddConsumer<PaymentFailedEventConsumer>();
    x.AddConsumer<StockNotReservedEventConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));

        cfg.ReceiveEndpoint(RabbitMQSettingsConst.OrderPaymentCompletedEventQueueName, e =>
        {
            e.ConfigureConsumer<PaymentCompletedEventConsumer>(context);
        });

        cfg.ReceiveEndpoint(RabbitMQSettingsConst.OrderPaymentFailedEventQueueName, e =>
        {
            e.ConfigureConsumer<PaymentFailedEventConsumer>(context);
        });

        cfg.ReceiveEndpoint(RabbitMQSettingsConst.OrderStockNotReservedQueueName, e =>
        {
            e.ConfigureConsumer<StockNotReservedEventConsumer>(context);
        });
    });
});
builder.Services.AddDbContext<AppDbContext>(x =>
{
    x.UseSqlServer(builder.Configuration.GetConnectionString("LocalDb"));
});
builder.Services.AddOptions<MassTransitHostOptions>()
    .Configure(options =>
    {
        // if specified, waits until the bus is started before
        // returning from IHostedService.StartAsync
        // default is false
        options.WaitUntilStarted = true;

        // if specified, limits the wait time when starting the bus
        options.StartTimeout = TimeSpan.FromSeconds(10);

        // if specified, limits the wait time when stopping the bus
        options.StopTimeout = TimeSpan.FromSeconds(30);
    });
//builder.Services.AddMassTransitHostedService();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
