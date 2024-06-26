﻿using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using static App.Features.Employees.UpdateEmployee;
using CSharpFunctionalExtensions;

namespace App.Features.Employees;
public static class UpdateEmployee
{
    public class Command : IRequest<Result<EmployeeResponse, IEnumerable<string>>>
    {
        public Guid Id { get; set; }
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

            var employeeResponse = await _applicationDbContext.Employees
                .Where(p => p.Id == request.Id)
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
                return Result.Failure<EmployeeResponse, IEnumerable<string>>(new List<string> { "Invalid employeeId"});
            }

            var updatedEmployee = new Employee
            {
                Id = employeeResponse.Id,
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
                Role = employeeResponse.Role
            };

            var updatedEmployeeResponse = new EmployeeResponse 
            {
                Id = employeeResponse.Id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Address = request.Address,
                City = request.City,
                Country = request.Country,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Username = request.Username,
                Status = request.Status,
                Role = employeeResponse.Role
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
public class UpdateEmployeeEndpoint : CarterModule
{
    public UpdateEmployeeEndpoint() : base("/api/employee")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id}", async (Guid id, CreateEmployeeRequest request, ISender sender, HttpContext context) =>
        {
            var command = new UpdateEmployee.Command
            {
                Id = id,
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
        }).RequireAuthorization("SuperUserPolicy");
    }
}