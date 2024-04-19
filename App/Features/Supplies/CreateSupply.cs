using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using FluentValidation;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using static App.Features.Supplies.CreateSupply;
using Microsoft.EntityFrameworkCore;

namespace App.Features.Supplies;
public static class CreateSupply
{
    public class Command : IRequest<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public double Quantity { get; set; } = 0;
        public double Price { get; set; } = 0;
        public Guid SupplierId { get; set; }
        public Guid SupplyCategoryId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Name).NotEmpty();
            RuleFor(c => c.SupplierId).NotEmpty();
            RuleFor(c => c.SupplyCategoryId).NotEmpty();
        }
    }
    internal sealed class Handler : IRequestHandler<Command, Guid>
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IValidator<Command> _validator;
        public Handler(ApplicationDbContext applicationDbContext, IValidator<Command> validator)
        {
            _applicationDbContext = applicationDbContext;
            _validator = validator;
        }
        public async Task<Guid> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return Guid.Empty;
            }
            var supplier = await _applicationDbContext.Suppliers
                    .Where(p => p.Id == request.SupplierId)
                .FirstOrDefaultAsync(cancellationToken);

            if (supplier is null)
            {
                return Guid.Empty;
            }

            var supplyCategory = await _applicationDbContext.SupplyCategories
                    .Where(p => p.Id == request.SupplyCategoryId)
                .FirstOrDefaultAsync(cancellationToken);

            if (supplyCategory is null)
            {
                return Guid.Empty;
            }

            var supply = new Supply
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Quantity = request.Quantity,
                Price = request.Price,
                SupplyCategory = supplyCategory,
                Supplier = supplier
            };

            _applicationDbContext.Add(supply);
            await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return supply.Id;
        }
    }
}
public class SupplyEndpoint : CarterModule
{
    public SupplyEndpoint() : base("/api/supply")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("", async (CreateSupplyRequest request, ISender sender, HttpContext context) =>
        {
            var command = new CreateSupply.Command
            {
                Name = request.Name,
                Quantity = request.Quantity,
                Price = request.Price,
                SupplyCategoryId = request.SupplyCategoryId,
                SupplierId = request.SupplierId
            };

            var result = await sender.Send(command).ConfigureAwait(false);

            if (result != Guid.Empty)
            {
                return Results.Created($"/api/supply/{result}", result);
            }
            else
            {
                return Results.BadRequest("Invalid request or validation failed.");
            }
        });
    }
}