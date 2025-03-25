using AppDomainEntityFramework.Entities;
using MediatR;
using System.Collections.Generic;

namespace CqrsWithMediatR.Read.Features.Products.Queries
{
    public record GetAllProductsQuery() : IRequest<List<ProductReadOnly>>;
}
