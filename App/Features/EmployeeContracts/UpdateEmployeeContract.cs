using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.EmployeeContracts.UpdateEmployeeContract;

namespace App.Features.EmployeeContracts;
public static class UpdateEmployeeContract
{
    public class Command : IRequest<EmployeeContractResponse>
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
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
            RuleFor(c => c.EndDate)
            .GreaterThan(c => c.StartDate).WithMessage("End date must be after start date.");
            RuleFor(c => c.StartDate)
             .NotEmpty().WithMessage("Start Date is required.")
            .Must(date => date <= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Start date cannot be after today's date.");
        }
    }
    internal sealed class Handler : IRequestHandler<Command, EmployeeContractResponse>
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IValidator<Command> _validator;
        public Handler(ApplicationDbContext applicationDbContext, IValidator<Command> validator)
        {
            _applicationDbContext = applicationDbContext;
            _validator = validator;
        }
        public async Task<EmployeeContractResponse> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return null;
            }
            var employee = await _applicationDbContext.Employees
                    .Where(p => p.Id == request.EmployeeId)
                .FirstOrDefaultAsync(cancellationToken);
            if (employee == null)
            {
                return null;
            }
            var employeePosition = await _applicationDbContext.EmployeePositions
                    .Where(p => p.Id == request.EmployeePositionId)
                .FirstOrDefaultAsync(cancellationToken);
            if (employeePosition == null)
            {
                return null;
            }
            var employeeContractResponse = await _applicationDbContext.EmployeeContracts
                .Where(p => p.Id == request.Id)
                .Select(p => new EmployeeContract
                {
                    Id = p.Id,
                    Type = p.Type,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    ActiveFlag = p.ActiveFlag,
                    Employee = p.Employee,
                    EmployeePosition = p.EmployeePosition
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (employeeContractResponse is null)
            {
                return null;
            }

            var updatedEmployeeContract = new EmployeeContract
            {
                Id = employeeContractResponse.Id,
                Type = request.Type,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ActiveFlag = request.ActiveFlag,
                Employee = employee,
                EmployeePosition = employeePosition
            };

            var updatedEmployeeContractResponse = new EmployeeContractResponse
            {
                Id = employeeContractResponse.Id,
                Type = request.Type,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ActiveFlag = request.ActiveFlag,
                Employee = employee,
                EmployeePosition = employeePosition
            };

            // Attach the updated entity
            _applicationDbContext.Attach(updatedEmployeeContract);

            // Set the entity state to modified
            _applicationDbContext.Entry(updatedEmployeeContract).State = EntityState.Modified;

            // Save changes to the database
            await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return updatedEmployeeContractResponse;

        }
    }
}
public class UpdateEmployeeContractEndpoint : CarterModule
{
    public UpdateEmployeeContractEndpoint() : base("/api/employeeContract")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id}", async (Guid id, UpdateEmployeeContractRequest request, ISender sender, HttpContext context) =>
        {
            var command = new UpdateEmployeeContract.Command
            {
                Id = id,
                Type = request.Type,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ActiveFlag = request.ActiveFlag,
                EmployeeId = request.EmployeeId,
                EmployeePositionId = request.EmployeePositionId
            };

            var result = await sender.Send(command).ConfigureAwait(false);

            if (result != null)
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