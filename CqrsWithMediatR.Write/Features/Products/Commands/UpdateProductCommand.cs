using MediatR;

namespace CqrsWithMediatR.Write.Features.Products.Commands
{
    public class UpdateProductCommand : IRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
        public byte[] RowVersion { get; set; } = default!;
    }
}
