using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using static App.Features.EmployeePositions.CreateEmployeePositions;

namespace App.Features.EmployeePositions;
public static class CreateEmployeePositions
{
    public class Command : IRequest<Result<Guid, IEnumerable<string>>>
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
            var employeePosition = new EmployeePosition
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description
            };

            _applicationDbContext.Add(employeePosition);
            await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return employeePosition.Id;
        }
    }
}
public class EmployeePositionEndpoint : CarterModule
{
    public EmployeePositionEndpoint() : base("/api/employeePosition")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("", async (CreateEmployeePositionRequest request, ISender sender, HttpContext context) =>
        {
            var command = new CreateEmployeePositions.Command
            {
                Name = request.Name,
                Description = request.Description
            };

            var result = await sender.Send(command).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                return Results.Created($"/api/employeePosition/{result.Value}", result.Value);
            }
            else
            {
                return Results.BadRequest(result.Error);
            }
        }).RequireAuthorization("SuperUserPolicy");
    }
}