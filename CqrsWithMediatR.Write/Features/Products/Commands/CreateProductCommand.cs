using MediatR;

namespace CqrsWithMediatR.Write.Features.Products.Commands
{
    public record CreateProductCommand(string Name, decimal Price) : IRequest<int>;
}
