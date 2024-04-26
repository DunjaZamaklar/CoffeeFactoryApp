using App.Contracts;
using App.Data.Database;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.EmployeeContracts.GetEmployeeContracts;

namespace App.Features.EmployeeContracts;

public static class GetEmployeeContracts
{
    public class Query : IRequest<List<EmployeeContractResponse>>
    {
    }

    internal sealed class Handler : IRequestHandler<Query, List<EmployeeContractResponse>>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<List<EmployeeContractResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var employeeContractsResponse = await _applicationDbContext.EmployeeContracts
                .Select(p => new EmployeeContractResponse
                {
                    Id = p.Id,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Type = p.Type,
                    ActiveFlag = p.ActiveFlag,
                    Employee = p.Employee,
                    EmployeePosition = p.EmployeePosition
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return employeeContractsResponse;
        }
    }
}

public class GetAllEmployeeContractsEndpoint : CarterModule
{
    public GetAllEmployeeContractsEndpoint() : base("/api/employeeContracts")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("", async (ISender sender) =>
        {
            var query = new GetEmployeeContracts.Query { };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result is null)
            {
                return null;
            }
            return result;
        }).RequireAuthorization("SuperUserPolicy");
    }
}