using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using static App.Features.SupplyOrderItems.UpdateSupplyOrderItem;
using CSharpFunctionalExtensions;

namespace App.Features.SupplyOrderItems;
public static class UpdateSupplyOrderItem
{
    public class Command : IRequest<Result<SupplyItemResponse, IEnumerable<string>>>
    {
        public Guid Id { get; set; }
        public double? Quantity { get; set; } = 0;
        public Guid? SupplyId { get; set; }
        public Guid? SupplyOrderId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Quantity > 0).NotEmpty();
            RuleFor(c => c.SupplyId).NotEmpty();
            RuleFor(c => c.SupplyOrderId).NotEmpty();
        }
    }
    internal sealed class Handler : IRequestHandler<Command, Result<SupplyItemResponse, IEnumerable<string>>>
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IValidator<Command> _validator;
        public Handler(ApplicationDbContext applicationDbContext, IValidator<Command> validator)
        {
            _applicationDbContext = applicationDbContext;
            _validator = validator;
        }
        public async Task<Result<SupplyItemResponse, IEnumerable<string>>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return Result.Failure<SupplyItemResponse, IEnumerable<string>>(validationResult.Errors.Select(e => e.ErrorMessage));
            }
           
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
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
            if (supplyOrderItem is null)
            {
                return Result.Failure<SupplyItemResponse, IEnumerable<string>>(new List<string> { "Invalid supplyOrderItemId value" });
            }

            var supplyOrder = supplyOrderItem.SupplyOrder;
            if (request.SupplyOrderId != null)
            {
                supplyOrder = await _applicationDbContext.SupplyOrders
                    .Where(so => so.Id == request.SupplyOrderId)
                    .Include(so => so.Status)
                .FirstOrDefaultAsync(cancellationToken);
                if (supplyOrder is null)
                {
                    return Result.Failure<SupplyItemResponse, IEnumerable<string>>(new List<string> { "Invalid supplyOrderId value" });
                }
                if (supplyOrder.Status.Name == "Finalized")
                {
                    return Result.Failure<SupplyItemResponse, IEnumerable<string>>(new List<string> { "You can't edit order in Finalized state!" });
                }
                else
                {
                    supplyOrderItem.SupplyOrder = supplyOrder;
                }

            }

            var supply = supplyOrderItem.Supply;
            if (request.SupplyId != null)
            {
                supply = await _applicationDbContext.Supplies
                    .Where(p => p.Id == request.SupplyId)
                .FirstOrDefaultAsync(cancellationToken);
                if (supply is null)
                {
                    return Result.Failure<SupplyItemResponse, IEnumerable<string>>(new List<string> { "Invalid supplyId value" });
                }
                else
                {
                    supplyOrderItem.Supply = supply;
                }
            }

            
            var quantity = supplyOrderItem.Quantity;
            var price = supplyOrderItem.Price;
            if (request.Quantity != null)
            {
                //update the supplyOrder total price
                quantity = (double)request.Quantity;
                supplyOrder.TotalPrice = supplyOrder.TotalPrice - price;
                price = quantity * supply.Price;
                supplyOrder.TotalPrice = supplyOrder.TotalPrice + price;
                _applicationDbContext.Entry(supplyOrder).State = EntityState.Detached;
                _applicationDbContext.Attach(supplyOrder);
                _applicationDbContext.Entry(supplyOrder).State = EntityState.Modified;
                supplyOrderItem.Quantity = quantity;
                supplyOrderItem.Price = price;

            }

            var supplyOrderResponse = new SupplyItemResponse
            {
                Id = supplyOrderItem.Id,
                Price = supplyOrderItem.Price,
                Quantity = supplyOrderItem.Quantity,
                Supply = supplyOrderItem.Supply,
                SupplyOrder = supplyOrderItem.SupplyOrder
            };

            _applicationDbContext.Entry(supplyOrderItem).State = EntityState.Detached;
            _applicationDbContext.Attach(supplyOrderItem);
            _applicationDbContext.Entry(supplyOrderItem).State = EntityState.Modified;

            await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return supplyOrderResponse;

        }
    }
}
public class UpdateSupplyOrderItemEndpoint : CarterModule
{
    public UpdateSupplyOrderItemEndpoint() : base("/api/supplyOrderItem")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id}", async (Guid id, CreateSupplySupplyItemRequest request, ISender sender, HttpContext context) =>
        {
            var command = new UpdateSupplyOrderItem.Command
            {
                Id = id,
                Quantity = request.Quantity,
                SupplyOrderId = request.SupplyOrderId,
                SupplyId = request.SupplyId
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