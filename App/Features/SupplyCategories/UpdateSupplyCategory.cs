using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.SupplyCategories.UpdateSupplyCategory;

namespace App.Features.SupplyCategories;
public static class UpdateSupplyCategory
{
    public class Command : IRequest<SupplyCategoryResponse>
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
    internal sealed class Handler : IRequestHandler<Command, SupplyCategoryResponse>
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IValidator<Command> _validator;
        public Handler(ApplicationDbContext applicationDbContext, IValidator<Command> validator)
        {
            _applicationDbContext = applicationDbContext;
            _validator = validator;
        }
        public async Task<SupplyCategoryResponse> Handle(Command request, CancellationToken cancellationToken)
        {

            var supplyCategoryResponse = await _applicationDbContext.SupplyCategories
                .Where(p => p.Id == request.Id)
                .Select(p => new SupplyCategoryResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (supplyCategoryResponse is null)
            {
                return null;
            }

            var updatedSupplyCategory = new SupplyCategory
            {
                Id = supplyCategoryResponse.Id,
                Name = request.Name,
                Description = request.Description
            };

            var updatedSupplyCategoryResponse = new SupplyCategoryResponse
            {
                Id = supplyCategoryResponse.Id,
                Name = request.Name,
                Description = request.Description
            };

            // Attach the updated entity
            _applicationDbContext.Attach(updatedSupplyCategory);

            // Set the entity state to modified
            _applicationDbContext.Entry(updatedSupplyCategory).State = EntityState.Modified;

            // Save changes to the database
            await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return updatedSupplyCategoryResponse;

        }
    }
}
public class UpdateSupplyCategoryEndpoint : CarterModule
{
    public UpdateSupplyCategoryEndpoint() : base("/api/supplyCategory")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id}", async (Guid id, CreateSupplyCategoryRequest request, ISender sender, HttpContext context) =>
        {
            var command = new UpdateSupplyCategory.Command
            {
                Id = id,
                Name = request.Name,
                Description = request.Description
            };

            var result = await sender.Send(command).ConfigureAwait(false);

            if (result != null)
            {
                return Results.Created($"/api/supplyCategory/{result}", result);
            }
            else
            {
                return Results.BadRequest("Invalid request or validation failed.");
            }
        });
    }
}