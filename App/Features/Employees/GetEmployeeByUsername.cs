using App.Contracts;
using App.Data.Database;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.Employees.GetEmployeeByUsername;

namespace App.Features.Employees;

public static class GetEmployeeByUsername
{
    public class Query : IRequest<EmployeeResponse>
    {
        public string username { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, EmployeeResponse>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<EmployeeResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            var employeeResponse = await _applicationDbContext.Employees
                .Where(p => p.Username == request.username)
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
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (employeeResponse is null)
            {
                return null;
            }
            return employeeResponse;
        }
    }
}

public class GetEmployeeByUsernameEndpoint : CarterModule
{
    public GetEmployeeByUsernameEndpoint() : base("/api/employee")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/username/{username}", async (string username, ISender sender) =>
        {
            var query = new GetEmployeeByUsername.Query { username = username };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result is null)
            {
                return null;
            }
            return result;
        });
    }
}