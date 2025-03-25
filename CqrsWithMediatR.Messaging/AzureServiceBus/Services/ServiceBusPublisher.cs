using Azure.Identity;
using Azure.Messaging.ServiceBus;
using CqrsWithMediatR.Configuration.Constants;
using CqrsWithMediatR.Configuration.Interfaces;
using CqrsWithMediatR.Contracts.AzureSeviceBus.DTOs;
using CqrsWithMediatR.Contracts.Interfaces;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CqrsWithMediatR.Messaging.AzureServiceBus.Services
{
    public class ServiceBusPublisher : IMessagePublisher
    {
        // 
        // NOTE: ServiceBusPublisher implements IMessagePublisher, which is not the MyClassName:IMyClassName convention.
        //       Therefore, ServiceRegistration.GetServices() will not include it when AddServicesWithDefaultConventions() is
        //       called by Program.cs to automatically register services to the DI container.
        //
        //       Because of this, ServiceBusPublisher:IMessagePublisher is manually added to the DI container as a singleton
        //       in Program.cs.
        //

        private readonly IKeyVaultService _appKeyVaultService;

        public ServiceBusPublisher(IKeyVaultService appKeyVaultService)
        {
            _appKeyVaultService = appKeyVaultService;
        }

        public async Task SendMessageAsync<T>(T eventMessage) where T : class
        {
            var queueName = await _appKeyVaultService.GetSecretValueAsync(KeyVaultSecretNames.Azure_Service_Bus_QueueName);
            var nameSpace = await _appKeyVaultService.GetSecretValueAsync(KeyVaultSecretNames.Azure_Service_Bus_Namespace);

            await using ServiceBusClient client = new ServiceBusClient(nameSpace, new DefaultAzureCredential());
            ServiceBusSender sender = client.CreateSender(queueName);

            // Convert eventMessage to JSON string
            string jsonPayload = JsonSerializer.Serialize(eventMessage);

            // Deserialize JSON string back to JsonElement
            JsonElement payloadElement = JsonSerializer.Deserialize<JsonElement>(jsonPayload);

            var wrappedMessage = new MessageWrapperDto()
            {
                EventType = typeof(T).Name,
                Payload = payloadElement
            };

            string messageBody = JsonSerializer.Serialize(wrappedMessage);
            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody))
            {
                ContentType = "application/json"
            };

            await sender.SendMessageAsync(message);
            Console.WriteLine($"Sent ProductCreatedEvent: {messageBody}");
        }
    }
}
