using AppDomainEntityFramework.Entities;
using MediatR;

namespace CqrsWithMediatR.Read.Features.Products.Queries
{
    public record GetProductByIdQuery(int Id) : IRequest<ProductReadOnly>;
}
