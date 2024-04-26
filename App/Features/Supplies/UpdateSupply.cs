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
using CSharpFunctionalExtensions;

namespace App.Features.Supplies;
public static class UpdateSupply
{
    public class Command : IRequest<Result<SupplyResponse, IEnumerable<string>>>
    {
        public Guid Id { get; set; }
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
    internal sealed class Handler : IRequestHandler<Command, Result<SupplyResponse, IEnumerable<string>>>
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IValidator<Command> _validator;
        public Handler(ApplicationDbContext applicationDbContext, IValidator<Command> validator)
        {
            _applicationDbContext = applicationDbContext;
            _validator = validator;
        }
        public async Task<Result<SupplyResponse, IEnumerable<string>>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return Result.Failure<SupplyResponse, IEnumerable<string>>(validationResult.Errors.Select(e => e.ErrorMessage));
            }
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
                return Result.Failure<SupplyResponse, IEnumerable<string>>(new List<string> { "Invalid supplyId value"});
            }
            var supplier = await _applicationDbContext.Suppliers
                    .Where(p => p.Id == request.SupplierId)
                .FirstOrDefaultAsync(cancellationToken);

            if (supplier is null)
            {
                return Result.Failure<SupplyResponse, IEnumerable<string>>(new List<string> { "Invalid supplierId value" });
            }

            var supplyCategory = await _applicationDbContext.SupplyCategories
                    .Where(p => p.Id == request.SupplyCategoryId)
                .FirstOrDefaultAsync(cancellationToken);

            if (supplyCategory is null)
            {
                return Result.Failure<SupplyResponse, IEnumerable<string>>(new List<string> { "Invalid supplyCategoryId value" });
            }
            var updatedSupply = new Supply
            {
                Id = supplyResponse.Id,
                Name = request.Name,
                Quantity = request.Quantity,
                Price = request.Price,
                Supplier = supplier,
                SupplyCategory = supplyCategory
            };

            var updatedSupplyResponse = new SupplyResponse 
            {
                Id = supplyResponse.Id,
                Name = request.Name,
                Quantity = request.Quantity,
                Price = request.Price,
                Supplier = supplier,
                SupplyCategory = supplyCategory
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
                SupplierId = request.SupplierId,
                SupplyCategoryId = request.SupplyCategoryId
            };

            var result = await sender.Send(command).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                return Results.Created($"/api/supply/{result.Value}", result.Value);
            }
            else
            {
                return Results.BadRequest(result.Error);
            }
        }).RequireAuthorization("SuperUserPolicy");
    }
}