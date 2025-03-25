
//using Azure.Identity;
//using Azure.Messaging.ServiceBus;
//using CqrsWithMediatR.API.AzureServiceBus.Models;
//using System.Text.Json;
//using System.Text;
//using System.Threading.Tasks;
//using System;
//using CqrsWithMediatR.Configuration.Interfaces;
//using CqrsWithMediatR.Configuration.Constants;

//namespace CqrsWithMediatR.API.AzureServiceBus.Services
//{
//    public class ServiceBusPublisher
//    {
//        private readonly IKeyVaultService _appKeyVaultService;

//        public ServiceBusPublisher(IKeyVaultService appKeyVaultService)
//        {
//            _appKeyVaultService = appKeyVaultService;
//        }

//        public async Task SendMessageAsync<T>(T eventMessage) where T : class
//        {
//            var queueName = await _appKeyVaultService.GetSecretValueAsync(KeyVaultSecretNames.Azure_Service_Bus_QueueName);
//            var nameSpace = await _appKeyVaultService.GetSecretValueAsync(KeyVaultSecretNames.Azure_Service_Bus_Namespace);

//            await using ServiceBusClient client = new ServiceBusClient(nameSpace, new DefaultAzureCredential());
//            ServiceBusSender sender = client.CreateSender(queueName);

//            // Convert eventMessage to JSON string
//            string jsonPayload = JsonSerializer.Serialize(eventMessage);

//            // Deserialize JSON string back to JsonElement
//            JsonElement payloadElement = JsonSerializer.Deserialize<JsonElement>(jsonPayload);

//            var wrappedMessage = new MessageWrapper()
//            {
//                EventType = typeof(T).Name,
//                Payload = payloadElement
//            };

//            string messageBody = JsonSerializer.Serialize(wrappedMessage);
//            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody))
//            {
//                ContentType = "application/json"
//            };

//            await sender.SendMessageAsync(message);
//            Console.WriteLine($"Sent ProductCreatedEvent: {messageBody}");
//        }
//    }
//}
