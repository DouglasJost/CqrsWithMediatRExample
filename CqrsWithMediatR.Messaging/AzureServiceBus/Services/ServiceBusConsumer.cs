using Azure.Messaging.ServiceBus;
using CqrsWithMediatR.Configuration.Constants;
using CqrsWithMediatR.Configuration.Interfaces;
using CqrsWithMediatR.Contracts.AzureSeviceBus.DTOs;
using CqrsWithMediatR.Contracts.AzureSeviceBus.Events;
using CqrsWithMediatR.WriteSync.Features.Products.Commands;
using MediatR;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CqrsWithMediatR.Messaging.AzureServiceBus.Services
{
    public class ServiceBusConsumer
    {
        private readonly IMediator _mediator;
        private readonly IKeyVaultService _appKeyVaultService;
        private readonly ServiceBusClient _serviceBusClient;

        public ServiceBusConsumer(
            IMediator mediator,
            IKeyVaultService appKeyVaultService,
            ServiceBusClient serviceBusClient)
        {
            _mediator = mediator;
            _appKeyVaultService = appKeyVaultService;
            _serviceBusClient = serviceBusClient;
        }

        public async Task StartListening()
        {
            var queueName = await _appKeyVaultService.GetSecretValueAsync(KeyVaultSecretNames.Azure_Service_Bus_QueueName);

            ServiceBusProcessor processor = _serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,   // Ensures messages are completed only after processing
                MaxConcurrentCalls = 5,         // Processes multiple messages in parallel
                PrefetchCount = 10,             // Fetches messages in batches
            });

            processor.ProcessMessageAsync += MessageHandlerAsync;
            processor.ProcessErrorAsync += ErrorHandler;

            await processor.StartProcessingAsync();
        }

        private async Task MessageHandlerAsync(ProcessMessageEventArgs args)
        {
            string body = Encoding.UTF8.GetString(args.Message.Body);
            var wrapper = JsonSerializer.Deserialize<MessageWrapperDto>(body);

            if (wrapper == null || wrapper.Payload.ValueKind == JsonValueKind.Undefined)
            {
                // throw an exception and-or log exception
                return;
            }

            if (wrapper.EventType == nameof(ProductCreatedEvent))
            {
                await HandleProductCreateEvent(wrapper);
            }
            else if (wrapper.EventType == nameof(ProductUpdatedEvent))
            {
                await HandleProductUpdatedEvent(wrapper);
            }

            await args.CompleteMessageAsync(args.Message);
        }

        private async Task HandleProductCreateEvent(MessageWrapperDto wrapper)
        {
            var productEvent = wrapper.Payload.Deserialize<ProductCreatedEvent>();
            if (productEvent == null)
            {
                throw new JsonException("Failed to deserialize ProductCreatedEvent from message.");
            }

            await _mediator.Send(new SyncProductCreateCommand(productEvent));
            Console.WriteLine($"ServiceBusConsumer.HandleProductCreateEvent - Sent SyncProductCreateCommand(productEvent) message");
        }

        private async Task HandleProductUpdatedEvent(MessageWrapperDto wrapper)
        {
            var productEvent = wrapper.Payload.Deserialize<ProductUpdatedEvent>();
            if (productEvent == null)
            {
                throw new JsonException("Failed to deserialize ProductUpdatedEvent from message.");
            }

            await _mediator.Send(new SyncProductUpdateCommand(productEvent));
            Console.WriteLine($"ServiceBusConsumer.HandleProductUpdatedEvent - Sent SyncProductUpdateCommand(productEvent) message");
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine($"Service Bus Error: {args.Exception.Message}");

            if (args.Exception is ServiceBusException sbEx)
            {
                Console.WriteLine($"Service Bus Error Code: {sbEx.Reason}");
            }

            Console.WriteLine($"Fully Qualified Namespace: {args.FullyQualifiedNamespace}");
            Console.WriteLine($"Entity Path: {args.EntityPath}");

            return Task.CompletedTask;
        }
    }
}
