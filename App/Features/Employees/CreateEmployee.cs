using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using FluentValidation;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using static App.Features.Employees.CreateEmployee;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace App.Features.Employees;
public static class CreateEmployee
{
    public class Command : IRequest<Result<Guid, IEnumerable<string>>>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        [PasswordPropertyText]
        public string Password { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Password).NotEmpty();
            RuleFor(c => c.Email).EmailAddress();
            RuleFor(c => c.FirstName).NotEmpty();
            RuleFor(c => c.LastName).NotEmpty();
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

            var employeeRole = await _applicationDbContext.UserRoles
                  .Where(p => p.Name == "User")
             .FirstOrDefaultAsync(cancellationToken);

            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Address = request.Address,
                City = request.City,
                Country = request.Country,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Username = request.Username,
                Password = request.Password,
                Status = request.Status,
                Role = employeeRole
            };

            _applicationDbContext.Add(employee);
            await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return employee.Id;
        }
    }
}
public class EmployeeEndpoint : CarterModule
{
    public EmployeeEndpoint() : base("/api/employee")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("", async (CreateEmployeeRequest request, ISender sender, HttpContext context) =>
        {
            var command = new CreateEmployee.Command
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Address = request.Address,
                City = request.City,
                Country = request.Country,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Username = request.Username,
                Password = request.Password,
                Status = request.Status
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