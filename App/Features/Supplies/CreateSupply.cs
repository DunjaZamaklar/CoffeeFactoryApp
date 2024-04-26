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
using CSharpFunctionalExtensions;

namespace App.Features.Supplies;
public static class CreateSupply
{
    public class Command : IRequest<Result<Guid, IEnumerable<string>>>
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
            var supplier = await _applicationDbContext.Suppliers
                    .Where(p => p.Id == request.SupplierId)
                .FirstOrDefaultAsync(cancellationToken);

            if (supplier is null)
            {
                return Result.Failure<Guid, IEnumerable<string>>(new List<string> { "Invalid supplierId value" });
            }

            var supplyCategory = await _applicationDbContext.SupplyCategories
                    .Where(p => p.Id == request.SupplyCategoryId)
                .FirstOrDefaultAsync(cancellationToken);

            if (supplyCategory is null)
            {
                return Result.Failure<Guid, IEnumerable<string>>(new List<string> { "Invalid supplyCategoryId value" });
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