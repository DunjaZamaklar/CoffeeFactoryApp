using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using static App.Features.Supplies.UpdateSupply;

namespace App.Features.Supplies;
public static class UpdateSupply
{
    public class Command : IRequest<SupplyResponse>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Quantity { get; set; } = 0;
        public double Price { get; set; } = 0;
        public Supplier Supplier { get; set; }
        public SupplyCategory SupplyCategory { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Name).NotEmpty();
            RuleFor(c => c.Supplier).NotEmpty();
            RuleFor(c => c.SupplyCategory).NotEmpty();
        }
    }
    internal sealed class Handler : IRequestHandler<Command, SupplyResponse>
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IValidator<Command> _validator;
        public Handler(ApplicationDbContext applicationDbContext, IValidator<Command> validator)
        {
            _applicationDbContext = applicationDbContext;
            _validator = validator;
        }
        public async Task<SupplyResponse> Handle(Command request, CancellationToken cancellationToken)
        {

            var supplyResponse = await _applicationDbContext.Supplies
                .Where(p => p.Id == request.Id)
                .Select(p => new SupplyResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Quantity = p.Quantity,
                    Price = p.Price,
                    SupplyCategory = p.SupplyCategory,
                    Supplier = p.Supplier
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (supplyResponse is null)
            {
                return null;
            }

            var updatedSupply = new Supply
            {
                Id = supplyResponse.Id,
                Name = request.Name,
                Quantity = request.Quantity,
                Price = request.Price,
                Supplier = request.Supplier,
                SupplyCategory = request.SupplyCategory
            };

            var updatedSupplyResponse = new SupplyResponse 
            {
                Id = supplyResponse.Id,
                Name = request.Name,
                Quantity = request.Quantity,
                Price = request.Price,
                Supplier = request.Supplier,
                SupplyCategory = request.SupplyCategory
            };

            // Attach the updated entity
            _applicationDbContext.Attach(updatedSupply);

            // Set the entity state to modified
            _applicationDbContext.Entry(updatedSupply).State = EntityState.Modified;

            // Save changes to the database
            await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return updatedSupplyResponse;

        }
    }
}
public class UpdateSupplyEndpoint : CarterModule
{
    public UpdateSupplyEndpoint() : base("/api/supply")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id}", async (Guid id, CreateSupplyRequest request, ISender sender, HttpContext context) =>
        {
            var command = new UpdateSupply.Command
            {
                Id = id,
                Name = request.Name,
                Quantity = request.Quantity,
                Price = request.Price,
                Supplier = request.Supplier,
                SupplyCategory = request.SupplyCategory
            };

            var result = await sender.Send(command).ConfigureAwait(false);

            if (result != null)
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