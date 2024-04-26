using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using static App.Features.SupplyOrders.UpdateSupplyOrder;
using CSharpFunctionalExtensions;

namespace App.Features.SupplyOrders;
public static class UpdateSupplyOrder
{
    public class Command : IRequest<Result<SupplyOrderResponse, IEnumerable<string>>>
    {
        public Guid Id { get; set; }
        public Guid? EmployeeContractId { get; set; }
        public Guid StatusId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.StatusId).NotEmpty();
        }
    }
    internal sealed class Handler : IRequestHandler<Command, Result<SupplyOrderResponse, IEnumerable<string>>>
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IValidator<Command> _validator;
        public Handler(ApplicationDbContext applicationDbContext, IValidator<Command> validator)
        {
            _applicationDbContext = applicationDbContext;
            _validator = validator;
        }
        public async Task<Result<SupplyOrderResponse, IEnumerable<string>>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return Result.Failure<SupplyOrderResponse, IEnumerable<string>>(validationResult.Errors.Select(e => e.ErrorMessage));
            }
            var supplyOrder = await _applicationDbContext.SupplyOrders
                .Where(so => so.Id == request.Id)
                .Include(so => so.Status)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (supplyOrder is null)
            {
                return Result.Failure<SupplyOrderResponse, IEnumerable<string>>(new List<string> { "Invalid supplyOrderId value" });
            }
            if (supplyOrder.Status.Name == "Finalized")
            {
                return Result.Failure<SupplyOrderResponse, IEnumerable<string>>(new List<string> { "You can't edit order in Finalized state!" });
            }

            _applicationDbContext.Entry(supplyOrder).State = EntityState.Detached;
            var supplyOrderStatus = await _applicationDbContext.SupplyOrderStatus
                    .Where(p => p.Id == request.StatusId)
                .FirstOrDefaultAsync(cancellationToken);

            if (supplyOrderStatus is null)
            {
                return Result.Failure<SupplyOrderResponse, IEnumerable<string>>(new List<string> { "Invalid statusId value" });
            }
            _applicationDbContext.Entry(supplyOrderStatus).State = EntityState.Detached;
            var employee = supplyOrder.Employee;
            if (request.EmployeeContractId != null) {
                employee = await _applicationDbContext.EmployeeContracts
                    .Where(p => p.Id == request.EmployeeContractId)
                .FirstOrDefaultAsync(cancellationToken);
                if (employee is null)
                {
                    return Result.Failure<SupplyOrderResponse, IEnumerable<string>>(new List<string> { "Invalid employeeContractId value" });
                }
                _applicationDbContext.Entry(employee).State = EntityState.Detached;
            }
            
            var updatedSupplyOrder = new SupplyOrder
            {
                Id = supplyOrder.Id,
                CreatedDate = supplyOrder.CreatedDate,
                CompletedDate = supplyOrder.CompletedDate,
                Employee = employee,
                Status = supplyOrderStatus,
                TotalPrice = supplyOrder.TotalPrice
            };
            if(supplyOrderStatus.Name == "Finalized")
            {
                updatedSupplyOrder.CompletedDate = DateTime.UtcNow;
                var orderItems = await _applicationDbContext.SupplyItems
               .Where(si => si.SupplyOrder.Id == request.Id)
               .Include(si => si.Supply)
               .ToListAsync(cancellationToken)
               .ConfigureAwait(false);

                if(orderItems.Count > 0)
                {
                    foreach (var item in orderItems)
                    {
                        var supply = item.Supply;
                        if (supply != null)
                        {
                            supply.Quantity = supply.Quantity + item.Quantity;
                            _applicationDbContext.Attach(supply);
                            _applicationDbContext.Entry(supply).State = EntityState.Modified;
                        }
                    }
                }
            }
            var supplyOrderItems = await _applicationDbContext.SupplyItems
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
                Id = updatedSupplyOrder.Id,
                TotalPrice = updatedSupplyOrder.TotalPrice,
                Employee = updatedSupplyOrder.Employee,
                CreatedDate = updatedSupplyOrder.CreatedDate,
                CompletedDate = updatedSupplyOrder.CompletedDate,
                Status = updatedSupplyOrder.Status,
                Items = supplyOrderItems
            };

            _applicationDbContext.Attach(updatedSupplyOrder);
            _applicationDbContext.Entry(updatedSupplyOrder).State = EntityState.Modified;
            await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return supplyOrderResponse;

        }
    }
}
public class UpdateSupplyOrderEndpoint : CarterModule
{
    public UpdateSupplyOrderEndpoint() : base("/api/supplyOrder")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id}", async (Guid id, UpdateSupplyOrderRequest request, ISender sender, HttpContext context) =>
        {
            var command = new UpdateSupplyOrder.Command
            {
                Id = id,
                EmployeeContractId = request.EmployeeContractId,
                StatusId = request.StatusId
            };

            var result = await sender.Send(command).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                return Results.Created($"/api/supplyOrder/{result.Value}", result.Value);
            }
            else
            {
                return Results.BadRequest(result.Error);
            }
        }).RequireAuthorization("SuperUserPolicy");
    }
}