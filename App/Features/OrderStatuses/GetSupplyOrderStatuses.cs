using App.Contracts;
using App.Data.Database;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.OrderStatuses.CreateSupplyOrderStatus;

namespace App.Features.OrderStatuses;

public static class GetSupplyOrderStatuses
{
    public class Query : IRequest<List<SupplyOrderStatusResponse>>
    {
    }

    internal sealed class Handler : IRequestHandler<Query, List<SupplyOrderStatusResponse>>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<List<SupplyOrderStatusResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var supplyOrderStatusResponse = await _applicationDbContext.SupplyOrderStatus
                .Select(p => new SupplyOrderStatusResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return supplyOrderStatusResponse;
        }
    }
}

public class GetAllSupplyOrderStatusEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("supplyOrderStatus", async (ISender sender) =>
        {
            var query = new GetSupplyOrderStatuses.Query { };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result is null)
            {
                return null;
            }
            return result;
        });
    }
}