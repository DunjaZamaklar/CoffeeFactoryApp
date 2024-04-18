using App.Contracts;
using App.Data.Database;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.Employees.DeleteEmployee;

namespace App.Features.Employees;

public static class DeleteEmployee
{

    public class Query : IRequest<Boolean>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, Boolean>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<Boolean> Handle(Query request, CancellationToken cancellationToken)
        {
            var entityToDelete = await _applicationDbContext.Employees.FindAsync(request.Id);
            if (entityToDelete != null)
            {
                _applicationDbContext.Employees.Remove(entityToDelete);
                // Save changes to the database
                await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
            return false;
        }
    }
}

public class DeleteEmployeeEndpoint : CarterModule
{
    public DeleteEmployeeEndpoint() : base("/api/employee")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id}", async (Guid id, ISender sender) =>
        {
            var query = new DeleteEmployee.Query { Id = id };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result)
            {
                return Results.Ok();
            }
            else
            {
                return Results.NotFound();
            }
        });
    }
}