using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using static App.Features.Employees.UpdateEmployeeRole;
using CSharpFunctionalExtensions;

namespace App.Features.Employees;
public static class UpdateEmployeeRole
{
    public class Command : IRequest<Result<EmployeeResponse, IEnumerable<string>>>
    {
        public Guid Id { get; set; } 
        public Guid RoleId { get; set; } 
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.RoleId).NotEmpty();
        }
    }
    internal sealed class Handler : IRequestHandler<Command, Result<EmployeeResponse, IEnumerable<string>>>
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IValidator<Command> _validator;
        public Handler(ApplicationDbContext applicationDbContext, IValidator<Command> validator)
        {
            _applicationDbContext = applicationDbContext;
            _validator = validator;
        }
        public async Task<Result<EmployeeResponse, IEnumerable<string>>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return Result.Failure<EmployeeResponse, IEnumerable<string>>(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var newRole = await _applicationDbContext.UserRoles
                  .Where(p => p.Id == request.RoleId)
             .FirstOrDefaultAsync(cancellationToken);

            if(newRole == null)
            {
                return Result.Failure<EmployeeResponse, IEnumerable<string>>(new List<string> { "Ivalid RoleId value!"}) ;
            }

            var employeeResponse = await _applicationDbContext.Employees
                .Where(p => p.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            _applicationDbContext.Entry(employeeResponse).State = EntityState.Detached;

            if (employeeResponse is null)
            {
                return Result.Failure<EmployeeResponse, IEnumerable<string>>(new List<string> { "Invalid employeeId value" });
            }

            var updatedEmployee = new Employee
            {
                Id = employeeResponse.Id,
                FirstName = employeeResponse.FirstName,
                LastName = employeeResponse.LastName,
                Address = employeeResponse.Address,
                City = employeeResponse.City,
                Country = employeeResponse.Country,
                PhoneNumber = employeeResponse.PhoneNumber,
                Email = employeeResponse.Email,
                Username = employeeResponse.Username,
                Password = employeeResponse.Password,
                Status = employeeResponse.Status,
                Role = newRole
            };

            var updatedEmployeeResponse = new EmployeeResponse
            {
                Id = employeeResponse.Id,
                FirstName = employeeResponse.FirstName,
                LastName = employeeResponse.LastName,
                Address = employeeResponse.Address,
                City = employeeResponse.City,
                Country = employeeResponse.Country,
                PhoneNumber = employeeResponse.PhoneNumber,
                Email = employeeResponse.Email,
                Username = employeeResponse.Username,
                Status = employeeResponse.Status,
                Role = newRole
            };
            // Attach the updated entity
            _applicationDbContext.Attach(updatedEmployee);

            // Set the entity state to modified
            _applicationDbContext.Entry(updatedEmployee).State = EntityState.Modified;

            // Save changes to the database
            await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return updatedEmployeeResponse;

        }
    }
}
public class UpdateEmployeeRoleEndpoint : CarterModule
{
    public UpdateEmployeeRoleEndpoint() : base("/api/employee")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("role/{Id}", async (Guid Id, UpdateRoleEmployeeRequest request, ISender sender, HttpContext context) =>
        {
            var command = new UpdateEmployeeRole.Command
            {
                RoleId = request.RoleId,
                Id = Id
            };

            var result = await sender.Send(command).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                return Results.Created($"/api/employee/{result.Value}", result.Value);
            }
            else
            {
                return Results.BadRequest(result.Error);
            }
        }).RequireAuthorization("AdminPolicy");
    }
}