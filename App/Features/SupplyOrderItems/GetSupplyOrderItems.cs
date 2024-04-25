using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.SupplyOrderItems.GetSupplyOrderItems;

namespace App.Features.SupplyOrderItems;

public static class GetSupplyOrderItems
{
    public class Query : IRequest<List<SupplyItemResponse>>
    {
    }

    internal sealed class Handler : IRequestHandler<Query, List<SupplyItemResponse>>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<List<SupplyItemResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var supplyItemsResponse = await _applicationDbContext.SupplyItems
                .Include(si => si.Supply)
                    .ThenInclude(s => s.Supplier)
                .Include(si => si.Supply)
                    .ThenInclude(s => s.SupplyCategory)
                .Include(si => si.SupplyOrder)
                    .ThenInclude(soe => soe.Status)
                .Include(si => si.SupplyOrder)
                    .ThenInclude(soe => soe.Employee)
                        .ThenInclude(e => e.Employee)
                .Include(si => si.SupplyOrder)
                    .ThenInclude(soe => soe.Employee)
                        .ThenInclude(e => e.EmployeePosition)
                .Select(p => new SupplyItemResponse
                {
                    Id = p.Id,
                    Supply = p.Supply,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    SupplyOrder = p.SupplyOrder
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return supplyItemsResponse;
        }
    }
}

public class GetAllSupplyOrderItemsEndpoint : CarterModule
{
    public GetAllSupplyOrderItemsEndpoint() : base("/api/supplyOrderItem")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("", async (ISender sender) =>
        {
            var query = new GetSupplyOrderItems.Query { };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result is null)
            {
                return null;
            }
            return result;
        });
    }
}