
using AppDomainEntityFramework;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using CqrsWithMediatR.API.DependencyInjection;
using CqrsWithMediatR.API.Pipelines;
using CqrsWithMediatR.Configuration.Constants;
using CqrsWithMediatR.Configuration.Enums;
using CqrsWithMediatR.Configuration.Interfaces;
using CqrsWithMediatR.Configuration.Services;
using CqrsWithMediatR.Contracts.Interfaces;
using CqrsWithMediatR.Messaging.AzureServiceBus.Services;
using CqrsWithMediatR.Read;
using CqrsWithMediatR.Write;
using CqrsWithMediatR.WriteSync;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CqrsWithMediatR.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //
            // AddTransient - Create a new instance every time it is requested
            // AddScoped    - One instance per HTTP request
            // AddSingleton - One instance for the entire app
            //

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Add services to the DI container.
            // 
            //var keyVaultUrl = Environment.GetEnvironmentVariable(KeyVaultSecretNames.Azure_KeyVault_Url)
            //    ?? throw new InvalidOperationException("The key vault URL is not defined.");
            //builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential());
            ////
            //var kvSource = SecretSource.EnvironmentVariable;
            //builder.Services.AddSingleton(new ApplicationKeyVaultService(keyVaultUrl, kvSource));


            // Register IKeyVaultService as a singleton

            var keyVaultUrl = Environment.GetEnvironmentVariable(KeyVaultSecretNames.Azure_KeyVault_Url)
                ?? throw new InvalidOperationException("The key vault URL is not defined.");
            var kvSource = SecretSource.EnvironmentVariable;
            builder.Services.AddSingleton<IKeyVaultService>(sp =>
            {
                return new ApplicationKeyVaultService(keyVaultUrl, kvSource);
            });

            // Register ServiceBusPublisher
            builder.Services.AddSingleton<IMessagePublisher, ServiceBusPublisher>();

            // Register remaining services that follow the convention MyClassName:IMyClassName
            builder.Services.AddServicesWithDefaultConventions();


            // Add DB context as a Factory.
            //   When a db context is needed, the respective class will need to inject a 
            //   IDbContextFactory and retrieve a context similar to this:
            //
            //     await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
            //
            builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("ProductDB")
                        .LogTo(Console.WriteLine, LogLevel.Information));


            // Add MediatR : Scan all handlers in the same assembly as Program.cs
            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(ReadAssemblyMarker).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(WriteAssemblyMarker).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(WriteSyncAssemblyMarker).Assembly);
            });


            // Register ServiceBusPublisher
            builder.Services.AddSingleton<ServiceBusPublisher>();


            // Register ServiceBusClient as Singleton (to prevent connection issues)
            var keyVaultService = new ApplicationKeyVaultService(keyVaultUrl, kvSource);
            var fullyQualifiedNamespace = await keyVaultService.GetSecretValueAsync(KeyVaultSecretNames.Azure_Service_Bus_Namespace)
                ?? throw new InvalidOperationException($"The environment variable '{KeyVaultSecretNames.Azure_Service_Bus_Namespace}' is not set in the Development environment.");
            builder.Services.AddSingleton(sp =>
            {
                return new ServiceBusClient(fullyQualifiedNamespace, new DefaultAzureCredential());
            });


            // Register ServiceBusConsumer
            builder.Services.AddSingleton<ServiceBusConsumer>();


            // Register Logging
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));


            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            var app = builder.Build();


            // Start the ServiceBusConsumer process
            var serviceBusConsumer = app.Services.GetRequiredService<ServiceBusConsumer>();
            await serviceBusConsumer.StartListening();


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();


            app.Run();
        }
    }
}
