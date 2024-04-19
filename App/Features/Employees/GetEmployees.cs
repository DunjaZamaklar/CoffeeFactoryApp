using App.Contracts;
using App.Data.Database;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.Employees.GetEmployees;

namespace App.Features.Employees;

public static class GetEmployees
{
    public class Query : IRequest<List<EmployeeResponse>>
    {
    }

    internal sealed class Handler : IRequestHandler<Query, List<EmployeeResponse>>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<List<EmployeeResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var employeeResponse = await _applicationDbContext.Employees
                .Select(p => new EmployeeResponse
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Address = p.Address,
                    City = p.City,
                    Country = p.Country,
                    PhoneNumber = p.PhoneNumber,
                    Email = p.Email,
                    Username = p.Username,
                    Status = p.Status
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return employeeResponse;
        }
    }
}

public class GetAllEmployeesEndpoint : CarterModule
{
    public GetAllEmployeesEndpoint() : base("/api/employee")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("", async (ISender sender) =>
        {
            var query = new GetEmployees.Query { };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result is null)
            {
                return null;
            }
            return result;
        });
    }
}