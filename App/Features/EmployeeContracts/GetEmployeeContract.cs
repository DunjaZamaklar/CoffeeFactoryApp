using App.Contracts;
using App.Data.Database;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.EmployeeContracts.GetEmployeeContract;

namespace App.Features.EmployeeContracts;

public static class GetEmployeeContract
{
    public class Query : IRequest<EmployeeContractResponse>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, EmployeeContractResponse>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<EmployeeContractResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            var employeeContarctResponse = await _applicationDbContext.EmployeeContracts
                .Where(p => p.Id == request.Id)
                .Select(p => new EmployeeContractResponse
                {
                    Id = p.Id,
                    Type = p.Type,
                    ActiveFlag = p.ActiveFlag,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Employee = p.Employee,
                    EmployeePosition = p.EmployeePosition
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (employeeContarctResponse is null)
            {
                return null;
            }
            return employeeContarctResponse;
        }
    }
}

public class GetEmployeeContractEndpoint : CarterModule
{
    public GetEmployeeContractEndpoint() : base("/api/employeeContract")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id}", async (Guid id, ISender sender) =>
        {
            var query = new GetEmployeeContract.Query { Id = id };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result is null)
            {
                return null;
            }
            return result;
        });
    }
}