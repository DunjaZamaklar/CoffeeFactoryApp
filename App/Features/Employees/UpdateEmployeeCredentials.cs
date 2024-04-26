using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using static App.Features.Employees.UpdateEmployeeCredentials;
using CSharpFunctionalExtensions;

namespace App.Features.Employees;
public static class UpdateEmployeeCredentials
{
    public class Command : IRequest<Result<Guid, IEnumerable<string>>>
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? UpdatedUsername { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Password).NotEmpty();
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

            var employeeResponse = await _applicationDbContext.Employees
                .Where(p => p.Username == request.Username)
                .Select(p => new EmployeeResponse
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Address = p.Address,
                    City = p.City,
                    Country = p.Country,
                    PhoneNumber = p.PhoneNumber,
                    Email = p.Email,
                    Username = p.Username,
                    Status = p.Status,
                    Role = p.Role
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (employeeResponse is null)
            {
                return Result.Failure<Guid, IEnumerable<string>>(new List<string> { "Invalid employeeId value" });
            }
            if (request.UpdatedUsername == null || request.UpdatedUsername == "") {
                request.UpdatedUsername = employeeResponse.Username;
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
                Username = request.UpdatedUsername,
                Password = request.Password,
                Status = employeeResponse.Status,
                Role = employeeResponse.Role
            };


            // Attach the updated entity
            _applicationDbContext.Attach(updatedEmployee);

            // Set the entity state to modified
            _applicationDbContext.Entry(updatedEmployee).State = EntityState.Modified;

            // Save changes to the database
            await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return employeeResponse.Id;

        }
    }
}
public class UpdateEmployeePasswordEndpoint : CarterModule
{
    public UpdateEmployeePasswordEndpoint() : base("/api/employee")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("credentials/{username}", async (string Username, UpdatePasswordEmployeeRequest request, ISender sender, HttpContext context) =>
        {
            var command = new UpdateEmployeeCredentials.Command
            {
                Username = Username,
                UpdatedUsername = request.Username,
                Password = request.Password,
            };

            var result = await sender.Send(command).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                return Results.Created();
            }
            else
            {
                return Results.BadRequest(result.Error);
            }
        }).RequireAuthorization("UserPolicy");
    }
}