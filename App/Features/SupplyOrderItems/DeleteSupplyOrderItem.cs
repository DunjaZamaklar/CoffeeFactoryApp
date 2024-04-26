using App.Contracts;
using App.Data.Database;
using App.Features.SupplyOrders;
using Carter;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;
using static App.Features.SupplyOrderItems.DeleteSupplyOrderItem;

namespace App.Features.SupplyOrderItems;

public static class DeleteSupplyOrderItem
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
            var entityToDelete = await _applicationDbContext.SupplyItems
                .Where(si => si.Id == request.Id)
                .Include(si => si.Supply)
                .Include(si => si.SupplyOrder)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (entityToDelete != null)
            {
                var supplyOrder = await _applicationDbContext.SupplyOrders
                 .Where(so => so.Id == entityToDelete.SupplyOrder.Id)
                 .Include(so => so.Status)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

                //update supplyorder total price value
                if (supplyOrder != null)
                {
                    if(supplyOrder.Status.Name == "Finalized")
                    {
                        return Result.Failure<Boolean, IEnumerable<string>>(new List<string> { "You can't remove order in Finalized state!" });
                    }
                    else
                    {
                        supplyOrder.TotalPrice = supplyOrder.TotalPrice - entityToDelete.Price;
                        _applicationDbContext.Attach(supplyOrder);
                        _applicationDbContext.Entry(supplyOrder).State = EntityState.Modified;
                    }

                }


                _applicationDbContext.SupplyItems.Remove(entityToDelete);
                // Save changes to the database
                await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
            return Result.Failure<Boolean, IEnumerable<string>>(new List<string> { "Invalid supplyOrderItemId value" });
        }
    }
}

public class DeleteSupplyOrderItemEndpoint : CarterModule
{
    public DeleteSupplyOrderItemEndpoint() : base("/api/supplyOrderItem")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id}", async (Guid id, ISender sender) =>
        {
            var query = new DeleteSupplyOrderItem.Query { Id = id };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return Results.Ok();
            }
            else
            {
                return Results.NotFound(result.Error);
            }
        }).RequireAuthorization("SuperUserPolicy");
    }
}