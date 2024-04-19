using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.EmployeeContracts.CreateEmployeeContract;

namespace App.Features.EmployeeContracts;
public static class CreateEmployeeContract
{
    public class Command : IRequest<Guid>
    {
        public string Type { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public Boolean ActiveFlag { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid EmployeePositionId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.EmployeeId).NotEmpty().WithMessage("EmployeeId is required.");
            RuleFor(c => c.EmployeePositionId).NotEmpty().WithMessage("EmployeePositionId is required.");
            RuleFor(c => c.Type).NotEmpty().WithMessage("Type is required.");
            RuleFor(c => c.StartDate)
             .NotEmpty().WithMessage("Start Date is required.")
            .Must(date => date <= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Start date cannot be after today's date.");
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
            var employee = await _applicationDbContext.Employees
                    .Where(p => p.Id == request.EmployeeId)
                .FirstOrDefaultAsync(cancellationToken);
            if (employee == null)
            {
                return Guid.Empty;
            }
            var employeePosition = await _applicationDbContext.EmployeePositions
                    .Where(p => p.Id == request.EmployeePositionId)
                .FirstOrDefaultAsync(cancellationToken);
            if (employeePosition == null)
            {
                return Guid.Empty;
            }
            var employeeContract = new EmployeeContract
            {
                Id = Guid.NewGuid(),
                Type = request.Type,
                StartDate = request.StartDate,
                ActiveFlag = true,
                Employee = employee,
                EmployeePosition = employeePosition
    };

            _applicationDbContext.Add(employeeContract);
            await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return employeeContract.Id;
        }
    }
}
public class EmployeeContractEndpoint : CarterModule
{
    public EmployeeContractEndpoint() : base("/api/employeeContract")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("", async (CreateEmployeeContractRequest request, ISender sender, HttpContext context) =>
        {
            var command = new CreateEmployeeContract.Command
            {
                Type = request.Type,
                StartDate = request.StartDate,
                EmployeeId = request.EmployeeId,
                EmployeePositionId = request.EmployeePositionId
            };

            var result = await sender.Send(command).ConfigureAwait(false);

            if (result != Guid.Empty)
            {
                return Results.Created($"/api/employeeContract/{result}", result);
            }
            else
            {
                return Results.BadRequest("Invalid request or validation failed.");
            }
        });
    }
}