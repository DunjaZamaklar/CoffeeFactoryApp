using App.Contracts;
using App.Data.Database;
using Carter;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.Suppliers.DeleteSupplier;

namespace App.Features.Suppliers;

public static class DeleteSupplier
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
            var entityToDelete = await _applicationDbContext.Suppliers.FindAsync(request.Id);
            if (entityToDelete != null)
            {
                try
                {
                    _applicationDbContext.Suppliers.Remove(entityToDelete);
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

public class DeleteSupplierEndpoint : CarterModule
{
    public DeleteSupplierEndpoint() : base("/api/supplier")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id}", async (Guid id, ISender sender) =>
        {
            var query = new DeleteSupplier.Query { Id = id };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return Results.Ok();
            }
            else
            {
                return Results.BadRequest(result.Error);
            }
        });
    }
}