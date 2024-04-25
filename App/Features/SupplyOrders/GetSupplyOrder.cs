using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.SupplyOrders.GetSupplyOrder;

namespace App.Features.SupplyOrders;

public static class GetSupplyOrder
{
    public class Query : IRequest<SupplyOrderResponse>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, SupplyOrderResponse>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<SupplyOrderResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            var supplyOrder = await _applicationDbContext.SupplyOrders
                .Include(so => so.Employee)
                    .ThenInclude(soe => soe.Employee)
                .Include(so => so.Employee)
                    .ThenInclude(soe => soe.EmployeePosition)
                .Include(so => so.Status)
                .Where(p => p.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (supplyOrder is null)
            {
                return null;
            }

            var supplyOrderItems = await _applicationDbContext.SupplyItems
                .Include(p => p.Supply)
                .ThenInclude(s => s.Supplier)
                .Include(p => p.Supply)
                .ThenInclude(s => s.SupplyCategory)
                .Where(p => p.SupplyOrder.Id == request.Id)
                .Select(p => new SupplyItemResponseOverview
                {
                    Id = p.Id,
                    Supply = p.Supply,
                    Price = p.Price,
                    Quantity = p.Quantity
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var supplyOrderResponse = new SupplyOrderResponse
                {
                Id = supplyOrder.Id,
                TotalPrice = supplyOrder.TotalPrice,
                Employee = supplyOrder.Employee,
                CreatedDate = supplyOrder.CreatedDate,
                CompletedDate = supplyOrder.CompletedDate,
                Status = supplyOrder.Status,
                Items = supplyOrderItems
            };
            return supplyOrderResponse;
        }
    }
}

public class GetSupplyOrderEndpoint : CarterModule
{
    public GetSupplyOrderEndpoint() : base("/api/supplyOrder")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id}", async (Guid id, ISender sender) =>
        {
            var query = new GetSupplyOrder.Query { Id = id };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result is null)
            {
                return null;
            }
            return result;
        });
    }
}