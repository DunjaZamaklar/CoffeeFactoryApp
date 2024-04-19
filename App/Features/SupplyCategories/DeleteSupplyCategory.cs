using App.Contracts;
using App.Data.Database;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.SupplyCategories.DeleteSupplyCategory;

namespace App.Features.SupplyCategories;

public static class DeleteSupplyCategory
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
            var entityToDelete = await _applicationDbContext.SupplyCategories.FindAsync(request.Id);
            if (entityToDelete != null)
            {
                _applicationDbContext.SupplyCategories.Remove(entityToDelete);
                // Save changes to the database
                await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
            return false;
        }
    }
}

public class DeleteSupplyCatgoryEndpoint : CarterModule
{
    public DeleteSupplyCatgoryEndpoint() : base("/api/supplyCategory")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id}", async (Guid id, ISender sender) =>
        {
            var query = new DeleteSupplyCategory.Query { Id = id };
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