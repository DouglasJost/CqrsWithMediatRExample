
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
using Microsoft.IdentityModel.Tokens;
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


            // Register IKeyVaultService as a singleton
            var keyVaultUrl = Environment.GetEnvironmentVariable(KeyVaultSecretNames.Azure_KeyVault_Url)
                ?? throw new InvalidOperationException("The key vault URL is not defined.");
            var kvSource = SecretSource.EnvironmentVariable;
            builder.Services.AddSingleton<IKeyVaultService>(sp =>
            {
                return new ApplicationKeyVaultService(keyVaultUrl, kvSource);
            });
            var keyVaultService = new ApplicationKeyVaultService(keyVaultUrl, kvSource);


            // Register ServiceBusPublisher
            builder.Services.AddSingleton<IMessagePublisher, ServiceBusPublisher>();


            // Register remaining services that follow the convention MyClassName:IMyClassName
            builder.Services.AddServicesWithDefaultConventions();


            // Add DB context as a Factory.
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


            // JWT Bearer Token Authentication
            var authenticationSecretForKey = await keyVaultService.GetSecretValueAsync(KeyVaultSecretNames.Authentication_SecretForKey);
            var authenticationIssuer = await keyVaultService.GetSecretValueAsync(KeyVaultSecretNames.Authentication_Issuer);
            var authenticationAudience = await keyVaultService.GetSecretValueAsync(KeyVaultSecretNames.Authentication_Audience);
            builder.Services.AddAuthentication("Bearer")
            // Configure the JWT.  Reference appsettings.json / appsettings.Development.json for JWT configuration.
            .AddJwtBearer(options =>
            {
                // Validation rules for incoming tokens
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    // Verify tokens's iss (issuer) claim against trusted issuer.
                    ValidateIssuer = true,
                    // Verify tokens's aud (audience) claim matches this application's audience. 
                    ValidateAudience = true,
                    // Validate that the token's signature is correct by using a trusted signing key.
                    ValidateIssuerSigningKey = true,
                    // Only tokens issued by this issuer are accepted.
                    ValidIssuer = authenticationIssuer,
                    // Only tokens intended for this audience are accepted.
                    ValidAudience = authenticationAudience,
                    // This is the security key used to validate the token's signature.
                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(authenticationSecretForKey))
                };

                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnAuthenticationFailed = context => 
                    {
                        Console.WriteLine($"JWT authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    }
                };
            });


            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            var app = builder.Build();


            // Ensure In-Memory DB is seeded.  REMOVE THIS when migrating to SQL Server database.
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.EnsureCreated(); // THIS is the key
            }


            // Start the ServiceBusConsumer process
            var serviceBusConsumer = app.Services.GetRequiredService<ServiceBusConsumer>();
            await serviceBusConsumer.StartListening();


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();


            app.Run();
        }
    }
}
