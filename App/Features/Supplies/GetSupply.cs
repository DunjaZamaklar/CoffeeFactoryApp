using App.Contracts;
using App.Data.Database;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.Supplies.GetSupply;

namespace App.Features.Supplies;

public static class GetSupply
{
    public class Query : IRequest<SupplyResponse>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, SupplyResponse>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<SupplyResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            var supplyResponse = await _applicationDbContext.Supplies
                .Where(p => p.Id == request.Id)
                .Select(p => new SupplyResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Quantity = p.Quantity,
                    Price = p.Price,
                    SupplyCategory = p.SupplyCategory,
                    Supplier = p.Supplier
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (supplyResponse is null)
            {
                return null;
            }
            return supplyResponse;
        }
    }
}

public class GetSupplyEndpoint : CarterModule
{
    public GetSupplyEndpoint() : base("/api/supply")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id}", async (Guid id, ISender sender) =>
        {
            var query = new GetSupply.Query { Id = id };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result is null)
            {
                return null;
            }
            return result;
        }).RequireAuthorization("UserPolicy");
    }
}