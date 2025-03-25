using AppDomainEntityFramework;
using AppDomainEntityFramework.Entities;
using CqrsWithMediatR.Contracts.AzureSeviceBus.Events;
using CqrsWithMediatR.Contracts.Interfaces;
using CqrsWithMediatR.Write.Features.Products.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CqrsWithMediatR.Write.Features.Products.Handlers
{
    public class CreateProductHandler : IRequestHandler<CreateProductCommand, int>
    {
        private readonly IMessagePublisher _serviceBusPublisher;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public CreateProductHandler(
            IMessagePublisher serviceBusPublisher,
            IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _serviceBusPublisher = serviceBusPublisher;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
            {
                var product = new Product
                {
                    Name = request.Name,
                    Price = request.Price
                };

                // For In-Memory database only, need to assign an initial value for RowVersion
                product.RowVersion = new byte[8];

                dbContext.Products.Add(product);
                await dbContext.SaveChangesAsync(cancellationToken);

                // Publish event to Azure Service Bus
                var productCreatedEvent = new ProductCreatedEvent(product.Id, product.Name, product.Price, product.RowVersion);

                // Fire-and-forget event publishing (does not block response)
                _ = Task.Run(async () =>
                {
                    await _serviceBusPublisher.SendMessageAsync(productCreatedEvent);
                });

                return product.Id;
            }
        }
    }
}
