using AppDomainEntityFramework;
using AppDomainEntityFramework.Entities;
using CqrsWithMediatR.WriteSync.Features.Products.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CqrsWithMediatR.WriteSync.Features.Products.Handlers
{
    public class SyncProductCreateHandler : IRequestHandler<SyncProductCreateCommand>
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public SyncProductCreateHandler(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task Handle(SyncProductCreateCommand request, CancellationToken cancellationToken)
        {
            await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
            {
                var product = new ProductReadOnly()
                {
                    Id = request.Event.Id,
                    Name = request.Event.Name,
                    Price = request.Event.Price,
                    RowVersion = request.Event.RowVersion
                };

                dbContext.ProductsReadOnly.Add(product);
                await dbContext.SaveChangesAsync(cancellationToken);

                Console.WriteLine($"SyncProductCreateHandler.Handle - ProductsReadOnly Created: PK:{request.Event.Id}, Name:{request.Event.Name}, Price:{request.Event.Price}, RowVersion: {request.Event.RowVersion}");

                return;
            }
        }
    }
}
