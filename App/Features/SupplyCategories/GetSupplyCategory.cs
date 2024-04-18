using App.Contracts;
using App.Data.Database;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.SupplyCategories.GetSupplyCategory;

namespace App.Features.SupplyCategories;

public static class GetSupplyCategory
{
    public class Query : IRequest<SupplyCategoryResponse>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, SupplyCategoryResponse>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<SupplyCategoryResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            var supplyCategoryResponse = await _applicationDbContext.SupplyCategories
                .Where(p => p.Id == request.Id)
                .Select(p => new SupplyCategoryResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (supplyCategoryResponse is null)
            {
                return null;
            }
            return supplyCategoryResponse;
        }
    }
}

public class GetSupplyCategoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("supplyCategory/{id}", async (Guid id, ISender sender) =>
        {
            var query = new GetSupplyCategory.Query { Id = id };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result is null)
            {
                return null;
            }
            return result;
        });
    }
}