using App.Contracts;
using App.Data.Database;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.EmployeeContracts.DeleteEmployeeContract;

namespace App.Features.EmployeeContracts;

public static class DeleteEmployeeContract
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
            var entityToDelete = await _applicationDbContext.EmployeeContracts.FindAsync(request.Id);
            if (entityToDelete != null)
            {
                _applicationDbContext.EmployeeContracts.Remove(entityToDelete);
                // Save changes to the database
                await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
            return false;
        }
    }
}

public class DeleteEmployeeContractEndpoint : CarterModule
{
    public DeleteEmployeeContractEndpoint() : base("/api/employeeContract")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id}", async (Guid id, ISender sender) =>
        {
            var query = new DeleteEmployeeContract.Query { Id = id };
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