﻿using AppDomainEntityFramework;
using CqrsWithMediatR.Contracts.AzureSeviceBus.Events;
using CqrsWithMediatR.Contracts.Interfaces;
using CqrsWithMediatR.Write.Features.Products.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using CqrsWithMediatR.Exceptions;

namespace CqrsWithMediatR.Write.Features.Products.Handlers
{
    public class UpdateProductHandler : IRequestHandler<UpdateProductCommand>
    {
        private readonly IMessagePublisher _serviceBusPublisher;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public UpdateProductHandler(
            IMessagePublisher serviceBusPublisher,
            IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _serviceBusPublisher = serviceBusPublisher;
            _dbContextFactory = dbContextFactory;
        }

        public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
            {
                var product = await dbContext.Products.FindAsync(request.Id);
                if (product == null)
                {
                    throw new KeyNotFoundException($"UpdateProductHandler: Product {request.Id} not found for update.");
                }

                // Check if using In-Memory database (temporary fix)
                if (dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
                {
                    product.RowVersion = BitConverter.GetBytes(BitConverter.ToInt64(product.RowVersion, 0) + 1); // Manual Increment
                }
                else
                {
                    dbContext.Entry(product).Property(p => p.RowVersion).OriginalValue = request.RowVersion; // EF Core handles it for SQL Server
                }

                product.Name = request.Name;
                product.Price = request.Price;

                try
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    throw new ConcurrencyException("Conflict detected: The product was updated by another request.", ex);
                }

                var productUpdatedEvent = new ProductUpdatedEvent(product.Id, product.Name, product.Price, product.RowVersion);

                // Fire-and-forget event publishing (does not block response)
                _ = Task.Run(async () =>
                {
                    await _serviceBusPublisher.SendMessageAsync(productUpdatedEvent);
                });
            }
        }


    }
}
