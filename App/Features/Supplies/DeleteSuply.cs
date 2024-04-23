using App.Contracts;
using App.Data.Database;
using Carter;
using CSharpFunctionalExtensions;
using CSharpFunctionalExtensions.ValueTasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.Supplies.DeleteSuply;

namespace App.Features.Supplies;

public static class DeleteSuply
{

    public class Query : IRequest<Result<Boolean, IEnumerable<string>>>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, Result<Boolean, IEnumerable<string>>>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<Result<Boolean, IEnumerable<string>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var entityToDelete = await _applicationDbContext.Supplies.FindAsync(request.Id);
            if (entityToDelete != null)
            {
                try
                {
                    _applicationDbContext.Supplies.Remove(entityToDelete);
                    await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return Result.Failure<Boolean, IEnumerable<string>>(new List<string> { ex.Message });
                }

                return true;
            }
            return Result.Failure<Boolean, IEnumerable<string>>(new List<string> { "Invalid supplyId value" });
        }
    }
}

public class DeleteSupplyEndpoint : CarterModule
{
    public DeleteSupplyEndpoint() : base("/api/supply")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id}", async (Guid id, ISender sender) =>
        {
            var query = new DeleteSuply.Query { Id = id };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return Results.Ok();
            }
            else
            {
                return Results.NotFound(result.Error);
            }
        });
    }
}