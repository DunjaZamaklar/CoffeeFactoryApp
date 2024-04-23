using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using FluentValidation;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using static App.Features.SupplyOrders.CreateSupplyOrder;
using Microsoft.EntityFrameworkCore;
using CSharpFunctionalExtensions;

namespace App.Features.SupplyOrders;
public static class CreateSupplyOrder
{
    public class Command : IRequest<Result<Guid, IEnumerable<string>>>
    {
        public Guid EmployeeContractId { get; set; }
        public Guid StatusId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.EmployeeContractId).NotEmpty();
            RuleFor(c => c.StatusId).NotEmpty();
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
                return Result.Failure<Guid, IEnumerable<string>>(validationResult.Errors.Select(e => e.ErrorMessage));
            }
            var employee = await _applicationDbContext.EmployeeContracts
                    .Where(p => p.Id == request.EmployeeContractId)
                .FirstOrDefaultAsync(cancellationToken);

            if (employee is null)
            {
                return Result.Failure<Guid, IEnumerable<string>>(new List<string> {"Invalid employeeContractId"});
            }

            var status = await _applicationDbContext.SupplyOrderStatus
                    .Where(p => p.Id == request.StatusId)
                .FirstOrDefaultAsync(cancellationToken);

            if (status is null)
            {
                return Result.Failure<Guid, IEnumerable<string>>(new List<string> { "Invalid statusId" });
            }

            var supplyOrder = new SupplyOrder
            {
                Id = Guid.NewGuid(),
                TotalPrice = 0,
                Employee = employee,
                Status = status,
                CompletedDate = null,
                CreatedDate = DateTime.UtcNow
            };

            _applicationDbContext.Add(supplyOrder);
            await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return supplyOrder.Id;
        }
    }
}
public class SupplyOrderEndpoint : CarterModule
{
    public SupplyOrderEndpoint() : base("/api/supplyOrder")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("", async (CreateSupplyOrderRequest request, ISender sender, HttpContext context) =>
        {
            var command = new CreateSupplyOrder.Command
            {
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
        });
    }
}