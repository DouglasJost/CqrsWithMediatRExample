using CqrsWithMediatR.Contracts.AzureSeviceBus.Events;
using MediatR;

namespace CqrsWithMediatR.WriteSync.Features.Products.Commands
{
    public record SyncProductCreateCommand(ProductCreatedEvent Event) : IRequest;
}
