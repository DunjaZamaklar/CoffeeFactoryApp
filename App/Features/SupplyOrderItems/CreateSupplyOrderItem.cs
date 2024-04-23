using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using FluentValidation;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using static App.Features.SupplyOrderItems.CreateSupplyOrderItem;
using Microsoft.EntityFrameworkCore;
using CSharpFunctionalExtensions;

namespace App.Features.SupplyOrderItems;
public static class CreateSupplyOrderItem
{
    public class Command : IRequest<Result<Guid,IEnumerable<string>>>
    {
        public double Quantity { get; set; } = 0;
        public Guid SupplyId { get; set; }
        public Guid SupplyOrderId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Quantity).GreaterThan(0);
            RuleFor(c => c.SupplyId).NotEmpty();
            RuleFor(c => c.SupplyOrderId).NotEmpty();
        }
    }
    internal sealed class Handler : IRequestHandler<Command, Result<Guid, IEnumerable<string>>>
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IValidator<Command> _validator;
        public Handler(ApplicationDbContext applicationDbContext, IValidator<Command> validator)
        {
            _applicationDbContext = applicationDbContext;
            _validator = validator;
        }
        public async Task<Result<Guid, IEnumerable<string>>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return Result.Failure<Guid,IEnumerable<string>>(validationResult.Errors.Select(e => e.ErrorMessage));
            }


            var supply = await _applicationDbContext.Supplies
                    .Where(p => p.Id == request.SupplyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (supply is null)
            {
                return Result.Failure<Guid, IEnumerable<string>>(new List<string> { "Invalid supplyId value" });
            }


            var supplyOrder = await _applicationDbContext.SupplyOrders
                    .Where(so => so.Id == request.SupplyOrderId)
                    .Include(so => so.Status)
                .FirstOrDefaultAsync(cancellationToken);

            if (supplyOrder is null)
            {
                return Result.Failure<Guid, IEnumerable<string>>( new List<string> { "Invalid supplyOrderId value" });
            }
            else
            {
                if(supplyOrder.Status.Name == "Finalized")
                {
                    return Result.Failure<Guid, IEnumerable<string>>(new List<string> { "You can't edit order in Finalized state!" });
                }
                else
                {
                    supplyOrder.TotalPrice = supplyOrder.TotalPrice + supply.Price * request.Quantity;
                    _applicationDbContext.Entry(supplyOrder).State = EntityState.Detached;
                    _applicationDbContext.Attach(supplyOrder);
                    _applicationDbContext.Entry(supplyOrder).State = EntityState.Modified;
                }
            }

            var supplyItem = new SupplyItem
            {
                Id = Guid.NewGuid(),
                Price = supply.Price * request.Quantity,
                Quantity = request.Quantity,
                Supply = supply,
                SupplyOrder = supplyOrder
            };
            
            _applicationDbContext.Add(supplyItem);
            await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return supplyItem.Id;
        }
    }
}
public class SupplyOrderItemEndpoint : CarterModule
{
    public SupplyOrderItemEndpoint() : base("/api/supplyOrderItem")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("", async (CreateSupplySupplyItemRequest request, ISender sender, HttpContext context) =>
        {
            var command = new CreateSupplyOrderItem.Command
            {
                 Quantity = request.Quantity,
                 SupplyId = request.SupplyId,
                 SupplyOrderId = request.SupplyOrderId
            };

            var result = await sender.Send(command).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                return Results.Created($"/api/supplyOrderItem/{result.Value}", result.Value);
            }
            else
            {
                return Results.BadRequest(result.Error);
            }
        });
    }
}