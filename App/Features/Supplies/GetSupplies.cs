using App.Contracts;
using App.Data.Database;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.Supplies.GetSupplies;

namespace App.Features.Supplies;

public static class GetSupplies
{
    public class Query : IRequest<List<SupplyResponse>>
    {
    }

    internal sealed class Handler : IRequestHandler<Query, List<SupplyResponse>>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<List<SupplyResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var supplyResponse = await _applicationDbContext.Supplies
                .Select(p => new SupplyResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    Supplier = p.Supplier,
                    SupplyCategory = p.SupplyCategory
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return supplyResponse;
        }
    }
}

public class GetAllSuppliesEndpoint : CarterModule
{
    public GetAllSuppliesEndpoint() : base("/api/supply")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("", async (ISender sender) =>
        {
            var query = new GetSupplies.Query { };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result is null)
            {
                return null;
            }
            return result;
        });
    }
}