using AppDomainEntityFramework;
using CqrsWithMediatR.WriteSync.Features.Products.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CqrsWithMediatR.WriteSync.Features.Products.Handlers
{
    public class SyncProductUpdateHandler : IRequestHandler<SyncProductUpdateCommand>
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public SyncProductUpdateHandler(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task Handle(SyncProductUpdateCommand request, CancellationToken cancellationToken)
        {
            await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
            {
                var existingProduct = await dbContext.ProductsReadOnly.FirstOrDefaultAsync(p => p.Id == request.Event.Id, cancellationToken);
                if (existingProduct == null)
                {
                    throw new KeyNotFoundException($"SyncProductUpdateHandler.Handle: Product {request.Event.Id} not found for update.");
                }

                existingProduct.Name = request.Event.Name;
                existingProduct.Price = request.Event.Price;
                existingProduct.RowVersion = request.Event.RowVersion;
                await dbContext.SaveChangesAsync(cancellationToken);

                Console.WriteLine($"SyncProductUpdateHandler.Handle - ProductsReadOnly Updated: PK:{request.Event.Id}, Name:{request.Event.Name}, Price:{request.Event.Price}, RowVersion: {request.Event.RowVersion}");

                return;
            }
        }

    }
}
