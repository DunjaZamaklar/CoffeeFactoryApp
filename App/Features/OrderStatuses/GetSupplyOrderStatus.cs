using App.Contracts;
using App.Data.Database;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.OrderStatuses.GetSupplyOrderStatus;

namespace App.Features.OrderStatuses;

public static class GetSupplyOrderStatus
{
    public class Query : IRequest<SupplyOrderStatusResponse>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, SupplyOrderStatusResponse>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<SupplyOrderStatusResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            var supplyOrderStatusResponse = await _applicationDbContext.SupplyOrderStatus
                .Where(p => p.Id == request.Id)
                .Select(p => new SupplyOrderStatusResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (supplyOrderStatusResponse is null)
            {
                return null;
            }
            return supplyOrderStatusResponse;
        }
    }
}

public class GetSupplyOrderStatusEndpoint : CarterModule
{
    public GetSupplyOrderStatusEndpoint() : base("/api/supplyOrderStatus")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id}", async (Guid id, ISender sender) =>
        {
            var query = new GetSupplyOrderStatus.Query { Id = id };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result is null)
            {
                return null;
            }
            return result;
        });
    }
}