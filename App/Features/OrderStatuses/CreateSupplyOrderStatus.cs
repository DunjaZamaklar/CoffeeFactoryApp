using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using FluentValidation;
using MediatR;
using static App.Features.OrderStatuses.CreateSupplyOrderStatus;

namespace App.Features.OrderStatuses;
public static class CreateSupplyOrderStatus
{
    public class Command : IRequest<Guid>
    {
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
            var supplyOrderStatus = new SupplyOrderStatus
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description
            };

            _applicationDbContext.Add(supplyOrderStatus);
            await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return supplyOrderStatus.Id;
        }
    }
}
public class SupplyOrderStatusEndpoint : CarterModule
{
    public SupplyOrderStatusEndpoint() : base("/api/supplyOrderStatus")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("", async (CreateProductRequest request, ISender sender, HttpContext context) =>
        {
            var command = new CreateSupplyOrderStatus.Command
            {
                Name = request.Name,
                Description = request.Description
            };

            var result = await sender.Send(command).ConfigureAwait(false);

            if (result != Guid.Empty)
            {
                return Results.Created($"/api/supplyOrderStatus/{result}", result);
            }
            else
            {
                return Results.BadRequest("Invalid request or validation failed.");
            }
        });
    }
}