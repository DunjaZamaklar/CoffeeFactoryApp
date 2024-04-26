using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using FluentValidation;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using static App.Features.Suppliers.CreateSupplier;
using CSharpFunctionalExtensions;

namespace App.Features.Suppliers;
public static class CreateSupplier
{
    public class Command : IRequest<Result<Guid, IEnumerable<string>>>
    {
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
            var supplier = new Supplier
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Address = request.Address,
                City = request.City,
                Country = request.Country,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email
            };

            _applicationDbContext.Add(supplier);
            await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return supplier.Id;
        }
    }
}
public class SupplierEndpoint : CarterModule
{
    public SupplierEndpoint() : base("/api/supplier")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("", async (CreateSupplierRequest request, ISender sender, HttpContext context) =>
        {
            var command = new CreateSupplier.Command
            {
                Name = request.Name,
                Address = request.Address,
                City = request.City,
                Country = request.Country,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email
            };

            var result = await sender.Send(command).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                return Results.Created($"/api/supplier/{result.Value}", result.Value);
            }
            else
            {
                return Results.BadRequest(result.Error);
            }
        }).RequireAuthorization("AdminPolicy", "SuperUserPolicy");
    }
}