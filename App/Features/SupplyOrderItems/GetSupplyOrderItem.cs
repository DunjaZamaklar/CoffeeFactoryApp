using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using App.Data.Migrations;
using Carter;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.SupplyOrderItems.GetSupplyOrderItem;

namespace App.Features.SupplyOrderItems;

public static class GetSupplyOrderItem
{
    public class Query : IRequest<Result<SupplyItemResponse, IEnumerable<string>>>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, Result<SupplyItemResponse, IEnumerable<string>>>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<Result<SupplyItemResponse, IEnumerable<string>>> Handle(Query request, CancellationToken cancellationToken)
        {

            var supplyOrderItem = await _applicationDbContext.SupplyItems
                .Where(si => si.Id == request.Id)
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
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
            if (supplyOrderItem is null)
            {
                return Result.Failure<SupplyItemResponse, IEnumerable<string>>(new List<string> { "Invalid supplyOrderItemId value" }); ;
            }
            return supplyOrderItem;
        }
    }
}

public class GetSupplyOrderItemEndpoint : CarterModule
{
    public GetSupplyOrderItemEndpoint() : base("/api/supplyOrderItem")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id}", async (Guid id, ISender sender) =>
        {
            var query = new GetSupplyOrderItem.Query { Id = id };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return result.Value;
            }

            return null;

        }).RequireAuthorization("UserPolicy");
    }
}