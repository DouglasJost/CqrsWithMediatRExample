using AppDomainEntityFramework.Entities;
using AppDomainEntityFramework;
using CqrsWithMediatR.Read.Features.Products.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CqrsWithMediatR.Read.Features.Products.Handlers
{
    public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductReadOnly?>
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public GetProductByIdHandler(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<ProductReadOnly?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
            {
                var product = await dbContext.ProductsReadOnly.FindAsync(request.Id);
                if (product == null)
                {
                    throw new KeyNotFoundException($"GetProductByIdHandler: Product {request.Id} not found.");
                }

                return new ProductReadOnly
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    RowVersionBase64 = Convert.ToBase64String(product.RowVersion)
                };
            }
        }
    }
}
