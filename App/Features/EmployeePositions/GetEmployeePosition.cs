using App.Contracts;
using App.Data.Database;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.EmployeePositions.GetEmployeePosition;

namespace App.Features.EmployeePositions;

public static class GetEmployeePosition
{
    public class Query : IRequest<EmployeePositionResponse>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, EmployeePositionResponse>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<EmployeePositionResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            var employeePositionResponse = await _applicationDbContext.EmployeePositions
                .Where(p => p.Id == request.Id)
                .Select(p => new EmployeePositionResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (employeePositionResponse is null)
            {
                return null;
            }
            return employeePositionResponse;
        }
    }
}

public class GetEmployeePositionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("employeePosition/{id}", async (Guid id, ISender sender) =>
        {
            var query = new GetEmployeePosition.Query { Id = id };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result is null)
            {
                return null;
            }
            return result;
        });
    }
}