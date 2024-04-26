using App.Contracts;
using App.Data.Database;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.SupplyCategories.GetSupplyCategories;

namespace App.Features.SupplyCategories;

public static class GetSupplyCategories
{
    public class Query : IRequest<List<SupplyCategoryResponse>>
    {
    }

    internal sealed class Handler : IRequestHandler<Query, List<SupplyCategoryResponse>>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<List<SupplyCategoryResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var supplyCategoryResponses = await _applicationDbContext.SupplyCategories
                .Select(p => new SupplyCategoryResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return supplyCategoryResponses;
        }
    }
}

public class GetAllSupplyCategoriesEndpoint : CarterModule
{
    public GetAllSupplyCategoriesEndpoint() : base("/api/supplyCategory")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("", async (ISender sender) =>
        {
            var query = new GetSupplyCategories.Query { };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result is null)
            {
                return null;
            }
            return result;
        }).RequireAuthorization("UserPolicy");
    }
}