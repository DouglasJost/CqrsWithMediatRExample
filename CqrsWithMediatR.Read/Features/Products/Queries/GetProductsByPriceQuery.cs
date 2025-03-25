using AppDomainEntityFramework.Entities;
using MediatR;
using System.Collections.Generic;

namespace CqrsWithMediatR.Read.Features.Products.Queries
{
    public record GetProductsByPriceQuery(decimal Price, string Operator) : IRequest<List<ProductReadOnly>>;
}
