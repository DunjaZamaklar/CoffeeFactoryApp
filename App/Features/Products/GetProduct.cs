using App.Contracts;
using App.Data.Database;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace App.Features.Products;
public static class GetProduct
{
    public class Query : IRequest<ProductResponse>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, ProductResponse>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<ProductResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            var productResponse = await _applicationDbContext.Products
                .Where(p => p.Id == request.Id)
                .Select(p => new ProductResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (productResponse is null)
            {
                return null;
            }
            return productResponse;
        }
    }
}

public class GetProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("products/{id}", async (Guid id, ISender sender) =>
        {
            var query = new GetProduct.Query { Id = id };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result is null)
            {
                return null;
            }
            return result;
        });
    }
}