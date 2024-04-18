using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using static App.Features.Suppliers.UpdateSupplier;

namespace App.Features.Suppliers;
public static class UpdateSupplier
{
    public class Command : IRequest<SupplierResponse>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Email).EmailAddress();
            RuleFor(c => c.Name).NotEmpty();
            RuleFor(c => c.Address).NotEmpty();
        }
    }
    internal sealed class Handler : IRequestHandler<Command, SupplierResponse>
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IValidator<Command> _validator;
        public Handler(ApplicationDbContext applicationDbContext, IValidator<Command> validator)
        {
            _applicationDbContext = applicationDbContext;
            _validator = validator;
        }
        public async Task<SupplierResponse> Handle(Command request, CancellationToken cancellationToken)
        {

            var supplierResponse = await _applicationDbContext.Suppliers
                .Where(p => p.Id == request.Id)
                .Select(p => new SupplierResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Address = p.Address,
                    City = p.City,
                    Country = p.Country,
                    PhoneNumber = p.PhoneNumber,
                    Email = p.Email
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (supplierResponse is null)
            {
                return null;
            }

            var updatedSupplier = new Supplier
            {
                Id = supplierResponse.Id,
                Name = request.Name,
                Address = request.Address,
                City = request.City,
                Country = request.Country,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email
            };

            var updatedSupplierResponse = new SupplierResponse 
            {
                Id = supplierResponse.Id,
                Name = request.Name,
                Address = request.Address,
                City = request.City,
                Country = request.Country,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email
            };

            // Attach the updated entity
            _applicationDbContext.Attach(updatedSupplier);

            // Set the entity state to modified
            _applicationDbContext.Entry(updatedSupplier).State = EntityState.Modified;

            // Save changes to the database
            await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return updatedSupplierResponse;

        }
    }
}
public class UpdateSupplierEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("supplier/{id}", async (Guid id, CreateSupplierRequest request, ISender sender, HttpContext context) =>
        {
            var command = new UpdateSupplier.Command
            {
                Id = id,
                Name = request.Name,
                Address = request.Address,
                City = request.City,
                Country = request.Country,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email
            };

            var result = await sender.Send(command).ConfigureAwait(false);

            if (result != null)
            {
                return Results.Created($"/api/supplier/{result}", result);
            }
            else
            {
                return Results.BadRequest("Invalid request or validation failed.");
            }
        });
    }
}