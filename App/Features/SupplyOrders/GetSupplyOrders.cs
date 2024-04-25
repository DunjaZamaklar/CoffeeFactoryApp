using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.SupplyOrders.GetSupplyOrders;

namespace App.Features.SupplyOrders;

public static class GetSupplyOrders
{
    public class Query : IRequest<List<SupplyOrderResponseOverview>>
    {
    }

    internal sealed class Handler : IRequestHandler<Query, List<SupplyOrderResponseOverview>>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<List<SupplyOrderResponseOverview>> Handle(Query request, CancellationToken cancellationToken)
        {
            var supplyOrderResponseOverview = await _applicationDbContext.SupplyOrders
                .Include(so => so.Employee)
                    .ThenInclude(soe => soe.Employee)
                .Include(so => so.Employee)
                    .ThenInclude(soe => soe.EmployeePosition)
                .Include(so => so.Status)
                .Select(p => new SupplyOrderResponseOverview
                {
                    Id = p.Id,
                    TotalPrice = p.TotalPrice,
                    Employee = p.Employee,
                    CreatedDate = p.CreatedDate,
                    CompletedDate = p.CompletedDate,
                    Status = p.Status
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return supplyOrderResponseOverview;
        }
    }
}

public class GetAllSupplyOrdersEndpoint : CarterModule
{
    public GetAllSupplyOrdersEndpoint() : base("/api/supplyOrder")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("", async (ISender sender) =>
        {
            var query = new GetSupplyOrders.Query { };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result is null)
            {
                return null;
            }
            return result;
        });
    }
}