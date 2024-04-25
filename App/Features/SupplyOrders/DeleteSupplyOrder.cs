using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using App.Data.Migrations;
using Carter;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.SupplyOrders.DeleteSupplyOrder;

namespace App.Features.SupplyOrders;

public static class DeleteSupplyOrder
{

    public class Query : IRequest<Result<Boolean, IEnumerable<string>>>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, Result<Boolean, IEnumerable<string>>>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<Result<Boolean, IEnumerable<string>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var entityToDelete = await _applicationDbContext.SupplyOrders
                .Where(so => so.Id == request.Id)
                .Include(so => so.Status)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (entityToDelete != null)
            {
                if(entityToDelete.Status.Name == "Finalized")
                {
                    return Result.Failure<Boolean, IEnumerable<string>>(new List<string> { "You can't remove order in Finalized state!" });
                }
                _applicationDbContext.Entry(entityToDelete).State = EntityState.Detached;
                var supplyItems = await _applicationDbContext.SupplyItems.Where(p => p.SupplyOrder.Id == request.Id)
                .Include(si => si.Supply)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

                if (supplyItems != null)
                {
                    _applicationDbContext.SupplyItems.RemoveRange(supplyItems);
                }
                _applicationDbContext.SupplyOrders.Remove(entityToDelete);
                // Save changes to the database
                await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
            return Result.Failure<Boolean, IEnumerable<string>>(new List<string> { "Invalid supplyOrderId value" });
        }
    }
}

public class DeleteSupplyOrderEndpoint : CarterModule
{
    public DeleteSupplyOrderEndpoint() : base("/api/supplyOrder")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id}", async (Guid id, ISender sender) =>
        {
            var query = new DeleteSupplyOrder.Query { Id = id };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return Results.Ok();
            }
            else
            {
                return Results.NotFound(result.Error);
            }
        });
    }
}