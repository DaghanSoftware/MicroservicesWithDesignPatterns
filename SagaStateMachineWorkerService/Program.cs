using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SagaStateMachineWorkerService;
using SagaStateMachineWorkerService.Models;
using Shared;
using System.Reflection;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext,services) =>
    {

        services.AddMassTransit(cfg =>
        {
            //‘AddSagaStateMachine’ metodu ile MassTransit servisine state machine entegre edilmekte ve generic olarak hangi state machine nesnesine hangi state instance nesnesinin olacağı bildirilmektedir.
            //Ayrıca ‘EntityFrameworkRepository’ metodu ile state’lerin hangi veritabanında depolanacağı bildirilmektedir.
            //Malum burada da SQL Server kullanacağımızdan dolayı Microsoft.EntityFrameworkCore.SqlServer kütüphanesini yüklemeniz gerekmektedir.
            cfg.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>().EntityFrameworkRepository(opt =>
            {
                opt.AddDbContext<DbContext, OrderStateDbContext>((provider, builder) =>
                {
                    builder.UseSqlServer(hostContext.Configuration.GetConnectionString("LocalDb"), m =>
                    {
                        m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                    });
                });
            });

            //RabbitMQ ile entegrasyonu gerçekleştirilmiştir.
            cfg.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(configure =>
            {
                configure.Host(hostContext.Configuration.GetConnectionString("RabbitMQ"));

                //OrderService'de yeni bir order oluştuğunda IOrderCreatedRequestEvent'ini Send ediyoruz.
                //SagaStateMachine veritabanında State initial OrderCreated olarak güncellemek için
                configure.ReceiveEndpoint(RabbitMQSettingsConst.OrderSaga, e =>
                {
                    e.ConfigureSaga<OrderStateInstance>(provider);
                });
            }));

        });

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
