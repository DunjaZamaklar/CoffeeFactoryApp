using App.Contracts;
using App.Data.Database;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.EmployeePositions.GetEmployeePositions;

namespace App.Features.EmployeePositions;

public static class GetEmployeePositions
{
    public class Query : IRequest<List<EmployeePositionResponse>>
    {
    }

    internal sealed class Handler : IRequestHandler<Query, List<EmployeePositionResponse>>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<List<EmployeePositionResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var employeePositionsResponse = await _applicationDbContext.EmployeePositions
                .Select(p => new EmployeePositionResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return employeePositionsResponse;
        }
    }
}

public class GetAllEmployeePositionsEndpoint : CarterModule
{
    public GetAllEmployeePositionsEndpoint() : base("/api/employeePosition")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("", async (ISender sender) =>
        {
            var query = new GetEmployeePositions.Query { };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result is null)
            {
                return null;
            }
            return result;
        });
    }
}