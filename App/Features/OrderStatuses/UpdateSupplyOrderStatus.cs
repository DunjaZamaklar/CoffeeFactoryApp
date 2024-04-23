using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.OrderStatuses.UpdateSupplyOrderStatus;

namespace App.Features.OrderStatuses;
public static class UpdateSupplyOrderStatus
{
    public class Command : IRequest<Result<SupplyOrderStatusResponse, IEnumerable<string>>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Name).NotEmpty();
        }
    }
    internal sealed class Handler : IRequestHandler<Command, Result<SupplyOrderStatusResponse, IEnumerable<string>>>
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IValidator<Command> _validator;
        public Handler(ApplicationDbContext applicationDbContext, IValidator<Command> validator)
        {
            _applicationDbContext = applicationDbContext;
            _validator = validator;
        }
        public async Task<Result<SupplyOrderStatusResponse, IEnumerable<string>>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return Result.Failure<SupplyOrderStatusResponse, IEnumerable<string>>(validationResult.Errors.Select(e => e.ErrorMessage));
            }
            var supplyOrderStatusResponse = await _applicationDbContext.SupplyOrderStatus
                .Where(p => p.Id == request.Id)
                .Select(p => new SupplyOrderStatusResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (supplyOrderStatusResponse is null)
            {
                return Result.Failure<SupplyOrderStatusResponse, IEnumerable<string>>(new List<string> { "Invalid statusId" });
            }

            var updatedSupplyOrderStatus = new SupplyOrderStatus
            {
                Id = supplyOrderStatusResponse.Id,
                Name = request.Name,
                Description = request.Description
            };

            var updatedSupplyOrderStatusresponse = new SupplyOrderStatusResponse
            {
                Id = supplyOrderStatusResponse.Id,
                Name = request.Name,
                Description = request.Description
            };

            // Attach the updated entity
            _applicationDbContext.Attach(updatedSupplyOrderStatus);

            // Set the entity state to modified
            _applicationDbContext.Entry(updatedSupplyOrderStatus).State = EntityState.Modified;

            // Save changes to the database
            await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return updatedSupplyOrderStatusresponse;

        }
    }
}
public class UpdateSupplyOrderStatusEndpoint : CarterModule
{
    public UpdateSupplyOrderStatusEndpoint() : base("/api/supplyOrderStatus")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id}", async (Guid id, CreateSupplyOrderStatusRequest request, ISender sender, HttpContext context) =>
        {
            var command = new UpdateSupplyOrderStatus.Command
            {
                Id = id,
                Name = request.Name,
                Description = request.Description
            };

            var result = await sender.Send(command).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                return Results.Created($"/api/supplyOrderStatus/{result.Value}", result.Value);
            }
            else
            {
                return Results.BadRequest(result.Error);
            }
        });
    }
}