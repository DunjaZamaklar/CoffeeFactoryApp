﻿using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.EmployeePositions.UpdateEmployeePosition;

namespace App.Features.EmployeePositions;
public static class UpdateEmployeePosition
{
    public class Command : IRequest<Result<EmployeePositionResponse, IEnumerable<string>>>
    {
        public Guid Id { get; set; }
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
    internal sealed class Handler : IRequestHandler<Command, Result<EmployeePositionResponse, IEnumerable<string>>>
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IValidator<Command> _validator;
        public Handler(ApplicationDbContext applicationDbContext, IValidator<Command> validator)
        {
            _applicationDbContext = applicationDbContext;
            _validator = validator;
        }
        public async Task<Result<EmployeePositionResponse, IEnumerable<string>>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return Result.Failure<EmployeePositionResponse, IEnumerable<string>>(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var employeePositionResponse = await _applicationDbContext.EmployeePositions
                .Where(p => p.Id == request.Id)
                .Select(p => new EmployeePositionResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (employeePositionResponse is null)
            {
                return Result.Failure<EmployeePositionResponse, IEnumerable<string>>(new List<string> { "Invalid employeePositionId"});
            }

            var updatedEmployeePosition = new EmployeePosition
            {
                Id = employeePositionResponse.Id,
                Name = request.Name,
                Description = request.Description
            };

            var updatedEmployeePositionResponse = new EmployeePositionResponse
            {
                Id = employeePositionResponse.Id,
                Name = request.Name,
                Description = request.Description
            };

            // Attach the updated entity
            _applicationDbContext.Attach(updatedEmployeePosition);

            // Set the entity state to modified
            _applicationDbContext.Entry(updatedEmployeePosition).State = EntityState.Modified;

            // Save changes to the database
            await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return updatedEmployeePositionResponse;

        }
    }
}
public class UpdateEmployeePositionEndpoint : CarterModule
{
    public UpdateEmployeePositionEndpoint() : base("/api/employeePosition")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id}", async (Guid id, CreateEmployeePositionRequest request, ISender sender, HttpContext context) =>
        {
            var command = new UpdateEmployeePosition.Command
            {
                Id = id,
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